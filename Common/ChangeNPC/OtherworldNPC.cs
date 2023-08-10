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
using Terraria.ModLoader.IO;
using System.IO;
using System.Reflection;

namespace OtherworldMod.Common.ChangeNPC
{
    //For Town NPCs:
    //Vine or rope integration into AI?
    public partial class OtherworldNPC : GlobalNPC
    {
        //Whether or not to run new AI (if this is false, dynamic hitbox and NPC grief settings have no effect
        public bool AIChanges => OtherworldMod.Instance.NPCAIChanges;

        //Determine whether or not to use Otherworld's dynamic hitbox setting(s) rather than a constant hitbox
        public bool DynamicHitbox => OtherworldMod.Instance.NPCDynamicHitboxes;

        //Whether or not NPCs can grief terrain (not all NPCs have this behaviour)
        public bool NPCGrief => OtherworldMod.Instance.NPCGrief;

        public override bool InstancePerEntity => true;

        public override void SetDefaults(NPC npc)
        {
            if (AIChanges)
            {
                if (npc.aiStyle == 2)
                    npc.noGravity = true;
                if (NPCID.Sets.ActsLikeTownNPC[npc.netID] && npc.aiStyle == NPCAIStyleID.Passive)
                    npc.GetGlobalNPC<OtherworldNPC>().aggro = 55;
                if (npc.type == NPCID.Harpy)
                    npc.noGravity = true;
                if (npc.type == NPCID.EyeofCthulhu)
                {
                    npc.GetGlobalNPC<OtherworldNPC>().spawnNPC = new int[] { NPCID.ServantofCthulhu, NPCID.DemonEye };
                }

                SetVanillaDefaults(npc);
            }
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (AIChanges)
            {
                if (Behaviours[npc.netID].HasEntry)
                {
                    NPCTarget = Behaviours[npc.netID].SetTarget(npc.whoAmI);
                    NPCRadar = Behaviours[npc.netID].SetRadar(npc.whoAmI);
                }
                else
                {
                    Core.Util.TargetCollective.AddTarget(new Core.Util.NPCTarget(npc.whoAmI));
                    NPCRadar = new Core.Util.NPCRadar(npc.whoAmI);
                }
            }
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (DynamicHitbox)
            {
                if (allowContactDmg)
                    return base.CanHitPlayer(npc, target, ref cooldownSlot);
                return false;
            }
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override bool CanBeHitByNPC(NPC npc, NPC attacker)
        {
            if (DynamicHitbox)
            {
                if (attacker.GetGlobalNPC<OtherworldNPC>().allowContactDmg)
                    if (npc.friendly != attacker.friendly) return true;
                    else return base.CanBeHitByNPC(npc, attacker);
                return false;
            }
            return base.CanBeHitByNPC(npc, attacker);
        }
        public override bool CanHitNPC(NPC npc, NPC target)
        {
            if (DynamicHitbox)
            {
                if (allowContactDmg)
                    if (npc.friendly != target.friendly) return true;
                    else return base.CanHitNPC(npc, target);
                return false;
            }
            return base.CanHitNPC(npc, target);
        }
        public override bool PreAI(NPC npc)
        {
            if (AIChanges)
            {
                if (npc.netID >= 0 && npc.netID < Behaviours.Length && Behaviours[npc.netID].HasEntry)
                {
                    int timer = (int)npc.ai[0];
                    string? curPhase = phase;
                    Behaviours[npc.netID].Update(npc, ref phase, ref timer);
                    npc.ai[0] = timer;
                    if (phase?.Equals(curPhase) == false)
                    {
                        npc.netUpdate = true;
                    }
                    return false;
                }
            }
            return base.PreAI(npc);
        }
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            if (AIChanges)
            {
                //Uncomment this line to debug Netsync
                //Logging.PublicLogger.Debug($"SendExtraAI(2) -> {Behaviours[npc.netID].HasEntry} : Phase = {phase} : Netmode = {Main.netMode}");

                //Note: the timer is already synced, NPC position and velocity are already synced, behaviour is deterministic
                //This *SHOULD* never cause problems.
                if (Behaviours[npc.netID].HasEntry)
                {
                    int index = Behaviours[npc.type].GetPhaseIndex(phase);
                    binaryWriter.Write(index);
                    //Uncomment this line to debug Netsync
                    //Logging.PublicLogger.Debug($"SendExtraAI(2) -> {Behaviours[npc.netID].HasEntry} : Phase = {phase} : Index = {index} : Netmode = {Main.netMode}");
                }
            }
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            if (AIChanges)
            {
                //Uncomment these lines to debug Netsync
                //Logging.PublicLogger.Debug($"ReceiveExtraAI(1) -> {Behaviours[npc.netID].HasEntry} : Phase = {phase} : Netmode = {Main.netMode}");
                //Logging.PublicLogger.Debug("\tReceiveExtraAI(1a) -> " + s);

                //Note: the timer is already synced, NPC position and velocity are already synced, behaviour is deterministic
                //This *SHOULD* never cause problems.
                if (Behaviours[npc.netID].HasEntry)
                {
                    int index = binaryReader.ReadInt32();
                    phase = Behaviours[npc.netID].PhaseFromIndex(index);
                    //Uncomment this line to debug Netsync
                    //Logging.PublicLogger.Debug($"ReceiveExtraAI(2) -> {Behaviours[npc.netID].HasEntry} : Phase = {phase} : Index = {index} : Netmode = {Main.netMode}");
                }
            }
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (AIChanges)
            {
                //Will look for way to add this into AI instead of through findframe
                if (npc.aiStyle == NPCAIStyleID.EyeOfCthulhu)
                {
                    npc.frameCounter++;
                    frameHeight = 166;
                    if (npc.life > npc.lifeMax * .5f)
                    {
                        npc.frame = new Rectangle(0, (((int)npc.frameCounter % 24) / 24) * frameHeight, 110, frameHeight);
                    }
                    else
                    {
                        npc.frame = new Rectangle(0, (3 + (((int)npc.frameCounter % 24) / 24)) * frameHeight, 110, frameHeight);
                    }
                    if (npc.frameCounter == 24)
                        npc.frameCounter = 0;
                    return;
                }
            }
            base.FindFrame(npc, frameHeight);
        }
    }
}
