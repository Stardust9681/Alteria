using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using static OtherworldMod.Common.ChangeNPC.Utilities.NPCMethods;
using static OtherworldMod.Common.ChangeNPC.Utilities.OtherworldNPCSets;

namespace OtherworldMod.Common.ChangeNPC.AI
{
    public class AIStyle_014 : AIStyleType
    {
        protected override int[] ApplicableNPCs => new int[] { NPCID.Harpy, NPCID.CaveBat, NPCID.JungleBat, NPCID.Hellbat, NPCID.Demon, NPCID.VoodooDemon,
                NPCID.GiantBat, NPCID.Slimer, NPCID.IlluminantBat, NPCID.IceBat, NPCID.Lavabat, NPCID.GiantFlyingFox, NPCID.RedDevil,
                NPCID.VampireBat, NPCID.FlyingSnake, NPCID.SporeBat, NPCID.QueenSlimeMinionPurple };
        public override void Load()
        {
            AddAI(BatMove1, BatAttack1, BatAttack2);
        }
        static string? BatMove1(NPC npc, int timer)
        {
            //Bat-enabled NPCs seem to have gravity for some reason
            //So disabling that
            if (!npc.noGravity)
                npc.noGravity = true;
            //Find a target, true if NPC found
            bool foundTarget = FindTarget(npc, out Vector2 targetPos);
            //Direction to target (account for confusion)
            int targetDir = targetPos.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            //Disable contact damage
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;
            //Find and move towards target X position
            float targetX = targetPos.X - (targetDir * 96f);
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
            float targetY = targetPos.Y - 320;
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
            if (AppxDistanceTo(npc, targetPos) < 400)
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
            bool foundTarget = FindTarget(npc, out Vector2 targetPos);
            //Direction to target (account for confusion)
            int targetDir = targetPos.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            //If npc not moving enough downwards, fix that. Also responsible for initial swoop downwards.
            if (npc.velocity.Y < 1)
            {
                npc.velocity.Y = 7.4f;
            }
            //Allow contact damage
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = true;
            //Adjust velocity over time, to slow down and to be more "swoop-like"
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(targetPos) * 5f, .021f);
            //If close enough to target, slow down
            if (AppxDistanceTo(npc, targetPos) > 600)
            {
                npc.velocity *= .987f;
            }
            //If moving away from the target or too far below the target move to next attack step.
            if (((npc.velocity.X < 0 ? -1 : 1) != targetDir) || npc.position.Y > targetPos.Y + 32f)
                return nameof(BatAttack2);
            return null;
        }
        static string? BatAttack2(NPC npc, int timer)
        {
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            //If NPC does not fire projectiles, go back to first move state
            if (gNPC.shootProj == null || gNPC.shootProj.Length == 0 || gNPC.shootProj[0] == 0)
                return nameof(BatMove1);
            //Disable contact damage
            gNPC.allowContactDmg = false;
            //Find a target
            bool foundTarget = FindTarget(npc, out Vector2 targetPos);
            npc.velocity *= .9f;
            //If timer is past a value, shoot projectile(s)
            if (timer%70 == 0)
            {
                Projectile proj = npc.SpawnProjDirect(npc.Center, npc.DirectionTo(targetPos) * 8.4f, Main.rand.Next(gNPC.shootProj), npc.damage / 3, npc.knockBackResist * 2f, Main.myPlayer);
                proj.friendly = npc.friendly;
                proj.hostile = !npc.friendly;

                float dist = AppxDistanceTo(npc, targetPos);
                if (dist < 600)
                {
                    if (timer < 210)
                        return null;
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
