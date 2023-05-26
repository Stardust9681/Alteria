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
    /// <summary>
    /// <see cref="NPCAIStyleID.CursedSkull"/>
    /// </summary>
    public class AIStyle_010
    {
        public static void Load()
        {
            foreach (int i in new int[] { NPCID.CursedSkull, NPCID.GiantCursedSkull })
            {
                Behaviours[i].Add(RotateCW, nameof(RotateCW));
                Behaviours[i].Add(RotateCCW, nameof(RotateCCW));
                Behaviours[i].Add(Attack1, nameof(Attack1));
                Behaviours[i].Add(Attack2, nameof(Attack2));
            }
        }
        public static void Unload()
        {
            foreach (int i in new int[] { NPCID.CursedSkull, NPCID.GiantCursedSkull })
            {
                UnloadAI(i);
            }
        }
        const float t = 360;
        const float r = 360f/90f;
        public static string? RotateCW(NPC npc, int timer)
        {
            if (timer > t)
                return nameof(Attack1);

            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;

            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);

            if (dist > 800)
                npc.velocity *= .86f;

            int xCubed = timer * timer * timer;
            float tSq = t * t;
            float rotation = r * (timer - (xCubed / (3 * tSq)));
            rotation += (npc.whoAmI * 45);
            Vector2 targetPos = target.Center + Vector2.UnitX.RotatedBy(MathHelper.ToRadians(rotation)) * npc.lifeMax * 2.5f;

            //npc.velocity.X = targetPos.X < npc.position.X ? -3f : 3f;
            //npc.velocity.Y = targetPos.Y < npc.position.Y ? -3f : 3f;
            float distSq = Vector2.DistanceSquared(npc.position, targetPos);
            npc.velocity = npc.DirectionTo(targetPos) * MathHelper.Lerp(2, 5.75f, (distSq - 625) / 16000f);
            if (distSq < 3200)
                npc.velocity *= .34f;
            //npc.position = targetPos;

            npc.rotation = (target.Center - npc.Center).ToRotation();
            npc.spriteDirection = target.position.X > npc.position.X ? -1 : 1;
            if (npc.spriteDirection == 1)
                npc.rotation += MathHelper.Pi;

            return null;
        }
        public static string? RotateCCW(NPC npc, int timer)
        {
            if (timer > t)
                return nameof(Attack1);

            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;

            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);

            if (dist > 800)
                npc.velocity *= .86f;

            int xCubed = timer * timer * timer;
            float tSq = t * t;
            float rotation = r * (timer - (xCubed / (3 * tSq)));
            rotation += (npc.whoAmI * 45);
            Vector2 targetPos = target.Center - Vector2.UnitX.RotatedBy(-MathHelper.ToRadians(rotation)) * npc.lifeMax * 2.5f;

            //npc.velocity.X = targetPos.X < npc.position.X ? -3f : 3f;
            //npc.velocity.Y = targetPos.Y < npc.position.Y ? -3f : 3f;
            float distSq = Vector2.DistanceSquared(npc.position, targetPos);
            npc.velocity = npc.DirectionTo(targetPos) * MathHelper.Lerp(2, 5.75f, (distSq-625)/16000f);
            if (distSq < 3200)
                npc.velocity *= .34f;
            //npc.position = targetPos;

            npc.rotation = (target.Center - npc.Center).ToRotation();
            npc.spriteDirection = target.position.X > npc.position.X ? -1 : 1;
            if (npc.spriteDirection == 1)
                npc.rotation += MathHelper.Pi;

            return null;
        }
        public static string? Attack1(NPC npc, int timer)
        {
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);

            Vector2 targetPos = target.Center;
            if (timer == 45)
                npc.velocity = npc.DirectionTo(targetPos) * 8f;
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = npc.velocity.LengthSquared() > 4f;

            if (dist > npc.lifeMax * 6f && timer > 60)
                return nameof(Attack2);
            return null;
        }
        public static string? Attack2(NPC npc, int timer)
        {
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);
            npc.velocity *= .98f;
            if (timer > 180 || !CanNPCShoot(npc))
            {
                if (target.direction > 0)
                    return nameof(RotateCW);
                else
                    return nameof(RotateCCW);
            }
            if ((timer+1) % 60 == 0)
            {
                Projectile p = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(target.Center) * 5f, Main.rand.Next(npc.GetGlobalNPC<OtherworldNPC>().shootProj), npc.damage / 5, 0, Main.myPlayer);
                p.friendly = npc.friendly;
                p.hostile = !npc.friendly;
            }
            return null;
        }
    }
}
