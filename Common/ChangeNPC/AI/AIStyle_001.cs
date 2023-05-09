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
    public class AIStyle_001
    {
        public static void Load()
        {
            foreach (int i in new int[] { NPCID.BigCrimslime, NPCID.LittleCrimslime, NPCID.JungleSlime, NPCID.YellowSlime,
                NPCID.RedSlime, NPCID.PurpleSlime, NPCID.BlackSlime, NPCID.BabySlime, NPCID.Pinky, NPCID.GreenSlime, NPCID.Slimer2,
                NPCID.Slimeling, NPCID.BlueSlime, NPCID.MotherSlime, NPCID.LavaSlime, NPCID.DungeonSlime, NPCID.CorruptSlime,
                NPCID.IlluminantSlime, NPCID.ToxicSludge, NPCID.IceSlime, NPCID.Crimslime, NPCID.SpikedIceSlime, NPCID.SpikedJungleSlime,
                NPCID.UmbrellaSlime, NPCID.RainbowSlime, NPCID.SlimeMasked, NPCID.HoppinJack, NPCID.SlimeRibbonWhite,
                NPCID.SlimeRibbonYellow, NPCID.SlimeRibbonGreen, NPCID.SlimeRibbonRed, NPCID.Grasshopper, NPCID.GoldGrasshopper,
                NPCID.SlimeSpiked, NPCID.SandSlime, NPCID.QueenSlimeMinionBlue, NPCID.QueenSlimeMinionPink, NPCID.GoldenSlime })
            {
                if (i > 0 && i < Behaviours.Length)
                {
                    Behaviours[i].Add(SlimeJump1, nameof(SlimeJump1));
                    Behaviours[i].Add(SlimeJump2, nameof(SlimeJump2));
                    Behaviours[i].Add(SlimeJump3, nameof(SlimeJump3));
                    Behaviours[i].Add(SlimeShoot1, nameof(SlimeShoot1));
                    Behaviours[i].Add(SlimeShoot2, nameof(SlimeShoot2));
                    Behaviours[i].Add(SlimeWet, nameof(SlimeWet));
                }
            }
        }
        public static void Unload()
        {
            foreach (int i in new int[] { NPCID.BigCrimslime, NPCID.LittleCrimslime, NPCID.JungleSlime, NPCID.YellowSlime,
                NPCID.RedSlime, NPCID.PurpleSlime, NPCID.BlackSlime, NPCID.BabySlime, NPCID.Pinky, NPCID.GreenSlime, NPCID.Slimer2,
                NPCID.Slimeling, NPCID.BlueSlime, NPCID.MotherSlime, NPCID.LavaSlime, NPCID.DungeonSlime, NPCID.CorruptSlime,
                NPCID.IlluminantSlime, NPCID.ToxicSludge, NPCID.IceSlime, NPCID.Crimslime, NPCID.SpikedIceSlime, NPCID.SpikedJungleSlime,
                NPCID.UmbrellaSlime, NPCID.RainbowSlime, NPCID.SlimeMasked, NPCID.HoppinJack, NPCID.SlimeRibbonWhite,
                NPCID.SlimeRibbonYellow, NPCID.SlimeRibbonGreen, NPCID.SlimeRibbonRed, NPCID.Grasshopper, NPCID.GoldGrasshopper,
                NPCID.SlimeSpiked, NPCID.SandSlime, NPCID.QueenSlimeMinionBlue, NPCID.QueenSlimeMinionPink, NPCID.GoldenSlime })
            {
                if (i > 0 && i < Behaviours.Length)
                    UnloadAI(i);
            }
        }
        static string? SlimeWet(NPC npc, int timer)
        {
            //Move to slime jump AI if NPC is no longer wet.
            if (!(npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet))
            {
                return nameof(SlimeJump1);
            }
            //Turn on hitbox, slimes causing contact damage in water is fine by me, and discourages exploits with water buckets
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
            //Find a target, true if NPC found
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            //Direction to target (account for confusion)
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            //Accelerate towards target until vel >= 6
            if (MathF.Abs(npc.velocity.X) < 6)
            {
                npc.velocity.X += targetDir * .14f;
            }
            //Else slow down a bit
            else
            {
                npc.velocity.X *= .98f;
            }
            //Swim in water
            npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y - .96f, -4.8f, npc.velocity.Y);
            return null;
        }
        //Same logic as SlimeJump2 and SlimeJump3, read comments here for both
        static string? SlimeJump1(NPC npc, int timer)
        {
            //Move to wet AI if NPC is wet
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(SlimeWet);
            }
            //Find a target, true if NPC found
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            //Direction to target (account for confusion)
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            if (npc.collideY)
            {
                //If npc collided with tiles, moving downwards
                if (npc.velocity.Y > -.04f)
                {
                    //Stop moving
                    npc.velocity.X = 0;
                    CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
                    //Disable contact damage on ground
                    gNPC.allowContactDmg = false;
                    //Check if NPC can shoot projectiles
                    bool canShoot = gNPC.shootProj != null && gNPC.shootProj.Length > 0 && gNPC.shootProj[0] != 0;
                    if (canShoot)
                    {
                        //Determine if NPC should move to shoot phase, and if so, which one.
                        float dist = AppxDistanceToTarget(npc, npcTarget);
                        if (timer < 120)
                        {
                            if (dist < 600)
                            {
                                if (dist < 240)
                                {
                                    return nameof(SlimeShoot2);
                                }
                                return nameof(SlimeShoot1);
                            }
                        }
                    }
                    //Jump if timer has passed 80 ticks (1.33 sec)
                    if (timer >= 80 && timer < 85)
                    {
                        npc.velocity = new Vector2(targetDir * 3.8f, -6.4f);
                        npc.position.Y -= 4f;
                    }
                    //Slime tried to jump, move to next jump phase
                    else if (timer >= 85)
                    {
                        return nameof(SlimeJump2);
                    }
                }
                //Else if NPC collided with tiles, moving upwards
                else
                {
                    //Move down a bit to make sure no extra collisions occur
                    npc.position.Y += 1.2f;
                    //If NPC stopped moving horizontally, try to maintain movement
                    if (npc.velocity.X == 0)
                        npc.velocity.X = npc.oldVelocity.X;
                }
            }
            //Else if NPC is in the air
            else
            {
                //If NPC moved past its target direction, slow down drastically
                if (targetDir != (npc.velocity.X < 0 ? -1 : 1))
                {
                    npc.velocity.X *= .943f;
                }
                //If NPC is falling, speed up, and allow contact damage
                if (npc.velocity.Y > 0)
                {
                    if (npc.velocity.Y < 32)
                        npc.velocity.Y *= 1.14f;
                    npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
                }
                //If NPC stopped moving horizontally, try to maintain movement
                if (npc.velocity.X == 0)
                    npc.velocity.X = npc.oldVelocity.X;
                return null;
            }
            return null;
        }
        //Comments in SlimeJump1
        static string? SlimeJump2(NPC npc, int timer)
        {
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(SlimeWet);
            }
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            if (npc.collideY)
            {
                if (npc.velocity.Y > -.04f)
                {
                    npc.velocity.X = 0;
                    CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
                    gNPC.allowContactDmg = false;
                    bool canShoot = gNPC.shootProj != null && gNPC.shootProj.Length > 0 && gNPC.shootProj[0] != 0;
                    if (canShoot)
                    {
                        float dist = AppxDistanceToTarget(npc, npcTarget);
                        if (timer < 150)
                        {
                            if (dist < 600)
                            {
                                if (dist < 240)
                                {
                                    return nameof(SlimeShoot2);
                                }
                                return nameof(SlimeShoot1);
                            }
                        }
                    }
                    if (timer >= 130 && timer < 135)
                    {
                        npc.velocity = new Vector2(targetDir * 4.2f, -7.7f);
                        npc.position.Y -= 4f;
                    }
                    else if (timer >= 135)
                    {
                        return nameof(SlimeJump3);
                    }
                }
                else
                {
                    npc.position.Y += 1.2f;
                    if (npc.velocity.X == 0)
                        npc.velocity.X = npc.oldVelocity.X;
                }
            }
            else
            {
                if (targetDir != (npc.velocity.X < 0 ? -1 : 1))
                {
                    npc.velocity.X *= .943f;
                }
                if (npc.velocity.Y > 0)
                {
                    if (npc.velocity.Y < 32)
                        npc.velocity.Y *= 1.14f;
                    npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
                }
                if (npc.velocity.X == 0)
                    npc.velocity.X = npc.oldVelocity.X;
                return null;
            }
            return null;
        }
        //Comments in SlimeJump1
        static string? SlimeJump3(NPC npc, int timer)
        {
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(SlimeWet);
            }
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            if (npc.collideY)
            {
                if (npc.velocity.Y > -.04f)
                {
                    npc.velocity.X = 0;
                    CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
                    gNPC.allowContactDmg = false;
                    bool canShoot = gNPC.shootProj != null && gNPC.shootProj.Length > 0 && gNPC.shootProj[0] != 0;
                    if (canShoot)
                    {
                        float dist = AppxDistanceToTarget(npc, npcTarget);
                        if (timer < 180)
                        {
                            if (dist < 600)
                            {
                                if (dist < 240)
                                {
                                    return nameof(SlimeShoot2);
                                }
                                return nameof(SlimeShoot1);
                            }
                        }
                    }
                    if (timer >= 180 && timer < 185)
                    {
                        npc.velocity = new Vector2(targetDir * 4.6f, -9.2f);
                        npc.position.Y -= 4f;
                    }
                    else if (timer >= 185)
                    {
                        return nameof(SlimeJump1);
                    }
                }
                else
                {
                    npc.position.Y += 1.2f;
                    if (npc.velocity.X == 0)
                        npc.velocity.X = npc.oldVelocity.X;
                }
            }
            else
            {
                if (targetDir != (npc.velocity.X < 0 ? -1 : 1))
                {
                    npc.velocity.X *= .943f;
                }
                if (npc.velocity.Y > 0)
                {
                    if (npc.velocity.Y < 32)
                        npc.velocity.Y *= 1.14f;
                    npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
                }
                if (npc.velocity.X == 0)
                    npc.velocity.X = npc.oldVelocity.X;
                return null;
            }
            return null;
        }
        //Same logic as SlimeShoot2, read comments here for it
        static string? SlimeShoot1(NPC npc, int timer)
        {
            //If NPC is wet, move to wet AI
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(SlimeWet);
            }
            //Disable contact damage
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            //Find a target
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);
            //If timer is past a value, shoot projectile(s)
            if (timer > 85)
            {
                Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(target.Center) * 6f, Main.rand.Next(npc.GetGlobalNPC<CombatNPC>().shootProj!), npc.damage / 2, npc.knockBackResist * 2f, Main.myPlayer);
                proj.friendly = npc.friendly;
                proj.hostile = !npc.friendly;

                //Determine if player is close enough to move to Shoot2
                if (dist < 600)
                {
                    if (dist < 240)
                    {
                        return nameof(SlimeShoot2);
                    }
                    return nameof(SlimeShoot1);
                }
            }
            //If slime falls, move to jump phase
            if (!npc.collideY)
            {
                return nameof(SlimeJump1);
            }
            return null;
        }
        //Comments in SlimeShoot1
        static string? SlimeShoot2(NPC npc, int timer)
        {
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(SlimeWet);
            }
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);
            if (timer > 170)
            {
                for (int i = 0; i < 4; i++)
                {
                    Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, new Vector2(0, -1).RotatedByRandom(1.047f) * Main.rand.NextFloat(3f, 5f), Main.rand.Next(npc.GetGlobalNPC<CombatNPC>().shootProj!), npc.damage / 2, npc.knockBackResist * 2f, Main.myPlayer);
                    proj.friendly = npc.friendly;
                    proj.hostile = !npc.friendly;
                }

                if (dist < 600)
                {
                    if (dist < 240)
                    {
                        return nameof(SlimeShoot2);
                    }
                    return nameof(SlimeShoot1);
                }
            }
            if (!npc.collideY)
            {
                return nameof(SlimeJump1);
            }
            return null;
        }
    }
}
