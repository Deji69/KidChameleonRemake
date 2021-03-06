﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Collision;

namespace Engine
{
    public static class ICollidableExtensions
    {

        public static void HandleCollision(this ICollidable obj, CollisionEvent collision, CollisionResponse response)
        {
            foreach (var handler in obj.CollisionResponders)
            {
                handler.HandleCollision(collision, response);
                if (!response.ShouldContinueHandling)
                    return;
            }
        }

        public static void AddCollisionChecks<T>(this T collidable, params ObjectType[] collisionTypes) where T : LogicObject, ICollidable
        {
            CollisionManager<T> m = null;

            if (collisionTypes.Contains(ObjectType.Block))
                m = new Collision.TileCollisionManager<T>(collidable);

            if (collisionTypes.Contains(ObjectType.Border))
                m = new Collision.LevelBorderCollisionManager<T>(collidable, DirectionFlags.Horizontal);


            var obj = collisionTypes.Where(p => p.Is(ObjectType.Thing)).ToArray();
            if (obj.Length > 0)
            {
                m = new Collision.ObjectCollisionManager<T>(collidable, obj);
                if(collidable.Context.Listeners != null)
                    collidable.GetWorld().CollisionInfo.CollisionListener.Register(collidable);
            }

            collidable.CollisionTypes = collisionTypes;
        }

        public static bool CanCollideWith(this ICollidable objectA, ICollidable objectB)
        {
            return objectA.CanCollideWith(objectB, true);
        }

        private static bool CanCollideWith(this ICollidable objectA, ICollidable objectB, bool checkReverse)
        {
            bool canCollide = false;
            foreach (var cType in objectB.CollisionTypes)
            {
                if (objectA.ObjectType.Is(cType))
                {
                    canCollide = true;
                    break;
                }
            }

            if (!canCollide)
                return false;

            if (checkReverse)
                return objectB.CanCollideWith(objectA, false);

            return true;
        }

        public static World GetWorld(this ICollidable obj)
        {
            return obj.Context.CurrentWorld;
        }
    
    }


}

namespace Engine.Collision
{
    public enum Side
    {
        None,
        Top,
        Left,
        Bottom,
        Right
    }

    public static class SideExtensions
    {

        public static RGPointI ToOffset(this Side s)
        {
            switch (s)
            {
                case Side.Top: return new RGPointI(0, -1);
                case Side.Left: return new RGPointI(-1, 0);
                case Side.Bottom: return new RGPointI(0,1);
                case Side.Right: return new RGPointI(1, 0);
                default: return RGPointI.Empty;
            }
        }

        public static Side Opposite(this Side s)
        {
            switch (s)
            {
                case Side.Top: return Side.Bottom;
                case Side.Left: return Side.Right;
                case Side.Bottom: return Side.Top;
                case Side.Right: return Side.Left;
                default: return Side.None;
            }
        }

        public static Direction ToDirection(this Side s)
        {
            switch (s)
            {
                case Side.Top: return Direction.Up;
                case Side.Left: return Direction.Left;
                case Side.Right: return Direction.Right;
                case Side.Bottom: return Direction.Down;
                default: return Direction.Default;
            }
        }
    
    }

    struct CorrectionRec
    {
        public Side Side;
        public RGRectangleI Rec;
    }

    public interface ICollidable : IWithPosition, IMoveable 
    {
        bool Alive { get; }
        LayerDepth LayerDepth { get; }
        ObjectType ObjectType { get; }
        ObjectType[] CollisionTypes { get; set; }
        RGRectangleI SecondaryCollisionArea { get; }
        ICollection<ICollisionResponder> CollisionResponders { get; }
    }

  

    public class CollisionEvent
    {
        private ICollidable mThisPosition;
        private ICollidable mOtherPosition;

        public ulong CollisionTime { get; private set; }

        #region Collision Values

        public RGRectangleI ThisCollisionTimeArea { get; private set; }
        public RGRectangleI OtherCollisionTimeArea { get; private set; }

        public RGPoint ThisCollisionTimeSpeed { get; private set; }
        public RGPoint OtherCollisionTimeSpeed { get; private set; }

        #endregion

        public ICollidable OtherObject { get { return mOtherPosition; } }

        /// <summary>
        /// Combined speed of the two objects
        /// </summary>
        public RGPoint CollisionSpeed { get; private set; }

        public bool IsBlocking { get; private set; }

        public bool OtherTopExposed { get; private set; }
        public bool OtherLeftExposed { get; private set; }
        public bool OtherRightExposed { get; private set; }
        public bool OtherBottomExposed { get; private set; }

        public RGRectangleI ThisArea { get { return mThisPosition.Area; } }
        public RGRectangleI OtherArea { get { return mOtherPosition.Area; } }

        public ObjectType ThisType { get { return mThisPosition.ObjectType; } }
        public ObjectType OtherType { get { return mOtherPosition.ObjectType; } }

        public HitboxType ThisHitboxType { get; private set; }
        public HitboxType OtherHitboxType { get; private set; }

        public RGLine OtherTop { get { if (OtherTopExposed) return OtherArea.TopSide; else return new RGLine(OtherArea.TopLeft, OtherArea.TopLeft); } }
        public RGLine OtherBottom { get { if (OtherBottomExposed) return OtherArea.BottomSide; else return new RGLine(OtherArea.BottomLeft, OtherArea.BottomLeft); } }
        public RGLine OtherLeft { get { if (OtherLeftExposed) return OtherArea.LeftSide; else return new RGLine(OtherArea.TopLeft, OtherArea.TopLeft); } }
        public RGLine OtherRight { get { if (OtherRightExposed) return OtherArea.RightSide; else return new RGLine(OtherArea.TopRight, OtherArea.TopRight); } }

        /// <summary>
        /// Side of the this object that collided
        /// </summary>
        public Side CollisionSide { get; set; }

        public RGPoint SlopeIntersectionPoint { get; set; }
        public bool IsSloped { get { return !SlopeIntersectionPoint.IsEmpty; } }
        public DirectionFlags SlopeDirection { get; set; }

        /// <summary>
        /// Either returns this event or its inversion, so that the given object is the primary one. Assumes that the given object is part of this event.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public CollisionEvent AdjustTo(ICollidable obj)
        {
            if (obj.Equals(mThisPosition))
                return this;
            else
                return this.Invert();
        }

        public CollisionEvent Invert()
        {
            var c = new CollisionEvent(mOtherPosition, mThisPosition, true, true, true, true, this.IsBlocking, this.OtherHitboxType,this.ThisHitboxType);
            switch (this.CollisionSide)
            {
                case Side.Left: c.CollisionSide = Side.Right; break;
                case Side.Right: c.CollisionSide = Side.Left; break;
                case Side.Top: c.CollisionSide = Side.Bottom; break;
                case Side.Bottom: c.CollisionSide = Side.Top; break;
            }

            c.ThisCollisionTimeArea = this.OtherCollisionTimeArea;
            c.ThisCollisionTimeSpeed = this.OtherCollisionTimeSpeed;
            c.OtherCollisionTimeArea = this.ThisCollisionTimeArea;
            c.OtherCollisionTimeSpeed = this.ThisCollisionTimeSpeed;



            return c;
        }

        public CollisionEvent(ICollidable thisPos, ICollidable otherPos, bool topExposed, bool leftExposed, bool rightExposed, bool bottomExposed, bool isBlocking, HitboxType thisHitboxType, HitboxType otherHitboxType)
        {
            this.CollisionTime = thisPos.Context.CurrentFrameNumber;
            this.OtherLeftExposed = leftExposed;
            this.OtherRightExposed = rightExposed;
            this.OtherTopExposed = topExposed;
            this.OtherBottomExposed = bottomExposed;

            mThisPosition = thisPos;
            mOtherPosition = otherPos;

            this.IsBlocking = isBlocking;

            this.ThisCollisionTimeArea = ThisArea;
            this.ThisCollisionTimeSpeed = mThisPosition.MotionManager.MotionOffset;
            this.OtherCollisionTimeArea = OtherArea;
            this.OtherCollisionTimeSpeed = mOtherPosition.MotionManager.MotionOffset;

            this.ThisHitboxType = thisHitboxType;
            this.OtherHitboxType =otherHitboxType;
        }

        public void CalcCollisionSpeed(ICollidable obj)
        {
            this.CollisionSpeed = obj.MotionManager.MotionOffset.Offset(this.OtherObject.MotionManager.MotionOffset);
        }

        public RGRectangleI ThisObjectPreviousPosition
        {
            get
            {
                var speed = ThisCollisionTimeSpeed;
                speed = speed.Scale(-1f, -1f);
                return ThisCollisionTimeArea.Offset(speed.ToPointI());
            }
        }
    }

}
