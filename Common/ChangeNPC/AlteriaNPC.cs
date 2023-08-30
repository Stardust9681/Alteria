using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Alteria.Core.Util.Utils;
using Terraria.DataStructures;
using Terraria.Utilities;
using Terraria.GameContent.ItemDropRules;
using static Alteria.Common.ChangeNPC.Utilities.AlteriaNPCSets;
using Alteria.Common.ChangeNPC.AI;
using Terraria.ModLoader.IO;
using System.IO;
using System.Reflection;
using Alteria.Common.ChangeNPC.Utilities;

namespace Alteria.Common.ChangeNPC
{
    //For Town NPCs:
    //Vine or rope integration into AI?
    public partial class AlteriaNPC : GlobalNPC
    {
        //Whether or not to run new AI (if this is false, dynamic hitbox and NPC grief settings have no effect
        public bool AIChanges => Alteria.Instance.NPCAIChanges;

        //Determine whether or not to use Otherworld's dynamic hitbox setting(s) rather than a constant hitbox
        public bool DynamicHitbox => Alteria.Instance.NPCDynamicHitboxes;

        //Whether or not NPCs can grief terrain (not all NPCs have this behaviour)
        public bool NPCGrief => Alteria.Instance.NPCGrief;

        public override bool InstancePerEntity => true;

        public override void SetDefaults(NPC npc)
        {
            if (AIChanges)
            {
                if (npc.aiStyle == 2)
                    npc.noGravity = true;
                if (NPCID.Sets.ActsLikeTownNPC[npc.netID] && npc.aiStyle == NPCAIStyleID.Passive)
                    npc.GetGlobalNPC<AlteriaNPC>().aggro = 55;
                if (npc.type == NPCID.Harpy)
                    npc.noGravity = true;
                if (npc.type == NPCID.EyeofCthulhu)
                {
                    npc.GetGlobalNPC<AlteriaNPC>().spawnNPC = new int[] { NPCID.ServantofCthulhu, NPCID.DemonEye };
                }
																if (npc.type == NPCID.BrainofCthulhu)
																{
																				npc.GetGlobalNPC<AlteriaNPC>().spawnNPC = new int[] { NPCID.Creeper };
																				npc.GetGlobalNPC<AlteriaNPC>().aggro = 1;
																}

                SetVanillaDefaults(npc);
            }
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (AIChanges)
            {
																AlteriaNPC thisNPC = npc.GetGlobalNPC<AlteriaNPC>();
                if (Behaviours[npc.netID].HasEntry)
                {
																				thisNPC.NPCTarget = Behaviours[npc.netID].SetTarget(npc.whoAmI);
																				thisNPC.NPCRadar = Behaviours[npc.netID].SetRadar(npc.whoAmI);
                }
                else
                {
																				thisNPC.NPCTarget = new Core.Util.NPCTarget(npc.whoAmI);
																				thisNPC.NPCRadar = new Core.Util.NPCRadar(npc.whoAmI);
                }
																Core.Util.TargetCollective.AddTarget(thisNPC.NPCTarget);
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
                if (attacker.GetGlobalNPC<AlteriaNPC>().allowContactDmg)
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
                try
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
                catch (NullReferenceException nullRef)
                {
                    Logging.PublicLogger.Error(nullRef.Message, nullRef);
                    Main.NewText(nullRef.Message, Color.DarkRed);
                    npc.timeLeft = 0;
                    npc.active = false;
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
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npc, npcLoot);
            /*ItemDropDatabase lootDB = (ItemDropDatabase)typeof(NPCLoot).GetField("itemDropDatabase", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(npcLoot);
            int netID = (int)typeof(NPCLoot).GetField("npcNetId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(npcLoot);
            List<IItemDropRule> drops = (List<IItemDropRule>)typeof(ItemDropDatabase).GetField("_globalEntries", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lootDB);
            List<DropRateInfo> dropRates = new List<DropRateInfo>();
            DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed(1f);
            foreach (IItemDropRule x in drops)
            {
                x.ReportDroprates(dropRates, ratesInfo);
                
            }
            int count = 0;
            LootTable table = new LootTable();
            foreach (DropRateInfo info in dropRates)
            {
                //Add loot to manager here
                LootDrop drop = new LootDrop(info.dropRate, NPCMethods.GetLootIndexName(NPCMethods.GetLootTableName(netID), count));
                drop.AddToPool(1, info.itemId, info.stackMin, info.stackMax);
                table.AddDrop(drop);
                npcLoot.Add(new NewItemDropRule(NPCMethods.GetLootTableName(netID), NPCMethods.GetLootIndexName(NPCMethods.GetLootTableName(netID), count));
                count++;
            }
            LootManager.AddTable(NPCMethods.GetLootTableName(netID), table);*/
        }
    }
}
