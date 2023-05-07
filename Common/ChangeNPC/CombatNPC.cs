using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static CombatPlus.Core.Util.Utils;
using Terraria.DataStructures;

namespace CombatPlus.Common.ChangeNPC
{
    public partial class CombatNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        

        public override void Load()
        {
            Behaviours = new AI[NPCLoader.NPCCount];
            for (int i = 0; i < Behaviours.Length; i++)
            {
                Behaviours[i] = new AI(i);
            }

            foreach (int i in new int[] { NPCID.BigCrimslime, NPCID.LittleCrimslime, NPCID.JungleSlime, NPCID.YellowSlime,
                NPCID.RedSlime, NPCID.PurpleSlime, NPCID.BlackSlime, NPCID.BabySlime, NPCID.Pinky, NPCID.GreenSlime, NPCID.Slimer2,
                NPCID.Slimeling, NPCID.BlueSlime, NPCID.MotherSlime, NPCID.LavaSlime, NPCID.DungeonSlime, NPCID.CorruptSlime,
                NPCID.IlluminantSlime, NPCID.ToxicSludge, NPCID.IceSlime, NPCID.Crimslime, NPCID.SpikedIceSlime, NPCID.SpikedJungleSlime,
                NPCID.UmbrellaSlime, NPCID.RainbowSlime, NPCID.SlimeMasked, NPCID.HoppinJack, NPCID.SlimeRibbonWhite,
                NPCID.SlimeRibbonYellow, NPCID.SlimeRibbonGreen, NPCID.SlimeRibbonRed, NPCID.Grasshopper, NPCID.GoldGrasshopper,
                NPCID.SlimeSpiked, NPCID.SandSlime, NPCID.QueenSlimeMinionBlue, NPCID.QueenSlimeMinionPink, NPCID.GoldenSlime })
            {
                if (i > 0 && i < Behaviours.Length)
                    RegisterSlime(i);
            }
            foreach (int i in new int[] { NPCID.DemonEye2, NPCID.PurpleEye2, NPCID.GreenEye2, NPCID.DialatedEye2, NPCID.SleepyEye2,
                NPCID.CataractEye2, NPCID.DemonEye, NPCID.TheHungryII, NPCID.WanderingEye, NPCID.PigronCorruption, NPCID.PigronHallow,
                NPCID.PigronCrimson, NPCID.CataractEye, NPCID.SleepyEye, NPCID.DialatedEye, NPCID.GreenEye, NPCID.PurpleEye,
                NPCID.DemonEyeOwl, NPCID.DemonEyeSpaceship })
            {
                if (i > 0 && i < Behaviours.Length)
                    RegisterEye(i);
            }
            foreach (int i in new int[] { NPCID.Harpy, NPCID.CaveBat, NPCID.JungleBat, NPCID.Hellbat, NPCID.Demon, NPCID.VoodooDemon,
                NPCID.GiantBat, NPCID.Slimer, NPCID.IlluminantBat, NPCID.IceBat, NPCID.Lavabat, NPCID.GiantFlyingFox, NPCID.RedDevil,
                NPCID.VampireBat, NPCID.FlyingSnake, NPCID.SporeBat, NPCID.QueenSlimeMinionPurple })
            {
                if (i > 0 && i < Behaviours.Length)
                    RegisterBat(i);
            }
        }
        public override void SetDefaults(NPC npc)
        {
            if (npc.aiStyle == 2)
                npc.noGravity = true;
            if (npc.type == NPCID.Harpy)
                npc.noGravity = true;

            SetVanillaDefaults(npc);
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if(allowContactDmg)
                return base.CanHitPlayer(npc, target, ref cooldownSlot);
            return false;
        }
        public override bool PreAI(NPC npc)
        {
            int timer = (int)npc.ai[0];
            /*CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            if(gNPC.ignoreAIChanges) return base.PreAI(npc);
            switch (npc.aiStyle)
            {
                case 0:
                    break;
                case NPCAIStyleID.Slime:
                    AI1(npc);
                    return false;
                case NPCAIStyleID.DemonEye:
                    AI2(npc);
                    return false;
                case NPCAIStyleID.Fighter:
                    AI3(npc);
                    return true;
                case NPCAIStyleID.EyeOfCthulhu:
                    AI4(npc);
                    return true;
                case NPCAIStyleID.Flying:
                    AI5(npc);
                    return false;
                case NPCAIStyleID.Worm:
                    break;
                case NPCAIStyleID.Passive:
                    break;
                case NPCAIStyleID.Caster:
                    break;
                case NPCAIStyleID.Spell:
                    break;
                case NPCAIStyleID.CursedSkull:
                    break;
            }*/
            if (npc.aiStyle == NPCAIStyleID.Slime)
            {
                Behaviours[npc.netID].Update(npc, ref phase, ref timer);
                npc.ai[0] = timer;
                return false;
            }
            if (npc.aiStyle == NPCAIStyleID.DemonEye)
            {
                try
                {
                    Behaviours[npc.netID].Update(npc, ref phase, ref timer);
                }
                catch (Exception e)
                {
                    Main.NewText(phase + ", " + e.Message);
                }
                npc.ai[0] = timer;
                return false;
            }
            if (npc.aiStyle == NPCAIStyleID.Bat)
            {
                Behaviours[npc.netID].Update(npc, ref phase, ref timer);
                npc.ai[0] = timer;
                return false;
            }
            return base.PreAI(npc);
        }
    }
}
