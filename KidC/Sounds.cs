﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace KidC
{
    static class Sounds
    {
        public static SoundResource None { get; private set; }       
        public static SoundResource IronKnightClimb { get; private set; }
        public static SoundResource ManiaxeThrow { get; private set; }
        public static SoundResource Jump { get; private set; }
        public static SoundResource BlockHit { get; private set; }
        public static SoundResource RockBlockDestroyed { get; private set; }
        public static SoundResource MetalBlockPing { get; private set; }


        public static SoundResource GetDiamond { get; private set; }
    
        
        public static SoundResource IronKnightTransform { get; private set; }
       
        
        public static SoundResource RedStealthTransform { get; private set; }
        public static SoundResource RedStealthAttack { get; private set; }
        public static SoundResource RedStealthShout { get; private set; }

        public static SoundResource EnemyBounce { get; private set; }
        public static SoundResource EnemyHit { get; private set; }

        public static SoundResource PlayerHit { get; private set; }
        public static SoundResource PlayerDie { get; private set; }
        public static SoundResource NoTime { get; private set; }
        public static SoundResource KidFlip { get; private set; }
        public static SoundResource Bummer { get; private set; }

        public static SoundResource RubberBounce { get; private set; }
        public static SoundResource ClockTick { get; private set; }
        public static SoundResource ScoreCounter { get; private set; }

        public static SoundResource BlockVanish { get; private set; }


        public static SoundResource FromString(string text)
        {
            return (SoundResource)typeof(Sounds).GetProperty(text).GetValue(null, null);
        }

        static Sounds()
        {
            List<SoundResource> sounds = new List<SoundResource>();

            sounds.Add(IronKnightClimb = new SoundResource("Special Move - Iron Knight and Maniaxe - (1C)"));
            ManiaxeThrow = IronKnightClimb;

            sounds.Add(Jump = new SoundResource("Jump - (17)"));
            sounds.Add(BlockHit = new SoundResource("Block - Hit - (0D)"));
            sounds.Add(RockBlockDestroyed = new SoundResource("Block - Rock Block Destroyed - (11)"));
            sounds.Add(IronKnightTransform = new SoundResource("Transformation - Iron Knight - (1A)"));
            sounds.Add(RedStealthTransform = new SoundResource("Transformation - Red Stealth - (2E)"));
            sounds.Add(RedStealthAttack = new SoundResource("Special Move - Red Stealth (sideways attack) - (23)"));
            sounds.Add(RedStealthShout = new SoundResource("Special Move - Red Stealth (attack down) - (13)"));
            sounds.Add(GetDiamond = new SoundResource("Prize - Diamond - (2D)",.5f));
            sounds.Add(EnemyBounce = new SoundResource("Enemy - Bouncing - (5D)"));
            sounds.Add(PlayerHit = new SoundResource("Voice - Ouch - (61)"));
            sounds.Add(KidFlip = new SoundResource("Special Move - The Kid - (64)"));
            sounds.Add(PlayerDie = new SoundResource("Voice - Die - (60)"));
            sounds.Add(NoTime = new SoundResource("Voice - Oh God - (51)"));
            sounds.Add(EnemyHit = new SoundResource("Enemy - Shot Dead - (4B)"));
            sounds.Add(MetalBlockPing = new SoundResource("Block - Metal Hit - (12)"));
            sounds.Add(RubberBounce = new SoundResource("Block - Rubber - (56)"));
            sounds.Add(Bummer = new SoundResource("Voice - Bummer - (68)"));
            sounds.Add(ClockTick = new SoundResource("Prize - Clock - (0E)"));
            sounds.Add(ScoreCounter = new SoundResource("Score Counter - (0C)"));
            sounds.Add(BlockVanish = new SoundResource("Block - Evanescent - (14)"));

            foreach(var sound in sounds)
                SoundManager.LoadSound(sound);


            None = new SoundResource("");
        }


    }
}
