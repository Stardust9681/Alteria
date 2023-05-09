using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using static OtherworldMod.Common.ChangeNPC.Utilities.NPCMethods;
using static OtherworldMod.Common.ChangeNPC.Utilities.OtherworldNPCSets;

namespace OtherworldMod.Common.ChangeNPC.AI
{
    public class AIStyle_004
    {
        public static void Load()
        {
            foreach (int i in new int[] { NPCID.EyeofCthulhu })
            {
                if (i > 0 && i < Behaviours.Length)
                {
                    Behaviours[i].Add(EOCMove1, nameof(EOCMove1));
                    Behaviours[i].Add(EOCMove2, nameof(EOCMove2));
                    Behaviours[i].Add(EOCMove3, nameof(EOCMove3));
                    Behaviours[i].Add(EOCAttack1, nameof(EOCAttack1));
                    Behaviours[i].Add(EOCAttack2, nameof(EOCAttack2));
                    Behaviours[i].Add(EOCAttack3, nameof(EOCAttack3));
                    Behaviours[i].Add(EOCAttack4, nameof(EOCAttack4));
                    Behaviours[i].Add(EOCAttack5, nameof(EOCAttack5));
                    Behaviours[i].Add(EOCPhase2, nameof(EOCPhase2));
                    Behaviours[i].Add(EOCSpawn1, nameof(EOCSpawn1));
                    Behaviours[i].Add(EOCSpawn2, nameof(EOCSpawn2));
                }
            }
        }
        public void Unload()
        {
            foreach (int i in new int[] { NPCID.EyeofCthulhu })
            {
                if (i > 0 && i < Behaviours.Length)
                    UnloadAI(i);
            }
        }
        //Move above player
        static string? EOCMove1(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            Vector2 npcCenter = npc.Center;
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            Vector2 targetCenter = target.Center;
            int targetDir = targetCenter.X < npcCenter.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = false;
            npc.rotation = (targetCenter - npcCenter).ToRotation() - MathHelper.PiOver2;
            Vector2 targetPos = targetCenter + new Vector2(0, -192);
            npc.velocity.X += targetDir * .07f;
            bool xReady = false;
            bool yReady = false;
            if (MathF.Abs(targetCenter.X - npcCenter.X) < 160)
            {
                npc.velocity.X *= .943f;
                xReady = true;
            }
            npc.velocity.Y += (targetPos.Y < npcCenter.Y ? -1 : 1) * (npc.confused ? -1 : 1) * .07f;
            if (MathF.Abs(targetPos.Y - npcCenter.Y) < 160)
            {
                npc.velocity.Y *= .932f;
                yReady = true;
            }
            if (xReady && yReady && timer > 300)
            {
                if (npc.life < npc.lifeMax * .55f)
                {
                    return nameof(EOCPhase2);
                }
                npc.velocity *= .5f;
                return nameof(EOCAttack1);
            }
            return null;
        }
        //Dash short
        static string? EOCAttack1(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            if (npc.life < npc.lifeMax * .5f)
            {
                return nameof(EOCPhase2);
            }
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = timer > 30;
            if (timer < 6)
            {
                Vector2 targetPos = target.Center + new Vector2(0, Main.rand.NextFloat(-4f, 4f));
                npc.rotation = Utils.AngleLerp(npc.rotation, (targetPos - npc.Center).ToRotation() - MathHelper.PiOver2, .75f);
            }
            if (timer == 30)
            {
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 12f;
            }
            else
            {
                if (timer > 30 && npc.velocity.LengthSquared() < 16)
                {
                    npc.velocity *= .5f;
                    float dist = AppxDistanceToTarget(npc, npcTarget);
                    if (dist < 250)
                        return nameof(EOCSpawn1);
                    if (dist > 250 && dist < 400)
                        return nameof(EOCAttack1);
                    if (dist > 400 && dist < 600)
                        return nameof(EOCAttack2);
                    else
                        return nameof(EOCAttack3);
                }
                npc.velocity *= .985f;
            }
            if (timer > 150)
            {
                npc.velocity *= .8f;
            }
            return null;
        }
        //Dash med
        static string? EOCAttack2(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            if (npc.life < npc.lifeMax * .5f)
            {
                return nameof(EOCPhase2);
            }
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = timer > 45;
            if (timer < 8)
            {
                Vector2 targetPos = target.Center + new Vector2(0, Main.rand.NextFloat(-8f, 8f));
                npc.rotation = Utils.AngleLerp(npc.rotation, (targetPos - npc.Center).ToRotation() - MathHelper.PiOver2, .65f);
            }
            if (timer == 45)
            {
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 15f;
            }
            else
            {
                if (timer > 45 && npc.velocity.LengthSquared() < 20)
                {
                    npc.velocity *= .5f;
                    float dist = AppxDistanceToTarget(npc, npcTarget);
                    if (dist < 250)
                        return nameof(EOCSpawn1);
                    if (dist > 250 && dist < 400)
                        return nameof(EOCAttack1);
                    if (dist > 400 && dist < 600)
                        return nameof(EOCAttack2);
                    else
                        return nameof(EOCAttack3);
                }
                npc.velocity *= .98f;
            }
            if (timer > 180)
            {
                npc.velocity *= .8f;
            }
            return null;
        }
        //Dash long
        static string? EOCAttack3(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            if (npc.life < npc.lifeMax * .5f)
            {
                return nameof(EOCPhase2);
            }
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = timer > 65;
            if (timer < 12)
            {
                Vector2 targetPos = target.Center + new Vector2(0, Main.rand.NextFloat(-16f, 16f));
                npc.rotation = Utils.AngleLerp(npc.rotation, (targetPos - npc.Center).ToRotation() - MathHelper.PiOver2, .55f);
            }
            if (timer == 65)
            {
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 18f;
            }
            else
            {
                if (timer > 65 && npc.velocity.LengthSquared() < 25)
                {
                    npc.velocity *= .5f;
                    float dist = AppxDistanceToTarget(npc, npcTarget);
                    if (dist < 250)
                        return nameof(EOCSpawn1);
                    if (dist > 250 && dist < 300)
                        return nameof(EOCAttack1);
                    if (dist > 400 && dist < 500)
                        return nameof(EOCAttack2);
                    else
                        return nameof(EOCAttack3);
                }
                npc.velocity *= .975f;
            }
            if (timer > 210)
            {
                npc.velocity *= .8f;
            }
            return null;
        }
        //Spawn servants
        static string? EOCSpawn1(NPC npc, int timer)
        {
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = false;
            if (gNPC.spawnNPC == null || gNPC.spawnNPC.Length == 0 || gNPC.spawnNPC[0] == 0)
            {
                return nameof(EOCMove1);
            }
            npc.rotation = Utils.AngleLerp(npc.rotation, (target.Center - npc.Center).ToRotation() - MathHelper.PiOver2, .1f);
            npc.velocity *= .9f;
            if (timer % 30 == 0)
            {
                NPC servant = NPC.NewNPCDirect(npc.GetSource_FromAI(), npc.Center, gNPC.spawnNPC[0], target: npc.target);
                servant.velocity = Vector2.UnitX * Main.rand.Next(new float[] { -3.6f, 3.6f });
                servant.friendly = npc.friendly;
            }
            if (Main.expertMode || Main.masterMode)
            {
                if (timer + 15 % 30 == 0)
                {
                    NPC servant = NPC.NewNPCDirect(npc.GetSource_FromAI(), npc.Center, gNPC.spawnNPC[0], target: npc.target);
                    servant.velocity = Vector2.UnitX * Main.rand.Next(new float[] { -4.8f, 4.8f });
                    servant.friendly = npc.friendly;
                }
            }
            if (timer > 90)
            {
                return nameof(EOCMove1);
            }
            return null;
        }
        //Make the transition to phase 2, uses methods below
        static string? EOCPhase2(NPC npc, int timer)
        {
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = false;
            npc.velocity *= .99f;
            npc.rotation = ((MathHelper.Pi * MathF.Sin(MathHelper.Pi * timer * .125f)) / 6);
            if (gNPC.spawnNPC != null && gNPC.spawnNPC.Length != 0 && gNPC.spawnNPC[0] != 0)
            {
                if (timer % 25 == 0)
                {
                    NPC servant = NPC.NewNPCDirect(npc.GetSource_FromAI(), npc.Center, gNPC.spawnNPC[0], target: npc.target);
                    servant.velocity = (Vector2.UnitX * Main.rand.Next(new float[] { -3.6f, 3.6f })).RotatedByRandom(.524f);
                    servant.friendly = npc.friendly;
                }
            }
            if (timer > 200)
            {
                //Spawn gores
                return nameof(EOCAttack4);
            }
            return null;
        }
        //Phase2 move above player
        static string? EOCMove2(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            Vector2 npcCenter = npc.Center;
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            Vector2 targetCenter = target.Center;
            int targetDir = targetCenter.X < npcCenter.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = false;
            npc.rotation = (targetCenter - npcCenter).ToRotation() - MathHelper.PiOver2;
            Vector2 targetPos = targetCenter + new Vector2(0, -160);
            npc.velocity.X += targetDir * .1f;
            bool xReady = false;
            bool yReady = false;
            if (MathF.Abs(targetCenter.X - npcCenter.X) < 144)
            {
                npc.velocity.X *= .92f;
                xReady = true;
            }
            npc.velocity.Y += (targetPos.Y < npcCenter.Y ? -1 : 1) * (npc.confused ? -1 : 1) * .1f;
            if (MathF.Abs(targetPos.Y - npcCenter.Y) < 144)
            {
                npc.velocity.Y *= .84f;
                yReady = true;
            }
            if (xReady && yReady && timer > 180)
            {
                npc.velocity *= .5f;
                return nameof(EOCAttack4);
            }
            return null;
        }
        //Phase2 dash med
        static string? EOCAttack4(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = timer > 90;
            if (timer == 90)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 16f;
            }
            else
            {
                if (timer < 90)
                {
                    npc.rotation = Utils.AngleLerp(npc.rotation, (target.Center - npc.Center).ToRotation() - MathHelper.PiOver2, .1f);
                }
                else if (npc.velocity.LengthSquared() < 20)
                {
                    npc.velocity *= .5f;
                    if (Main.expertMode || Main.masterMode)
                    {
                        return nameof(EOCAttack5);
                    }
                    else
                    {
                        return nameof(EOCSpawn2);
                    }
                }
                npc.velocity *= .985f;
            }
            if (timer > 210)
            {
                npc.velocity *= .8f;
            }
            return null;
        }
        //Phase2 hyperdash
        static string? EOCAttack5(NPC npc, int timer)
        {
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            float healthFactor = ((float)npc.life / (float)npc.lifeMax);
            if (timer < 5)
            {
                Vector2 targetPos = target.Center + new Vector2(Main.rand.NextFloat(-128f, 128f) * (1 - (2 * healthFactor)), Main.rand.NextFloat(-256f, 256f) * (1 - (2 * healthFactor)));
                npc.rotation = Utils.AngleLerp(npc.rotation, (targetPos - npc.Center).ToRotation() - MathHelper.PiOver2, MathHelper.Clamp((1 - (healthFactor)) * .8f, 0, 1));
            }
            int pauseDur = (int)MathHelper.Lerp(5, 30, healthFactor * 2f);
            gNPC.allowContactDmg = timer > pauseDur;
            float velMult = MathHelper.Lerp(20, 30, 1 - (2 * healthFactor));
            if (timer == pauseDur)
            {
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * velMult;
            }
            else
            {
                if (timer > pauseDur && npc.velocity.LengthSquared() < velMult * 2f)
                {
                    npc.velocity *= .5f;
                    if (AppxDistanceToTarget(npc, npcTarget) < 125)
                        return nameof(EOCMove2);
                    if (Main.expertMode || Main.masterMode)
                    {
                        if (Main.rand.NextFloat() > 1.33f * healthFactor)
                            return nameof(EOCAttack5);
                        return nameof(EOCSpawn2);
                    }
                    else
                    {
                        return nameof(EOCSpawn2);
                    }
                }
                npc.velocity *= (-.08f * healthFactor) + .975f;
            }
            if (timer > 90)
            {
                npc.velocity *= .8f;
            }
            return null;
        }
        //Phase2 spawn servants
        static string? EOCSpawn2(NPC npc, int timer)
        {
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = false;
            if (gNPC.spawnNPC == null || gNPC.spawnNPC.Length == 0 || gNPC.spawnNPC[0] == 0)
            {
                return nameof(EOCMove2);
            }
            npc.rotation = Utils.AngleLerp(npc.rotation, (target.Center - npc.Center).ToRotation() - MathHelper.PiOver2, .1f);
            npc.velocity *= .9f;
            int npcType = Main.rand.Next(gNPC.spawnNPC);
            if (NPC.CountNPCS(npcType) > 2 || timer > 90)
            {
                return nameof(EOCMove2);
            }
            if (timer % 45 == 0)
            {
                NPC servant = NPC.NewNPCDirect(npc.GetSource_FromAI(), npc.Center, npcType, target: npc.target);
                servant.velocity = Vector2.UnitX.RotatedBy(npc.rotation) * (npc.confused ? -1 : 1);
                servant.friendly = npc.friendly;
                servant.AddBuff(BuffID.BloodButcherer, 36000);
                servant.SpawnedFromStatue = true;
            }
            return null;
        }
        //Daytime
        static string? EOCMove3(NPC npc, int timer)
        {
            npc.velocity.Y -= .1f;
            return null;
        }
    }
}
