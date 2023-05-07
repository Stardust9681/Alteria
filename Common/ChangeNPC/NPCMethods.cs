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
        public static void AddPhase(int npcType, Func<NPC, int, string?> func)
        {
            Behaviours[npcType].Add(func);
        }
        public static void ModifyPhase(int npcType, string? expectedPhase, Func<NPC, int, string?> func)
        {
            if(expectedPhase != null)
                Behaviours[npcType].ModifyPhase(expectedPhase, func);
            else
                Behaviours[npcType].ModifyPhase("", func);
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
        public static void RegisterSlime(int type)
        {
            Behaviours[type].Add(SlimeJump1, nameof(SlimeJump1));
            Behaviours[type].Add(SlimeJump2, nameof(SlimeJump2));
            Behaviours[type].Add(SlimeJump3, nameof(SlimeJump3));
            Behaviours[type].Add(SlimeShoot1, nameof(SlimeShoot1));
            Behaviours[type].Add(SlimeShoot2, nameof(SlimeShoot2));
            Behaviours[type].Add(SlimeWet, nameof(SlimeWet));
        }
        public static void RegisterEye(int type)
        {
            Behaviours[type].Add(EyeAttack1, nameof(EyeAttack1));
            Behaviours[type].Add(EyeAttack2, nameof(EyeAttack2));
            Behaviours[type].Add(EyeAttack3, nameof(EyeAttack3));
            Behaviours[type].Add(EyeWet, nameof(EyeWet));
            Behaviours[type].Add(EyeDaytime, nameof(EyeDaytime));
        }
        public static void RegisterBat(int type)
        {
            Behaviours[type].Add(BatMove1, nameof(BatMove1));
            Behaviours[type].Add(BatAttack1, nameof(BatAttack1));
            Behaviours[type].Add(BatAttack2, nameof(BatAttack2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>True if NPC target, False if Player target
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
        /// <returns></returns>
        public static float DistanceToTarget(NPC npc, bool isNPC = false)
        {
            Entity target = isNPC ? Main.npc[npc.target] : Main.player[npc.target];
            return Vector2.Distance(npc.position, target.position);
        }

        public static float AppxDistanceToTarget(NPC npc, bool isNPC = false)
        {
            Entity target = isNPC ? Main.npc[npc.target] : Main.player[npc.target];
            return Math.Abs(npc.Center.X - target.Center.X) + Math.Abs(npc.Center.Y - target.Center.Y);
        }
    }
}
