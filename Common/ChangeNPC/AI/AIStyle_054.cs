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

namespace Alteria.Common.ChangeNPC.AI
{
				/// <summary>
				///<see cref="Terraria.ID.NPCAIStyleID.BrainOfCthulhu"/>
				/// </summary>
				public class AIStyle_054 : AIStyleType
				{
								protected override ITargetable SetDefaultTarget(int npcIndex)
								{
												return new NPCTarget<AIStyle_054>(npcIndex);
								}
								public override void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar)
								{
												info.Position = Main.npc[npcIndex].Center;
												info.faction = Faction.UnivHostile;
								}
								protected override int[] ApplicableNPCs => new int[] { NPCID.BrainofCthulhu };
								public override void Load()
								{
												AddAI(Reset, Observe, PhaseOut, PhaseIn, AttackPhase1);
								}
								private static string? Reset(NPC npc, int timer)
								{
												npc.dontTakeDamage = false;
												NPC.crimsonBoss = npc.whoAmI;
												if (Main.netMode != NetmodeID.MultiplayerClient)
												{
																//Change this to npc.localAI[0] maybe?
																int spawnCount = NPC.GetBrainOfCthuluCreepersCount();
																TargetCollective.TryFindTarget(GetNPC_1(npc).NPCTarget, out int thisIndex);
																for (int i = 0; i < spawnCount; i++)
																{
																				NPC.NewNPCDirect(npc.GetSource_FromAI(), npc.position + new Vector2(-npc.width, npc.height * .5f), GetNPC_1(npc).spawnNPC?.First() ?? NPCID.Creeper, ai1:thisIndex).netUpdate = true;
																}
												}
												return nameof(Observe);
								}
								private static string? Observe(NPC npc, int timer)
								{
												npc.target = PullTarget(npc, out TargetInfo info);
												int all = TargetCollective.CountAll(x => { return x is NPCTarget<AIStyle_054>; });
												int count = TargetCollective.CountTargetsInRange(info.Position, 96f, x => { return x is NPCTarget<AIStyle_054>; } );
												if (count >= all / 2)
												{
																return nameof(PhaseOut);
												}
												return nameof(Observe);
								}
								private static string? PhaseOut(NPC npc, int timer)
								{
												if (!TargetCollective.TryGetTarget(npc.target, out ITargetable target))
												{
																npc.alpha = 0;
																return nameof(Observe);
												}
												npc.alpha = timer * 2;
												if (timer > 127)
																return nameof(PhaseIn);
												return null;
								}
								private static string? PhaseIn(NPC npc, int timer)
								{
												if (!TargetCollective.TryGetTarget(npc.target, out ITargetable target))
												{
																npc.alpha = 0;
																return nameof(Observe);
												}
												npc.position = target.GetInfo(GetNPC_1(npc).NPCRadar).Position + new Vector2(-npc.width/2, -npc.height * 2);
												npc.alpha-=2;
												if (timer > 127)
												{
																npc.alpha = 0;
																return nameof(AttackPhase1);
												}
												return null;
								}
								private static string? AttackPhase1(NPC npc, int timer)
								{
												if (timer < 15)
																npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, -2f, .1f);
												else
																npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, 8f, .1f);
												if (timer > 60)
												{
																npc.velocity.Y = 0;
																return nameof(Observe);
												}
												return null;
								}
				}
}
