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
        string? SlimeWet(NPC npc, int timer)
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
        string? SlimeJump1(NPC npc, int timer)
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
        string? SlimeJump2(NPC npc, int timer)
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
        string? SlimeJump3(NPC npc, int timer)
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
        string? SlimeShoot1(NPC npc, int timer)
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
        string? SlimeShoot2(NPC npc, int timer)
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

        /// <summary>
        /// Demon Eye AI - rewritten
        /// </summary>
        /// <param name="npc"></param>
        void AI2(NPC npc)
        {
            //Idea for Demon Eye AI:
            //Do an EoC charge as primary attack, with a proper windup
            //After charge, bounce around a bit until velocity is very slow (lenSQ()<6.5 I think is fair)
            //Each bounce slows NPC a bit
            //Bounce off water unless `npc.ignoreWater` is true or w/e
            //NPC can only damage during charge attack, or when moving a certain speed from standard accelleration

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
                //npc.rotation = MathHelper.Lerp(npc.rotation, (target.position - npc.position).ToRotation(), .12f);
                if (npc.ai[1] < 0)
                {
                    npc.velocity = (target.position - npc.position).SafeNormalize(Vector2.Zero) * MathHelper.Clamp(npc.defense * .48f, 4f, 10f) * conf;
                    npc.ai[0] = MathHelper.Clamp(npc.damage * 12f, 120, 720);
                    npc.ai[1] = npc.ai[0] * .25f;
                    switch (npc.netID)
                    {
                        case NPCID.PigronCorruption:
                        case NPCID.PigronCrimson:
                        case NPCID.PigronHallow:
                            npc.noTileCollide = true;
                            break;
                    }
                }
                else
                {
                    npc.ai[1]--;
                    npc.velocity.Y -= .014f;
                }
            }
            else
            {
                npc.ai[0]--;
                npc.velocity.X += (target.position.X < npc.position.X ? -.096f : .096f) * conf;
                npc.velocity.Y += (target.position.Y < npc.position.Y ? -.024f : .024f) * conf;
            }
            if (dist.X + dist.Y > 600)
                npc.ai[0] -= 5;
            if (dist.X + dist.Y < 160)
            {
                switch (npc.netID)
                {
                    case NPCID.PigronCorruption:
                    case NPCID.PigronCrimson:
                    case NPCID.PigronHallow:
                        npc.noTileCollide = false;
                        break;
                }
            }

            if (!npc.noTileCollide)
            {
                if (npc.collideX)
                    npc.velocity.X *= -1.25f;
                if (npc.collideY)
                    npc.velocity.Y *= -1.25f;
            }
            npc.velocity *= .998f;
            npc.spriteDirection = npc.velocity.X < 0 ? -1 : 1;

            if (npc.wet)
                npc.velocity.Y -= MathHelper.Clamp(npc.velocity.Y - .028f, -6, 6);

            npc.velocity.Y *= .9f;
            npc.velocity *= .8f;

            npc.rotation = MathHelper.Lerp(npc.rotation, npc.velocity.ToRotation(), .15f);
        }

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
}
