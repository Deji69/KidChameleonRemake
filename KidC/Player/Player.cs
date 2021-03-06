﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;


namespace KidC
{
    class PlayerSprite : Sprite, IDamageable
    {
        private PlayerHitController mHitController;

        private PlayerSprite(Layer layer, ObjectType objectType) : base(layer, objectType) { }

        private static SpriteCreationInfo CreatePlayer(GameContext ctx, Layer layer, ObjectType playerType, int maxHealth, bool secondaryHitboxIsDamaging)
        {
            var sprite = new PlayerSprite(layer, playerType);
            var spriteInfo = new SpriteCreationInfo(sprite);

            sprite.Location = new RGPointI(120, 125);

            sprite.AddCollisionChecks(ObjectType.Block, ObjectType.Border, KCObjectType.Collectable, KCObjectType.Enemy);

            var player = KidCGame.CreatePlayer(ctx);

            var transformationController = spriteInfo.AddBehavior(new TransformationController(sprite, maxHealth));


            var gravityCtl = new GravityController(sprite);
            spriteInfo.AddBehavior(new PlayerHitController(sprite, secondaryHitboxIsDamaging, gravityCtl, new HealthController(sprite, 9999)));
            var playerCtl = spriteInfo.AddBehavior(new PlatformerPlayerController(sprite, player, gravityCtl));
            new PlatformerBlockBreaker(sprite);
            new IceWalkController(playerCtl);
            new PlayerBounceController(sprite, playerCtl);
            new StompDamager(sprite, gravityCtl);
         
            sprite.mHitController = spriteInfo.GetBehavior<PlayerHitController>();
            return spriteInfo;
        }

        public static SpriteCreationInfo CreateKid(Layer layer)
        {
            var ctx = layer.Context;
            var playerInfo = CreatePlayer(ctx, layer, KCObjectType.JamesKid, 2, false);
            var player = playerInfo.Sprite;
            var flipController = new KidFlipController(player, ctx.FirstPlayer);

            new BehaviorExclusionController(player, flipController, playerInfo.GetBehavior<PlatformerPlayerController>());

            var spriteSheet = GameResource<SpriteSheet>.Load(new GamePath(PathType.SpriteSheets, "kid"), ctx);
            player.AddAnimation(KCAnimation.Stand, new Animation(spriteSheet, Direction.Right, 0));
            player.AddAnimation(KCAnimation.Walk, new Animation(spriteSheet, Direction.Right, 1, 2, 3, 4, 5, 6));
            player.GetAnimation(KCAnimation.Walk).SetFrameDuration(6);

            player.AddAnimation(KCAnimation.Turn, new Animation(spriteSheet, Direction.Left, 12));

            player.AddAnimation(KCAnimation.Jump, new Animation(spriteSheet, Direction.Right, 10));
            player.AddAnimation(KCAnimation.Fall, new Animation(spriteSheet, Direction.Right, 11)).SetFrameDuration(6);

            player.AddAnimation(KCAnimation.ClimbUp, new Animation(spriteSheet, Direction.Right, 13, 14, 15, 16, 17, 18)).SetFrameDuration(5);
            player.AddAnimation(KCAnimation.ClimbDown, new Animation(spriteSheet, Direction.Left, 19, 20, 21, 22, 23, 24)).SetFrameDuration(5);

            player.AddAnimation(KCAnimation.Crawl, new Animation(spriteSheet, Direction.Right, 7, 8, 9, 8)).SetFrameDuration(10);
            player.AddAnimation(KCAnimation.Flip, new Animation(spriteSheet, Direction.Left, false, 25, 26, 27, 28, 29, 30, 31)).SetFrameDuration(3);
            player.AddAnimation(KCAnimation.Dead, new Animation(spriteSheet, Direction.Right, 32));

            new PlayerDieController(player);

            player.SetAnimation(KCAnimation.Stand);

            return playerInfo;
        }

        public static SpriteCreationInfo CreateIronKnight(Layer layer)
        {
            var ctx = layer.Context;
            var playerInfo = CreatePlayer(ctx, layer, KCObjectType.IronKnight, 4, false);
            var player = playerInfo.Sprite;

            new IronKnightClimbController(player, ctx.FirstPlayer, playerInfo.GetBehavior<PlatformerPlayerController>());

            new IronKnightBrickBreakerController(player);

            var spriteSheet = GameResource<SpriteSheet>.Load(new GamePath(PathType.SpriteSheets, "ironknight"), ctx);
            player.AddAnimation(KCAnimation.Stand, new Animation(spriteSheet, Direction.Left, 0));
            player.AddAnimation(KCAnimation.Walk, new Animation(spriteSheet, Direction.Left, 1, 2, 3, 4, 5, 6)).SetFrameDuration(6);

            player.AddAnimation(KCAnimation.Jump, new Animation(spriteSheet, Direction.Right, 25));
            player.AddAnimation(KCAnimation.Fall, new Animation(spriteSheet, Direction.Right, false, 26, 27)).SetFrameDuration(30);

            player.AddAnimation(KCAnimation.ClimbDown, new Animation(spriteSheet, Direction.Left, 7, 8, 9, 10, 11, 12));
            player.AddAnimation(KCAnimation.ClimbUp, new Animation(spriteSheet, Direction.Right, 16, 17, 18, 19, 20, 21));
            player.GetAnimation(KCAnimation.ClimbUp).SetFrameDuration(5);
            player.GetAnimation(KCAnimation.ClimbDown).SetFrameDuration(5);

            player.AddAnimation(KCAnimation.Crawl, new Animation(spriteSheet, Direction.Right, 13, 14, 15, 14));
            player.GetAnimation(KCAnimation.Crawl).SetFrameDuration(10);

            player.AddAnimation(KCAnimation.TransitionIn, CreateTransitionAnimation(spriteSheet, Direction.Right, 29, 4, 60, false));
            player.AddAnimation(KCAnimation.TransitionOut, CreateTransitionAnimation(spriteSheet, Direction.Right, 29, 4, 30, true));

            player.AddAnimation(KCAnimation.IronKnightClimb, new Animation(spriteSheet, Direction.Right, 22, 23, 24)).SetFrameDuration(10);
            player.SetAnimation(KCAnimation.Stand);

            new StompController(player, Sounds.EnemyBounce);
            return playerInfo;
        }

        public static SpriteCreationInfo CreateRedStealth(Layer layer)
        {
            var ctx = layer.Context;
            var playerInfo = CreatePlayer(ctx, layer, KCObjectType.RedStealth, 3, true);
            var player = playerInfo.Sprite;
            new RedStealthSwordController(player, playerInfo.GetBehavior<PlatformerPlayerController>());
            var spriteSheet = GameResource<SpriteSheet>.Load(new GamePath(PathType.SpriteSheets, "redstealth"), ctx);
            player.AddAnimation(KCAnimation.Stand, new Animation(spriteSheet, Direction.Right, 4));
            player.AddAnimation(KCAnimation.Walk, new Animation(spriteSheet, Direction.Right, 12, 13, 14, 15, 16)).SetFrameDuration(6);

            player.AddAnimation(KCAnimation.Jump, new Animation(spriteSheet, Direction.Right, 8));
            player.AddAnimation(KCAnimation.Fall, new Animation(spriteSheet, Direction.Right, false, 9));

            player.AddAnimation(KCAnimation.ClimbDown, new Animation(spriteSheet, Direction.Left, 10, 11)).SetFrameDuration(5);

            player.AddAnimation(KCAnimation.Crawl, new Animation(spriteSheet, Direction.Right, 17, 18, 19, 18)).SetFrameDuration(10);

            player.AddAnimation(KCAnimation.TransitionIn, CreateTransitionAnimation(spriteSheet, Direction.Right, 1, 3, 60, false));
            player.AddAnimation(KCAnimation.TransitionOut, CreateTransitionAnimation(spriteSheet, Direction.Right, 1, 3, 30, true));

            player.AddAnimation(KCAnimation.Attack, new Animation(spriteSheet, Direction.Right, false, 5, 6, 7)).SetFrameDuration(2);
            player.AddAnimation(KCAnimation.AttackAlt, new Animation(spriteSheet, Direction.Right, false, 20, 23, 22, 21)).SetFrameDuration(1);


            player.SetAnimation(KCAnimation.Stand);

            return playerInfo;
        }

        private static Animation CreateTransitionAnimation(SpriteSheet sheet, Direction dir, int startFrame, int totalFrames, int totalAnimationDuration, bool reverse)
        {
            List<int> frames = new List<int>();
            int frameDuration = 2;
            int numFrames = totalAnimationDuration / frameDuration;

            int index = 0;
            int count = 0;
            while (index < (totalFrames - 1))
            {
                frames.Add(startFrame + index);
                frames.Add(startFrame + index + 1);

                if (++count > (numFrames / totalFrames))
                {
                    count = 0;
                    index++;
                }
            }

            if (reverse)
                frames.Reverse();

            var a = new Animation(sheet, dir, false, frames.ToArray());
            a.SetFrameDuration(frameDuration);
            return a;
        }

        public void Damage(HitInfo hitInfo)
        {
            mHitController.RegisterHit(hitInfo);
        }

        public ulong InvincibleUntil
        {
            get
            {
                return mHitController.InvincibleUntil;
            }
            set
            {
                mHitController.InvincibleUntil = value;
            }
        }
    }
}
