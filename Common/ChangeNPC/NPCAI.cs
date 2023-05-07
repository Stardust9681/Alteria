using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static CombatPlus.Core.Util.Utils;

namespace CombatPlus.Common.ChangeNPC
{
#nullable enable
    //Splitting GlobalX into partial X_Info files, moving to Common.ChangeX
    public partial class CombatNPC : GlobalNPC
    {
        //Idea for Slime AI:
        //Keep wiggles as warning for bounce + by-threes jump pattern (lo, med, hi, repeat)
        //Slime can only damage when falling downwards (velocity.Y > 0), 0-knockback damage on ground collision
        //Small afterimage trail when falling downwards + "shockwave-type" particles from hitting ground
        //X-velocity changes slightly in-air depending on player location
        //First jump happens very quickly, second jump happens at med speed, and highest jump takes time to wind up)
        //Jump-time average should come to around current times, to keep movement somewhat consistent

        #region Slime AI
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
                    if(npc.velocity.X == 0)
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
                    Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, new Vector2(0, -1).RotatedByRandom(1.047f)*Main.rand.NextFloat(3f, 5f), Main.rand.Next(npc.GetGlobalNPC<CombatNPC>().shootProj!), npc.damage / 2, npc.knockBackResist * 2f, Main.myPlayer);
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
        #endregion

        #region Demon Eye AI
        //Executed at daytime
        static string? EyeDaytime(NPC npc, int timer)
        {
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            //Find target
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            //Find target direction
            Vector2 targetDir = npc.DirectionTo(target.position);
            //Change velocity to move away from target (account for confusion)
            npc.velocity -= targetDir * (npc.confused ? -.14f : .14f);
            //Never move on from this AI (I'm not all too concerned about it having weird behaviour when it cycles to nighttime again)
            return null;
        }
        static string? EyeWet(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EyeDaytime);
            }
            //If NPC is no longer wet change to different phase
            if (!(npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet))
            {
                return nameof(EyeAttack1);
            }
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            npc.velocity.Y -= .048f;
            npc.velocity.X *= .98f;
            return null;
        }
        static string? EyeAttack1(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EyeDaytime);
            }
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(EyeWet);
            }
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float dist = AppxDistanceToTarget(npc, npcTarget);
            //If target is far enough, move to charge attack
            if (dist > 800)
            {
                return nameof(EyeAttack2);
            }
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            npc.velocity.X += targetDir * .07f;
            int moveDir = npc.velocity.X < 0 ? -1 : 1;
            if (targetDir == moveDir)
            {
                npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, ((npc.Center.Y - target.Center.Y)*(npc.confused?-1:1)) * .008f, .05f);
            }
            else
            {
                npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, ((target.Center.Y - npc.Center.Y) * (npc.confused ? -1 : 1)) * .012f, .05f);
            }
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = targetDir == moveDir && (MathF.Abs(npc.velocity.X) + MathF.Abs(npc.velocity.Y)) > 6.5f;
            if (npc.collideX)
            {
                npc.velocity.X = -npc.oldVelocity.X;
            }
            if (npc.collideY)
            {
                if (MathF.Abs(npc.oldVelocity.Y) > 4)
                    npc.velocity.Y = -npc.oldVelocity.Y;
                else
                {
                    npc.velocity.Y = npc.oldVelocity.Y < 0 ? -4.5f : 4.5f;
                }
            }
            return null;
        }
        static string? EyeAttack2(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EyeDaytime);
            }
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(EyeWet);
            }
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            //Disable contact damage on ground
            gNPC.allowContactDmg = false;
            //Check if NPC can shoot projectiles
            bool canShoot = gNPC.shootProj != null && gNPC.shootProj.Length > 0 && gNPC.shootProj[0] != 0;
            npc.velocity += npc.DirectionTo(target.position) * .5f;
            npc.velocity *= .9f;
            if (!canShoot || timer > 90)
            {
                return nameof(EyeAttack3);
            }
            else if (timer != 0 && timer % 30 == 0)
            {
                Vector2 vel = npc.DirectionTo(target.position).RotatedBy(MathHelper.ToRadians(((timer / 30) - 2) * 10)) * (npc.confused ? -5.4f : 5.4f);
                Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, vel, Main.rand.Next(gNPC.shootProj), npc.damage / 2, 0f, Main.myPlayer);
                proj.friendly = npc.friendly;
                proj.hostile = !npc.friendly;
                npc.position -= vel;
            }
            return null;
        }
        static string? EyeAttack3(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EyeDaytime);
            }
            if (npc.wet || npc.honeyWet || npc.lavaWet || npc.shimmerWet)
            {
                return nameof(EyeWet);
            }
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            if (npc.collideX)
            {
                npc.velocity.X = -npc.oldVelocity.X;
            }
            if (npc.collideY)
            {
                if (MathF.Abs(npc.oldVelocity.Y) > 4)
                    npc.velocity.Y = -npc.oldVelocity.Y;
                else
                {
                    npc.velocity.Y = npc.oldVelocity.Y < 0 ? -4.5f : 4.5f;
                }
            }
            if (timer > 180)
            {
                if (timer == 181)
                {
                    npc.velocity *= 4f;
                }
                npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
                if (npc.velocity.LengthSquared() < 49f)
                {
                    npc.velocity += npc.DirectionTo(target.position).RotatedByRandom(.262f) * (npc.confused ? -.35f : .35f);
                }
                else
                {
                    npc.velocity *= .986f;
                }
                float dist = AppxDistanceToTarget(npc, npcTarget);
                if (dist < timer * 3f && timer > 270)
                {
                    return nameof(EyeAttack1);
                }
                return null;
            }
            else
            {
                npc.velocity += npc.DirectionTo(target.position) * (npc.confused ? -1 : 1);
                npc.velocity *= .15f;
                npc.rotation = npc.velocity.ToRotation();
            }
            return null;
        }
        #endregion
        #region Flying AI

        #endregion

        #region Bat AI
        static string? BatMove1(NPC npc, int timer)
        {
            if (!npc.noGravity)
                npc.noGravity = true;
            //Find a target, true if NPC found
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            //Direction to target (account for confusion)
            int targetDir = target.position.X < npc.position.X ? -1 : 1 * (npc.confused ? -1 : 1);
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = false;
            float targetX = target.position.X - (targetDir * 96f);
            if (npc.position.X > targetX)
            {
                npc.velocity.X -= .08f;
            }
            else
            {
                npc.velocity.X += .08f;
            }
            if (MathF.Abs(npc.position.X - targetX) < 64)
                npc.velocity.X *= .95f;
            float targetY = target.position.Y - 320;
            if (npc.position.Y > targetY)
            {
                npc.velocity.Y -= .06f;
            }
            else
            {
                npc.velocity.Y += .06f;
            }
            if (MathF.Abs(npc.position.Y - targetY) < 64)
                npc.velocity.Y *= .98f;
            if (AppxDistanceToTarget(npc, npcTarget) < 400)
            {
                npc.velocity *= .987f;
            }
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
            if (npc.velocity.Y < 1)
            {
                npc.velocity.Y = 7.4f;
            }
            npc.GetGlobalNPC<CombatNPC>().allowContactDmg = true;
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(target.position) * 4f, .018f);
            if (AppxDistanceToTarget(npc, npcTarget) > 600)
            {
                npc.velocity *= .987f;
            }
            if (((npc.velocity.X < 0 ? -1 : 1) != targetDir) || npc.position.Y > target.position.Y)
                return nameof(BatAttack2);
            return null;
        }
        static string? BatAttack2(NPC npc, int timer)
        {
            //Disable contact damage
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            if (gNPC.shootProj == null || gNPC.shootProj.Length == 0 || gNPC.shootProj[0] == 0)
                return nameof(BatMove1);
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
        #endregion

        #region EoC AI
        //Move above player
        static string? EOCMove1(NPC npc, int timer)
        {
            if (Main.dayTime)
            {
                return nameof(EOCMove3);
            }
            Vector2 npcCenter = npc.Center;
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            Vector2 targetCenter = target.Center;
            int targetDir = targetCenter.X < npcCenter.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = false;
            npc.rotation = (targetCenter - npcCenter).ToRotation()-MathHelper.PiOver2;
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
            if (npc.life < npc.lifeMax * .625f)
            {
                return nameof(EOCMove1);
            }
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
            if (npc.life < npc.lifeMax * .625f)
            {
                return nameof(EOCMove1);
            }
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
            if (npc.life < npc.lifeMax * .625f)
            {
                return nameof(EOCMove1);
            }
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            gNPC.allowContactDmg = timer > 90;
            if (timer == 90)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation+MathHelper.PiOver2) * 16f;
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
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = target.Center.X < npc.Center.X ? -1 : 1 * (npc.confused ? -1 : 1);
            float healthFactor = ((float)npc.life / (float)npc.lifeMax);
            if (timer < 5)
            {
                Vector2 targetPos = target.Center + new Vector2(Main.rand.NextFloat(-128f, 128f)*(1-(2*healthFactor)), Main.rand.NextFloat(-256f, 256f)*(1-(2*healthFactor)));
                npc.rotation = Utils.AngleLerp(npc.rotation, (targetPos - npc.Center).ToRotation() - MathHelper.PiOver2, MathHelper.Clamp((1-(healthFactor))*.8f, 0, 1));
            }
            int pauseDur = (int)MathHelper.Lerp(3, 40, healthFactor);
            gNPC.allowContactDmg = timer > pauseDur;
            float velMult = MathHelper.Lerp(22, 36, 1 - (2 * healthFactor));
            if (timer == pauseDur)
            {
                npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * velMult;
            }
            else
            {
                if (timer > pauseDur && npc.velocity.LengthSquared() < velMult*2f)
                {
                    npc.velocity *= .5f;
                    if (Main.expertMode || Main.masterMode)
                    {
                        if (Main.rand.NextFloat() > 1.33f*healthFactor)
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
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
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
        #endregion

        /// <summary>
        /// Fighter AI - rewritten
        /// </summary>
        /// <param name="npc"></param>
        void AI3(NPC npc)
        {
            
        }

        /// <summary>
        /// Eye of Cthulhu AI - rewritten
        /// </summary>
        /// <param name="npc"></param>
        void AI4(NPC npc)
        {

        }

        /// <summary>
        /// Flying/Flight, hornets/harpies/bats, AI - rewritten
        /// </summary>
        /// <param name="npc"></param>
        void AI5(NPC npc)
        {
            //Copied AI2(NPC)

            //GlobalNPC
            CombatNPC gNPC = npc.GetGlobalNPC<CombatNPC>();
            //Target entity
            Entity target = npc;
            //Allow friendly and hostile targetting
            if (!npc.friendly)
            {
                npc.TargetClosest(false);
                target = Main.player[npc.target];
            }
            else
            {
                float dSq = float.MaxValue;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    float otherDSq = npc.DistanceSQ(other.position);
                    if (other.active && !other.friendly && !other.immortal && otherDSq < dSq)
                    {
                        dSq = otherDSq;
                        npc.target = i;
                        target = other;
                    }
                }
            }
            //+/- direction of the target entity
            int direction = npc.position.X > target.position.X ? -1 : 1;
            int conf = npc.confused ? -1 : 1;
            direction *= conf;
            //X,Y distance from target
            Vector2 dist = new Vector2(MathF.Abs(npc.position.X - target.position.X), MathF.Abs(npc.position.Y - target.position.Y));

            npc.velocity *= 1.25f;
            npc.velocity.Y *= 1.1111f;

            if (npc.ai[0] < 0)
            {
                npc.velocity *= .942f;
                npc.spriteDirection = direction;
                if (npc.ai[1] < 0)
                {
                    npc.velocity = (target.position - npc.position).SafeNormalize(Vector2.Zero) * MathHelper.Clamp(npc.defense * .48f, 4f, 10f) * conf;
                    npc.ai[0] = MathHelper.Clamp(npc.damage * 12f, 120, 720);
                    npc.ai[1] = npc.ai[0] * .25f;
                }
                else
                {
                    npc.ai[1]--;
                }
            }
            else
            {
                npc.ai[0]--;
                npc.velocity.X += target.position.X < npc.position.X ? -.096f : .096f * conf;
                npc.velocity.Y += target.position.Y < npc.position.Y ? -.024f : .024f * conf;
                npc.spriteDirection = npc.velocity.X < 0 ? -1 : 1;
                npc.rotation = npc.velocity.X * .0048f;
            }
            if (dist.X + dist.Y > 600)
                npc.ai[0] -= 5;

            if (!npc.noTileCollide)
            {
                if (npc.collideX)
                    npc.velocity.X *= -1.25f;
                if (npc.collideY)
                    npc.velocity.Y *= -1.25f;
            }

            npc.velocity.Y -= .014f;

            npc.velocity *= .998f;
            npc.spriteDirection = npc.velocity.X < 0 ? -1 : 1;

            if (npc.wet)
                npc.velocity.Y -= MathHelper.Clamp(npc.velocity.Y - .028f, -6, 6);

            npc.velocity.Y *= .9f;
            npc.velocity *= .8f;

            //AI5(NPC) changes:
            if (gNPC.shootProj != null && gNPC.shootProj.Length > 0 && gNPC.shootProj[0] != 0)
            {
                if (npc.localAI[0] < 0)
                {
                    Projectile p;
                    switch (npc.netID)
                    {
                        case NPCID.Hornet:
                        case NPCID.HornetFatty:
                        case NPCID.HornetHoney:
                        case NPCID.HornetLeafy:
                        case NPCID.HornetSpikey:
                        case NPCID.HornetStingy:
                        case NPCID.BigHornetFatty:
                        case NPCID.BigHornetHoney:
                        case NPCID.BigHornetLeafy:
                        case NPCID.BigHornetSpikey:
                        case NPCID.BigHornetStingy:
                        case NPCID.BigMossHornet:
                        case NPCID.GiantMossHornet:
                        case NPCID.LittleHornetFatty:
                        case NPCID.LittleHornetHoney:
                        case NPCID.LittleHornetLeafy:
                        case NPCID.LittleHornetSpikey:
                        case NPCID.LittleHornetStingy:
                        case NPCID.LittleMossHornet:
                        case NPCID.MossHornet:
                        case NPCID.TinyMossHornet:
                            p = QuickProjDirect(npc, npc.Center, (target.position - npc.position).SafeNormalize(Vector2.Zero) * (npc.life / (float)npc.lifeMax) * (npc.damage * .1f), gNPC.shootProj[0], npc.damage, 1f, Main.myPlayer);
                            if (p != null) { p.hostile = !npc.friendly; p.friendly = npc.friendly; }
                            break;
                        case NPCID.Harpy:
                            p = QuickProjDirect(npc, npc.Center, (target.position - npc.position).SafeNormalize(Vector2.Zero) * 6.4f, gNPC.shootProj[0], npc.damage, 1f, Main.myPlayer);
                            if (p != null) { p.hostile = !npc.friendly; p.friendly = npc.friendly; }
                            break;
                        default:
                            p = QuickProjDirect(npc, npc.Center, (target.position - npc.position).SafeNormalize(Vector2.Zero) * 5.4f, gNPC.shootProj[0], npc.damage, 1f, Main.myPlayer);
                            if (p != null) { p.hostile = !npc.friendly; p.friendly = npc.friendly; }
                            break;
                    }
                    npc.localAI[0] = MathHelper.Clamp(npc.damage * 9f, 90f, 280f);
                }
                else
                {
                    npc.localAI[0]--;
                }
            }
        }
    }
#nullable disable
}
