using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;

namespace OtherworldMod.Common.ChangeNPC
{
#nullable enable
    //Splitting GlobalX into partial X_Info files, moving to Common.ChangeX
    public partial class OtherworldNPC : GlobalNPC
    {
        //Idea for Slime AI:
        //Keep wiggles as warning for bounce + by-threes jump pattern (lo, med, hi, repeat)
        //Slime can only damage when falling downwards (velocity.Y > 0), 0-knockback damage on ground collision
        //Small afterimage trail when falling downwards + "shockwave-type" particles from hitting ground
        //X-velocity changes slightly in-air depending on player location
        //First jump happens very quickly, second jump happens at med speed, and highest jump takes time to wind up)
        //Jump-time average should come to around current times, to keep movement somewhat consistent

        //To be changed over to new AI system.
        void AI5(NPC npc)
        {
            //Copied AI2(NPC)

            //GlobalNPC
            OtherworldNPC gNPC = npc.GetGlobalNPC<OtherworldNPC>();
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
