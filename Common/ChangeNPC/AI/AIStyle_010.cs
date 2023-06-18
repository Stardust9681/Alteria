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
using OtherworldMod.Core.Util;

namespace OtherworldMod.Common.ChangeNPC.AI
{
#nullable enable
    /// <summary>
    /// <see cref="NPCAIStyleID.CursedSkull"/>
    /// </summary>
    public class AIStyle_010 : AIStyleType
    {
        protected override int[] ApplicableNPCs => new int[] { NPCID.CursedSkull, NPCID.GiantCursedSkull };
        public override void Load()
        {
            AddAI(RotateCW, RotateCCW, Attack1, Attack2);
        }
        const float t = 360;
        const float r = 360f/90f;
        public static string? RotateCW(NPC npc, int timer)
        {
            if (timer > t)
                return nameof(Attack1);

            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;

            npc.target = TargetCollective.PullTarget(new NPCTargetSource(npc), out TargetInfo info);
            float dist = AppxDistanceTo(npc, info.Position);

            if (dist > 800)
                npc.velocity *= .86f;

            int xCubed = timer * timer * timer;
            float tSq = t * t;
            float rotation = r * (timer - (xCubed / (3 * tSq)));
            rotation += (npc.whoAmI * 45);
            Vector2 targetPos = info.Position + Vector2.UnitX.RotatedBy(MathHelper.ToRadians(rotation)) * npc.lifeMax * 2.5f;

            //npc.velocity.X = targetPos.X < npc.position.X ? -3f : 3f;
            //npc.velocity.Y = targetPos.Y < npc.position.Y ? -3f : 3f;
            float distSq = Vector2.DistanceSquared(npc.position, targetPos);
            npc.velocity = npc.DirectionTo(targetPos) * MathHelper.Lerp(2, 5.75f, (distSq - 625) / 16000f);
            if (distSq < 3200)
                npc.velocity *= .34f;
            //npc.position = targetPos;

            npc.rotation = (targetPos - npc.Center).ToRotation();
            npc.spriteDirection = targetPos.X > npc.position.X ? -1 : 1;
            if (npc.spriteDirection == 1)
                npc.rotation += MathHelper.Pi;

            return null;
        }
        public static string? RotateCCW(NPC npc, int timer)
        {
            if (timer > t)
                return nameof(Attack1);

            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;

            npc.target = TargetCollective.PullTarget(new NPCTargetSource(npc), out TargetInfo info);
            float dist = AppxDistanceTo(npc, info.Position);

            if (dist > 800)
                npc.velocity *= .86f;

            int xCubed = timer * timer * timer;
            float tSq = t * t;
            float rotation = r * (timer - (xCubed / (3 * tSq)));
            rotation += (npc.whoAmI * 45);
            Vector2 targetPos = info.Position - Vector2.UnitX.RotatedBy(-MathHelper.ToRadians(rotation)) * npc.lifeMax * 2.5f;

            //npc.velocity.X = targetPos.X < npc.position.X ? -3f : 3f;
            //npc.velocity.Y = targetPos.Y < npc.position.Y ? -3f : 3f;
            float distSq = Vector2.DistanceSquared(npc.position, targetPos);
            npc.velocity = npc.DirectionTo(targetPos) * MathHelper.Lerp(2, 5.75f, (distSq-625)/16000f);
            if (distSq < 3200)
                npc.velocity *= .34f;
            //npc.position = targetPos;

            npc.rotation = (targetPos - npc.Center).ToRotation();
            npc.spriteDirection = targetPos.X > npc.position.X ? -1 : 1;
            if (npc.spriteDirection == 1)
                npc.rotation += MathHelper.Pi;

            return null;
        }
        public static string? Attack1(NPC npc, int timer)
        {
            npc.target = TargetCollective.PullTarget(new NPCTargetSource(npc), out TargetInfo info);
            float dist = AppxDistanceTo(npc, info.Position);

            if (timer == 45)
                npc.velocity = npc.DirectionTo(info.Position) * 8f;
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = npc.velocity.LengthSquared() > 4f;

            if (dist > npc.lifeMax * 6f && timer > 60)
                return nameof(Attack2);
            return null;
        }
        public static string? Attack2(NPC npc, int timer)
        {
            npc.target = TargetCollective.PullTarget(new NPCTargetSource(npc), out TargetInfo info);
            float dist = AppxDistanceTo(npc, info.Position);
            npc.velocity *= .98f;
            if (timer > 180 || !CanNPCShoot(npc))
            {
                if (info.Position.X > npc.position.X)
                    return nameof(RotateCW);
                else
                    return nameof(RotateCCW);
            }
            if ((timer+1) % 60 == 0)
            {
                Projectile p = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(info.Position) * 5f, Main.rand.Next(npc.GetGlobalNPC<OtherworldNPC>().shootProj), npc.damage / 5, 0, Main.myPlayer);
                p.friendly = npc.friendly;
                p.hostile = !npc.friendly;
            }
            return null;
        }
    }
}
