using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static CombatPlus.Core.Util.Utils;
using static CombatPlus.Common.ChangeNPC.Utilities.NPCMethods;
using static CombatPlus.Common.ChangeNPC.Utilities.OtherworldNPCSets;

namespace CombatPlus.Common.ChangeNPC.AI
{
    public class AIStyle_014
    {
        public static void Load()
        {
            foreach (int i in new int[] { NPCID.Harpy, NPCID.CaveBat, NPCID.JungleBat, NPCID.Hellbat, NPCID.Demon, NPCID.VoodooDemon,
                NPCID.GiantBat, NPCID.Slimer, NPCID.IlluminantBat, NPCID.IceBat, NPCID.Lavabat, NPCID.GiantFlyingFox, NPCID.RedDevil,
                NPCID.VampireBat, NPCID.FlyingSnake, NPCID.SporeBat, NPCID.QueenSlimeMinionPurple })
            {
                if (i > 0 && i < Behaviours.Length)
                {
                    Behaviours[i].Add(BatMove1, nameof(BatMove1));
                    Behaviours[i].Add(BatAttack1, nameof(BatAttack1));
                    Behaviours[i].Add(BatAttack2, nameof(BatAttack2));
                }
            }
        }
        public void Unload()
        {
            foreach (int i in new int[] { NPCID.Harpy, NPCID.CaveBat, NPCID.JungleBat, NPCID.Hellbat, NPCID.Demon, NPCID.VoodooDemon,
                NPCID.GiantBat, NPCID.Slimer, NPCID.IlluminantBat, NPCID.IceBat, NPCID.Lavabat, NPCID.GiantFlyingFox, NPCID.RedDevil,
                NPCID.VampireBat, NPCID.FlyingSnake, NPCID.SporeBat, NPCID.QueenSlimeMinionPurple })
            {
                if (i > 0 && i < Behaviours.Length)
                    UnloadAI(i);
            }
        }
        static string? BatMove1(NPC npc, int timer)
        {
            //Bat-enabled NPCs seem to have gravity for some reason
            //So disabling that
            if (!npc.noGravity)
                npc.noGravity = true;
            //Find a target, true if NPC found
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            //Direction to target (account for confusion)
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            //Disable contact damage
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            //Find and move towards target X position
            float targetX = target.position.X - (targetDir * 96f);
            if (npc.position.X > targetX)
            {
                npc.velocity.X -= .08f;
            }
            else
            {
                npc.velocity.X += .08f;
            }
            //Slow down if near target position
            if (MathF.Abs(npc.position.X - targetX) < 64)
                npc.velocity.X *= .95f;
            //Find and move towards target Y position
            float targetY = target.position.Y - 320;
            if (npc.position.Y > targetY)
            {
                npc.velocity.Y -= .06f;
            }
            else
            {
                npc.velocity.Y += .06f;
            }
            //Slow down if near target position
            if (MathF.Abs(npc.position.Y - targetY) < 64)
                npc.velocity.Y *= .98f;
            //Slow down if very close to target
            if (AppxDistanceToTarget(npc, npcTarget) < 400)
            {
                npc.velocity *= .987f;
            }
            //If close enough to position or hit ceiling (ex; cave bats in caverns)
            if (npc.DistanceSQ(new Vector2(targetX, targetY)) < npc.Size.LengthSquared() || (npc.collideY && npc.oldVelocity.Y < 0))
            {
                return nameof(BatAttack1);
            }
            return null;
        }
        static string? BatAttack1(NPC npc, int timer)
        {
            //Find a target, true if NPC found
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            //Direction to target (account for confusion)
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            //If npc not moving enough downwards, fix that. Also responsible for initial swoop downwards.
            if (npc.velocity.Y < 1)
            {
                npc.velocity.Y = 7.4f;
            }
            //Allow contact damage
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
            //Adjust velocity over time, to slow down and to be more "swoop-like"
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(target.position) * 4f, .018f);
            //If close enough to target, slow down
            if (AppxDistanceToTarget(npc, npcTarget) > 600)
            {
                npc.velocity *= .987f;
            }
            //If moving away from the target or too far below the target move to next attack step.
            if (((npc.velocity.X < 0 ? -1 : 1) != targetDir) || npc.position.Y > target.position.Y)
                return nameof(BatAttack2);
            return null;
        }
        static string? BatAttack2(NPC npc, int timer)
        {
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            //If NPC does not fire projectiles, go back to first move state
            if (gNPC.shootProj == null || gNPC.shootProj.Length == 0 || gNPC.shootProj[0] == 0)
                return nameof(BatMove1);
            //Disable contact damage
            gNPC.allowContactDmg = false;
            //Find a target
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            npc.velocity *= .9f;
            //If timer is past a value, shoot projectile(s)
            if (timer > 70)
            {
                Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(target.Center) * 8.4f, Main.rand.Next(gNPC.shootProj), npc.damage / 3, npc.knockBackResist * 2f, Main.myPlayer);
                proj.friendly = npc.friendly;
                proj.hostile = !npc.friendly;

                float dist = AppxDistanceToTarget(npc, npcTarget);
                //Determine if player is close enough to move to Shoot2
                if (dist < 600)
                {
                    //Just rand, repeats this state some number of times, then moves to move state
                    if (Main.rand.NextBool())
                        return nameof(BatAttack2);
                    else
                        return nameof(BatMove1);
                }
                else
                    return nameof(BatMove1);
            }
            return null;
        }
    }
}
