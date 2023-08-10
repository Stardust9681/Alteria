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
using OtherworldMod.Common.Interface;
using OtherworldMod.Common.Structure;

namespace OtherworldMod.Common.ChangeNPC.AI
{
    public class AIStyle_055 : AIStyleType
    {
        protected override ITargetable SetDefaultTarget(int npcIndex)
        {
            return new Core.Util.NPCTarget<AIStyle_055>(npcIndex);
        }
        public override void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar)
        {
            info.Position = Main.npc[npcIndex].position;
        }
        protected override int[] ApplicableNPCs => new int[] { NPCID.Creeper };
        public override void Load()
        {
            AddAI(ShieldBrain);
        }

        private static string? ShieldBrain(NPC npc, int timer)
        {
            float t = 900;
            float r = 3f;

            if (timer > t)
                return nameof(Attack1);

            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;

            npc.target = PullTarget(npc, out TargetInfo info);
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

        private static string? Attack1(NPC npc, int timer)
        {
            npc.target = PullTarget(npc, out TargetInfo info);
            return null;
        }
    }
}
