using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using Terraria.DataStructures;
using Terraria.Utilities;
using static OtherworldMod.Common.ChangeNPC.Utilities.OtherworldNPCSets;
using OtherworldMod.Common.ChangeNPC.AI;

namespace OtherworldMod.Common.ChangeNPC
{
    //For Town NPCs:
        //Vine or rope integration into AI?
    public partial class OtherworldNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npc, npcLoot);
        }
        public override void Load()
        {
            Behaviours = new AIStyle[NPCLoader.NPCCount];
            for (int i = 0; i < Behaviours.Length; i++)
            {
                Behaviours[i] = new AIStyle(i);
            }

            //ILoadable was throwing nullref, hooray.
            //So now you get to see this garbage until I add loading system for the ai classes.
            AIStyle_001.Load();
            AIStyle_002.Load();
            AIStyle_004.Load();
            AIStyle_005.Load();
            AIStyle_010.Load();
            AIStyle_014.Load();
        }
        public override void SetDefaults(NPC npc)
        {
            if (npc.aiStyle == 2)
                npc.noGravity = true;
            if (npc.type == NPCID.Harpy)
                npc.noGravity = true;
            if (npc.type == NPCID.EyeofCthulhu)
            {
                npc.GetGlobalNPC<OtherworldNPC>().spawnNPC = new int[] { NPCID.ServantofCthulhu, NPCID.DemonEye };
            }
            SetVanillaDefaults(npc);
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if(allowContactDmg)
                return base.CanHitPlayer(npc, target, ref cooldownSlot);
            return false;
        }
        public override bool CanBeHitByNPC(NPC npc, NPC attacker)
        {
            if (attacker.GetGlobalNPC<OtherworldNPC>().allowContactDmg)
                if (npc.friendly != attacker.friendly) return true;
                else return base.CanBeHitByNPC(npc, attacker);
            return false;
        }
        public override bool CanHitNPC(NPC npc, NPC target)
        {
            if (allowContactDmg)
                if (npc.friendly != target.friendly) return true;
                else return base.CanHitNPC(npc, target);
            return false;
        }
        public override bool PreAI(NPC npc)
        {
            int timer = (int)npc.ai[0];
            if (Behaviours[npc.netID].HasEntry)
            {
                Behaviours[npc.netID].Update(npc, ref phase, ref timer);
                npc.ai[0] = timer;
                return false;
            }
            return base.PreAI(npc);
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            //Will look for way to add this into AI instead of through findframe
            if (npc.aiStyle == NPCAIStyleID.EyeOfCthulhu)
            {
                npc.frameCounter++;
                frameHeight = 166;
                if (npc.life > npc.lifeMax * .5f)
                {
                    npc.frame = new Rectangle(0, (((int)npc.frameCounter%24)/24)*frameHeight, 110, frameHeight);
                }
                else
                {
                    npc.frame = new Rectangle(0, (3+(((int)npc.frameCounter % 24) / 24)) * frameHeight, 110, frameHeight);
                }
                if (npc.frameCounter == 24)
                    npc.frameCounter = 0;
                return;
            }
            base.FindFrame(npc, frameHeight);
        }
    }
}
