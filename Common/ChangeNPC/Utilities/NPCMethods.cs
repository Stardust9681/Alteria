using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using static OtherworldMod.Common.ChangeNPC.Utilities.OtherworldNPCSets;
using static OtherworldMod.Common.ChangeNPC.AI.AIStyle_001;
using OtherworldMod.Common.ChangeNPC.Structure;

namespace OtherworldMod.Common.ChangeNPC.Utilities
{
    public static class NPCMethods
    {
        public static void AddPhase(int npcType, Func<NPC, int, string?> func)
        {
            Behaviours[npcType].Add(func);
        }
        public static void ModifyPhase(int npcType, string? expectedPhase, Func<NPC, int, string?> func)
        {
            if (expectedPhase != null)
                Behaviours[npcType].ModifyPhase(expectedPhase, func);
            else
                Behaviours[npcType].ModifyPhase("", func);
        }
        public static void OverridePhase(int npcType, string? expectedPhase, Func<NPC, int, string?> func)
        {
            if (expectedPhase != null)
                Behaviours[npcType].Add(func, expectedPhase);
            else
                Behaviours[npcType].Add(func, "");
        }
        public static void SetHitboxActive()
        {
        }
        public static void SetShotProjs()
        {
        }
        public static void SetSpawnedNPCs()
        {
        }

        public static void UnloadAI(int type)
        {
            Behaviours[type].Unload();
        }

        //Need to set this to return either position, rect, or floatrect
        //To account for invis and aggro

        /// <summary>
        /// </summary>
        /// <param name="npc"></param>
        /// <returns>True if NPC target, False if Player target</returns>
        /// <remarks>Obsolete: Use <see cref="FindTarget(NPC, out Vector2, bool, TargetMode)"/> instead.</remarks>
        [Obsolete]
        public static bool FindTarget(NPC npc, bool tileImportant = false)
        {
            bool isNPC = false;
            float dist = float.MaxValue;
            int target = npc.whoAmI;
            if (!npc.friendly)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (!Main.player[i].active) continue;
                    if (tileImportant && !Collision.CanHitLine(npc.position, npc.width, npc.height, Main.player[i].position, 32, 48)) continue;
                    float test = Vector2.DistanceSquared(npc.Center, Main.player[i].Center);
                    if (test < dist)
                    {
                        isNPC = false;
                        dist = test;
                        target = i;
                    }
                }
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active) continue;
                if (Main.npc[i].friendly == npc.friendly) continue;
                if (tileImportant && !Collision.CanHitLine(npc.position, npc.width, npc.height, Main.npc[i].position, Main.npc[i].width, Main.npc[i].height)) continue;
                float test = Vector2.DistanceSquared(npc.Center, Main.npc[i].Center);
                if (test < dist)
                {
                    isNPC = true;
                    dist = test;
                    target = i;
                }
            }
            npc.target = target;
            return isNPC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static float DistanceSQToTarget(NPC npc, bool isNPC = false)
        {
            Entity target = isNPC ? Main.npc[npc.target] : Main.player[npc.target];
            return Vector2.DistanceSquared(npc.position, target.position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="isNPC"></param>
        public static float DistanceToTarget(NPC npc, bool isNPC = false)
        {
            Entity target = isNPC ? Main.npc[npc.target] : Main.player[npc.target];
            return Vector2.Distance(npc.position, target.position);
        }

        /// <remarks>Obsolete: Use <see cref="AppxDistanceTo(NPC, Vector2)"/> instead.</remarks>
        [Obsolete]
        public static float AppxDistanceToTarget(NPC npc, bool isNPC = false)
        {
            Entity target = isNPC ? Main.npc[npc.target] : Main.player[npc.target];
            return Math.Abs(npc.Center.X - target.Center.X) + Math.Abs(npc.Center.Y - target.Center.Y);
        }

        public static float AppxDistanceTo(NPC npc, Vector2 pos)
        {
            return Math.Abs(npc.Center.X - pos.X) + Math.Abs(npc.Center.Y - pos.Y);
        }

        public static bool CanNPCShoot(NPC npc)
        {
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            return (gNPC.shootProj != null && gNPC.shootProj.Length > 0 && gNPC.shootProj[0] != 0);
        }

        //Returns false if no target is found.
        public static bool FindTarget(NPC npc, out Vector2 targetPos, bool tileImportant = false, TargetMode mode = TargetMode.Default)
        {
            targetPos = npc.position;
            if (mode == TargetMode.NoTarget)
                return false;
            float appxDist = float.MaxValue;
            bool foundTarget = false;
            if (mode.Equals(TargetMode.Default | TargetMode.PlayerOnly | TargetMode.Any | TargetMode.AnyIgnoreFriends) && mode!=TargetMode.NPCOnly)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (!p.active || p.dead)
                        continue;
                    float testDist = AppxDistanceTo(npc, p.Center);
                    if (testDist < appxDist)
                    {
                        foundTarget = true;
                        if (!tileImportant)
                        {
                            appxDist = testDist;
                            targetPos = p.Center;
                        }
                        else if(Collision.CanHitLine(npc.position, npc.width, npc.height, p.position, p.width, p.height))
                        {
                            appxDist = testDist;
                            targetPos = p.Center;
                        }
                    }
                }
            }
            if (mode.Equals(TargetMode.Default | TargetMode.NPCOnly | TargetMode.Any | TargetMode.AnyIgnoreFriends) && mode!=TargetMode.PlayerOnly)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.active)
                        continue;
                    if (npc.friendly == n.friendly && !mode.Equals(TargetMode.NPCOnly | TargetMode.AnyIgnoreFriends))
                        continue;
                    float testDist = AppxDistanceTo(npc, n.Center);
                    if (testDist < appxDist)
                    {
                        foundTarget = true;
                        if (!tileImportant)
                        {
                            appxDist = testDist;
                            targetPos = n.Center;
                        }
                        else if (Collision.CanHitLine(npc.position, npc.width, npc.height, n.position, n.width, n.height))
                        {
                            appxDist = testDist;
                            targetPos = n.Center;
                        }
                    }
                }
            }
            Vector2 offset = targetPos - npc.position;
            if (npc.confused)
                offset *= -1;
            targetPos = npc.position + offset;
            return foundTarget;
        }
    }
}
