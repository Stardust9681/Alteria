using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static CombatPlus.Core.Util.Utils;

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

            Behaviours[NPCID.BlueSlime].Add(SlimeJump1, nameof(SlimeJump1));
            Behaviours[NPCID.BlueSlime].Add(SlimeJump2, nameof(SlimeJump2));
            Behaviours[NPCID.BlueSlime].Add(SlimeJump3, nameof(SlimeJump3));
            Behaviours[NPCID.BlueSlime].Add(SlimeShoot1, nameof(SlimeShoot1));
            Behaviours[NPCID.BlueSlime].Add(SlimeShoot2, nameof(SlimeShoot2));
            Behaviours[NPCID.BlueSlime].Add(SlimeWet, nameof(SlimeWet));

            Behaviours[NPCID.SpikedIceSlime].Add(SlimeJump1, nameof(SlimeJump1));
            Behaviours[NPCID.SpikedIceSlime].Add(SlimeJump2, nameof(SlimeJump2));
            Behaviours[NPCID.SpikedIceSlime].Add(SlimeJump3, nameof(SlimeJump3));
            Behaviours[NPCID.SpikedIceSlime].Add(SlimeShoot1, nameof(SlimeShoot1));
            Behaviours[NPCID.SpikedIceSlime].Add(SlimeShoot2, nameof(SlimeShoot2));
            Behaviours[NPCID.SpikedIceSlime].Add(SlimeWet, nameof(SlimeWet));

            Behaviours[NPCID.DemonEye].Add(EyeAttack1, nameof(EyeAttack1));
            Behaviours[NPCID.DemonEye].Add(EyeAttack2, nameof(EyeAttack2));
            Behaviours[NPCID.DemonEye].Add(EyeWet, nameof(EyeWet));
            Behaviours[NPCID.DemonEye].Add(EyeDaytime, nameof(EyeDaytime));
        }
        public override void SetDefaults(NPC npc)
        {
            if (npc.aiStyle == 2)
                npc.noGravity = true;
            if (npc.type == NPCID.Harpy)
                npc.aiStyle = 5;

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
            return base.PreAI(npc);
        }
    }
}
