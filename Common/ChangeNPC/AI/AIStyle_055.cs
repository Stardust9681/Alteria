using System;
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
using Alteria.Common.Interface;
using Alteria.Common.Structure;

namespace Alteria.Common.ChangeNPC.AI
{
    /// <summary>
    /// <see cref="NPCAIStyleID.Creeper"/>
    /// </summary>
    public class AIStyle_055 : AIStyleType
    {
        protected override ITargetable SetDefaultTarget(int npcIndex)
        {
												return new Core.Util.NPCTarget<AIStyle_055>(npcIndex);
        }
        public override void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar)
        {
            info.Position = Main.npc[npcIndex].position;
												info.faction = Faction.UnivHostile | Faction.FlagSupport;
        }
        protected override int[] ApplicableNPCs => new int[] { NPCID.Creeper };
        public override void Load()
        {
            AddAI(ShieldSmall, Charge, Return);
        }

        private static string? ShieldSmall(NPC npc, int timer)
        {
            float t = 120;
            float r = 3f;

            if (timer > t + (npc.whoAmI * 15))
                return nameof(Charge);

												if (!TargetCollective.TryGetTarget((int)npc.ai[1], out ITargetable src))
												{
																//If ai[1] is out of bounds...
																if (npc.ai[1] < 0 || npc.ai[1] > TargetCollective.CountAll())
																{
																				try
																				{
																								if (TargetCollective.TryFindTarget(Main.npc.First(x => x.active = true && x.aiStyle == NPCAIStyleID.BrainOfCthulhu).GetGlobalNPC<AlteriaNPC>().NPCTarget, out int ai1))
																								{
																												npc.ai[1] = ai1;
																								}
																								else
																								{
																												npc.ai[1] = npc.whoAmI;
																								}
																				}
																				catch
																				{
																								npc.ai[1] = npc.whoAmI;
																				}
																}
																return nameof(ShieldSmall);
												}
												TargetInfo info = src.GetInfo(GetNPC_1(npc).NPCRadar);

            float dist = AppxDistanceTo(npc, info.Position);

            if (dist > 800)
                npc.velocity *= .86f;

            int xCubed = timer * timer * timer;
            float tSq = t * t;
            float rotation = r * (timer - (xCubed / (3 * tSq)));
            rotation += (npc.whoAmI * (360f / NPC.CountNPCS(npc.type)));
            Vector2 targetPos = info.Position + (Vector2.UnitX.RotatedBy(MathHelper.ToRadians(rotation)) * (240f + Main.rand.NextFloat(-64, 64)));

            //npc.velocity.X = targetPos.X < npc.position.X ? -3f : 3f;
            //npc.velocity.Y = targetPos.Y < npc.position.Y ? -3f : 3f;
            float distSq = Vector2.DistanceSquared(npc.position, targetPos);
            npc.velocity = npc.DirectionTo(targetPos) * MathHelper.Lerp(2, 5.75f, (distSq - 625) / 14000f);

            //Don't deal contact damage if NPC is too far from the position it should move to
            if (distSq < 3200)
            {
                npc.GetGlobalNPC<AlteriaNPC>().allowContactDmg = true;
                npc.velocity *= .34f;
            }
            else
            {
                npc.GetGlobalNPC<AlteriaNPC>().allowContactDmg = false;
            }
            //npc.position = targetPos;

            npc.rotation = (targetPos - npc.Center).ToRotation();
            npc.spriteDirection = targetPos.X > npc.position.X ? -1 : 1;
            if (npc.spriteDirection == 1)
                npc.rotation += MathHelper.Pi;

            return null;
        }

        private static string? Charge(NPC npc, int timer)
        {
            if (timer > 135)
                return nameof(Return);

            npc.target = PullTarget(npc, out TargetInfo info);

            if (timer > 75 && Vector2.DistanceSquared(npc.position, info.Position) < 1024)
                return nameof(Return);

            if (timer < 15)
                npc.velocity = Vector2.UnitX.RotatedBy(-MathHelper.ToRadians(timer * 48)) * timer * .5f;
            else if (timer < 20)
                npc.velocity = npc.DirectionTo(info.Position) * 15f;
            else
                npc.velocity *= .99f;
            npc.GetGlobalNPC<AlteriaNPC>().allowContactDmg = timer > 45;

            return null;
        }

        private static string Return(NPC npc, int timer)
        {
            if (timer > 120)
                return nameof(ShieldSmall);

												if (!TargetCollective.TryGetTarget((int)npc.ai[1], out ITargetable src))
												{
																//If ai[1] is out of bounds...
																if (npc.ai[1] < 0 || npc.ai[1] > TargetCollective.CountAll())
																{
																				try
																				{
																								if (TargetCollective.TryFindTarget(Main.npc.First(x => x.active = true && x.aiStyle == NPCAIStyleID.BrainOfCthulhu).GetGlobalNPC<AlteriaNPC>().NPCTarget, out int ai1))
																								{
																												npc.ai[1] = ai1;
																								}
																								else
																								{
																												npc.ai[1] = npc.whoAmI;
																								}
																				}
																				catch
																				{
																								npc.ai[1] = npc.whoAmI;
																				}
																}
																return nameof(ShieldSmall);
												}
												TargetInfo info = src.GetInfo(GetNPC_1(npc).NPCRadar);

												if (timer > 45 && Vector2.DistanceSquared(npc.position, info.Position) < 57600)
                return nameof(ShieldSmall);

            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(info.Position) * 6f, .075f);
            npc.GetGlobalNPC<AlteriaNPC>().allowContactDmg = false;

            return null;
        }
    }
}
