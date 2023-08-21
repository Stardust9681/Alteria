﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Alteria.Core.Util.Utils;
using static Alteria.Common.ChangeNPC.Utilities.NPCMethods;
using static Alteria.Common.ChangeNPC.Utilities.AlteriaNPCSets;
using Alteria.Core.Util;

namespace Alteria.Common.ChangeNPC.AI
{
#nullable enable
    /// <summary>
    /// <see cref="NPCAIStyleID.Flying"/>
    /// </summary>
    public class AIStyle_005 : AIStyleType
    {
        protected override ITargetable SetDefaultTarget(int npcIndex)
        {
            return new Core.Util.NPCTarget<AIStyle_005>(npcIndex);
        }
        public override void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar)
        {
            info.Position = Main.npc[npcIndex].position;
        }
        protected override int[] ApplicableNPCs => new int[] { NPCID.BigHornetStingy, NPCID.LittleHornetStingy, NPCID.BigHornetSpikey,
                NPCID.LittleHornetSpikey, NPCID.BigHornetLeafy, NPCID.LittleHornetLeafy, NPCID.BigHornetHoney, NPCID.LittleHornetHoney,
                NPCID.BigHornetFatty, NPCID.LittleHornetFatty, NPCID.BigCrimera, NPCID.LittleCrimera, NPCID.GiantMossHornet,
                NPCID.BigMossHornet, NPCID.LittleMossHornet, NPCID.TinyMossHornet, NPCID.BigStinger, NPCID.LittleStinger, NPCID.BigEater,
                NPCID.LittleEater, NPCID.ServantofCthulhu, NPCID.EaterofSouls, NPCID.MeteorHead, NPCID.Hornet, NPCID.Corruptor,
                NPCID.Probe, NPCID.Crimera, NPCID.MossHornet, NPCID.Moth, NPCID.Bee, NPCID.BeeSmall, NPCID.HornetFatty,
                NPCID.HornetHoney, NPCID.HornetLeafy, NPCID.HornetSpikey, NPCID.HornetStingy, NPCID.Parrot, NPCID.BloodSquid };
        public override void Load()
        {
            AddAI(FlierMove1, FlierMove2, FlierAttack1, FlierAttack2, Idle);
        }

        //cw circle around player
        static string? FlierMove1(NPC npc, int timer)
        {
            AlteriaNPC gNPC = GetNPC_1(npc);
            gNPC.allowContactDmg = false;
            //Find target
            npc.target = PullTarget(npc, out TargetInfo info);
            int targetDir = npc.position.X > info.Position.X ? -1 : 1;
            float appxDist = AppxDistanceTo(npc, info.Position);
            Vector2 offset = npc.DirectionTo(info.Position);
            if (appxDist < npc.damage * 6f)
                npc.velocity -= new Vector2(offset.X, (offset.Y + 1) * .5f) * .3f;
            if (appxDist > npc.damage * 16f)
                npc.velocity += offset * .15f;
            npc.rotation = offset.ToRotation() - MathHelper.PiOver2;
            float rotation = (((timer * timer) * .001f) % MathHelper.PiOver2) + (3 * MathHelper.PiOver2);
            offset = Vector2.UnitX.RotatedBy(rotation);
            Vector2 targetPos = info.Position + (offset * npc.damage * (npc.confused ? -8f : 8f));
            npc.velocity.X += (npc.position.X > targetPos.X) ? -.04f : .04f;
            npc.velocity.Y += (npc.position.Y > targetPos.Y) ? -.04f : .04f;
            if (MathF.Abs(npc.position.X - targetPos.X) < 64)
                npc.velocity.X *= .97f;
            if (MathF.Abs(npc.position.Y - targetPos.Y) < 64)
                npc.velocity.Y *= .97f;

            if (targetDir < 0 && timer > 120)
                return nameof(FlierMove2);

            if (timer > 120 && appxDist < npc.damage * 9f && MathF.Abs(npc.position.X - targetPos.X) < 96)
            {
                npc.velocity *= 0f;
                return nameof(FlierAttack2);
            }
            return null;
        }
        //ccw circle around player
        static string? FlierMove2(NPC npc, int timer)
        {
            AlteriaNPC gNPC = GetNPC_1(npc);
            gNPC.allowContactDmg = false;
            //Find target
            npc.target = PullTarget(npc, out TargetInfo info);
            int targetDir = npc.position.X > info.Position.X ? -1 : 1;
            float appxDist = AppxDistanceTo(npc, info.Position);
            Vector2 offset = npc.DirectionTo(info.Position);
            if (appxDist < npc.damage * 6f)
                npc.velocity -= new Vector2(offset.X, (offset.Y+1)*.5f) * .3f;
            if (appxDist > npc.damage * 16f)
                npc.velocity += offset * .15f;
            npc.rotation = offset.ToRotation() - MathHelper.PiOver2;
            float rotation = (-(((timer*timer) * .001f) % MathHelper.PiOver2)) + (3 * MathHelper.PiOver2);
            offset = Vector2.UnitX.RotatedBy(rotation);
            Vector2 targetPos = info.Position + (offset * npc.damage * (npc.confused ? -8f : 8f));
            npc.velocity.X += (npc.position.X > targetPos.X) ? -.04f : .04f;
            npc.velocity.Y += (npc.position.Y > targetPos.Y) ? -.04f : .04f;
            if (MathF.Abs(npc.position.X - targetPos.X) < 64)
                npc.velocity.X *= .97f;
            if (MathF.Abs(npc.position.Y - targetPos.Y) < 64)
                npc.velocity.Y *= .97f;

            if (targetDir > 0 && timer > 120)
                return nameof(FlierMove1);

            if (timer > 120 && appxDist < npc.damage * 9f && MathF.Abs(npc.position.X - targetPos.X) < 96)
            {
                return nameof(FlierAttack2);
            }
            return null;
        }
        //move quickly towards player
        static string? FlierAttack1(NPC npc, int timer)
        {
            AlteriaNPC gNPC = GetNPC_1(npc);
            //Find target
            npc.target = PullTarget(npc, out TargetInfo info);
            float lenSQ = npc.velocity.LengthSquared();
            int targetDir = npc.position.X > info.Position.X ? -1 : 1;
            npc.GetGlobalNPC<AlteriaNPC>().allowContactDmg = lenSQ > 9f;
            if (timer < 20)
            {
                Vector2 offset = npc.DirectionTo(info.Position);
                npc.rotation = offset.ToRotation() - MathHelper.PiOver2;
                npc.velocity = offset * MathF.Sin(timer * MathHelper.Pi * .167f) * 2.4f;
                return null;
            }
            npc.velocity += Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * .28f;
            float appxDist = AppxDistanceTo(npc, info.Position);
            if (npc.collideX || npc.collideY || timer > 180 || appxDist > 550)
            {
                return nameof(Idle);
            }
            if (appxDist > 350 && targetDir != (npc.velocity.X < 0 ? -1 : 1))
            {
                npc.velocity *= .8f;
            }
            return null;
        }
        //fire projectile(s) if applicable
        static string? FlierAttack2(NPC npc, int timer)
        {
            //Find target
            npc.target = PullTarget(npc, out TargetInfo info);
            if (timer < 30)
            {
                npc.velocity *= .9f;
            }
            else
            {
                if (!CanNPCShoot(npc))
                {
                    return nameof(FlierAttack1);
                }
                if (timer % 60 == 0)
                {
                    Projectile p = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(info.Position) * 5f, Main.rand.Next(npc.GetGlobalNPC<AlteriaNPC>().shootProj), npc.damage / 5, 0, Main.myPlayer);
                    p.friendly = npc.friendly;
                    p.hostile = !npc.friendly;
                    if (timer > 120)
                    {
                        return nameof(FlierAttack1);
                    }
                }
            }
            return null;
        }

        static string? Idle(NPC npc, int timer)
        {
            npc.target = PullTarget(npc, out TargetInfo info);
            npc.velocity *= .99f;
            npc.rotation = (info.Position - npc.position).ToRotation();
            if (timer > 90)
            {
                int targetDir = npc.position.X > info.Position.X ? -1 : 1;
                if (targetDir > 0)
                    return nameof(FlierMove1);
                else
                    return nameof(FlierMove2);
            }
            return null;
        }
    }
}
