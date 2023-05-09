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
    public class AIStyle_005
    {
        public static void Load()
        {
            foreach (int i in new int[] { NPCID.BigHornetStingy, NPCID.LittleHornetStingy, NPCID.BigHornetSpikey,
                NPCID.LittleHornetSpikey, NPCID.BigHornetLeafy, NPCID.LittleHornetLeafy, NPCID.BigHornetHoney, NPCID.LittleHornetHoney,
                NPCID.BigHornetFatty, NPCID.LittleHornetFatty, NPCID.BigCrimera, NPCID.LittleCrimera, NPCID.GiantMossHornet,
                NPCID.BigMossHornet, NPCID.LittleMossHornet, NPCID.TinyMossHornet, NPCID.BigStinger, NPCID.LittleStinger, NPCID.BigEater,
                NPCID.LittleEater, NPCID.ServantofCthulhu, NPCID.EaterofSouls, NPCID.MeteorHead, NPCID.Hornet, NPCID.Corruptor,
                NPCID.Probe, NPCID.Crimera, NPCID.MossHornet, NPCID.Moth, NPCID.Bee, NPCID.BeeSmall, NPCID.HornetFatty,
                NPCID.HornetHoney, NPCID.HornetLeafy, NPCID.HornetSpikey, NPCID.HornetStingy, NPCID.Parrot, NPCID.BloodSquid })
            {
                if (i > 0 && i < Behaviours.Length)
                {
                    Behaviours[i].Add(FlierMove1, nameof(FlierMove1));
                    Behaviours[i].Add(FlierMove2, nameof(FlierMove2));
                    Behaviours[i].Add(FlierAttack1, nameof(FlierAttack1));
                }
            }
        }
        public static void Unload()
        {
            foreach (int i in new int[] { NPCID.BigHornetStingy, NPCID.LittleHornetStingy, NPCID.BigHornetSpikey,
                NPCID.LittleHornetSpikey, NPCID.BigHornetLeafy, NPCID.LittleHornetLeafy, NPCID.BigHornetHoney, NPCID.LittleHornetHoney,
                NPCID.BigHornetFatty, NPCID.LittleHornetFatty, NPCID.BigCrimera, NPCID.LittleCrimera, NPCID.GiantMossHornet,
                NPCID.BigMossHornet, NPCID.LittleMossHornet, NPCID.TinyMossHornet, NPCID.BigStinger, NPCID.LittleStinger, NPCID.BigEater,
                NPCID.LittleEater, NPCID.ServantofCthulhu, NPCID.EaterofSouls, NPCID.MeteorHead, NPCID.Hornet, NPCID.Corruptor,
                NPCID.Probe, NPCID.Crimera, NPCID.MossHornet, NPCID.Moth, NPCID.Bee, NPCID.BeeSmall, NPCID.HornetFatty,
                NPCID.HornetHoney, NPCID.HornetLeafy, NPCID.HornetSpikey, NPCID.HornetStingy, NPCID.Parrot, NPCID.BloodSquid })
            {
                if (i > 0 && i < Behaviours.Length)
                {
                    Behaviours[i].Unload();
                }
            }
        }

        //cw circle around player
        static string? FlierMove1(NPC npc, int timer)
        {
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;
            //Find target
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = npc.position.X > target.position.X ? -1 : 1;
            float appxDist = AppxDistanceToTarget(npc, npcTarget);
            npc.rotation = npc.DirectionTo(target.position).ToRotation() - MathHelper.PiOver2;
            Vector2 offset = Vector2.UnitX.RotatedBy(timer * MathHelper.Pi * .004135f);
            Vector2 targetPos = target.Center + (offset * npc.damage * (npc.confused ? -5f : 5f));
            Main.NewText((timer * MathHelper.Pi * .00334f) + " : " + (timer));
            npc.velocity.X += (npc.position.X > targetPos.X) ? -.07f : .07f;
            npc.velocity.Y += (npc.position.Y > targetPos.Y) ? -.07f : .07f;
            if (MathF.Abs(npc.position.X - targetPos.X) < 64)
                npc.velocity.X *= .98f;
            if (MathF.Abs(npc.position.Y - targetPos.Y) < 64)
                npc.velocity.Y *= .98f;
            if (npc.collideX || npc.collideY)
            {
                if (targetDir > 0)
                    return nameof(FlierMove1);
                else
                    return nameof(FlierMove2);
            }

            if (timer > 180 && appxDist < npc.damage * 5.4f)
            {
                npc.velocity *= .5f;
                return nameof(FlierAttack1);
            }
            return null;
        }
        //ccw circle around player
        static string? FlierMove2(NPC npc, int timer)
        {
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;
            //Find target
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            int targetDir = npc.position.X > target.position.X ? -1 : 1;
            float appxDist = AppxDistanceToTarget(npc, npcTarget);
            npc.rotation = npc.DirectionTo(target.position).ToRotation() - MathHelper.PiOver2;
            Vector2 offset = Vector2.UnitX.RotatedBy(-timer * MathHelper.Pi * .004135f);
            Vector2 targetPos = target.Center + (offset * npc.damage * (npc.confused ? -5f : 5f));
            npc.velocity.X += (npc.position.X > targetPos.X) ? -.07f : .07f;
            npc.velocity.Y += (npc.position.Y > targetPos.Y) ? -.07f : .07f;
            if (MathF.Abs(npc.position.X - targetPos.X) < 64)
                npc.velocity.X *= .98f;
            if (MathF.Abs(npc.position.Y - targetPos.Y) < 64)
                npc.velocity.Y *= .98f;
            if (npc.collideX || npc.collideY)
            {
                if (targetDir > 0)
                    return nameof(FlierMove1);
                else
                    return nameof(FlierMove2);
            }
            if (timer > 180 && appxDist < npc.damage * 5.4f)
            {
                npc.velocity *= .5f;
                return nameof(FlierAttack1);
            }
            return null;
        }
        //move quickly towards player
        static string? FlierAttack1(NPC npc, int timer)
        {
            //Find target
            bool npcTarget = FindTarget(npc);
            Entity target = npcTarget ? Main.npc[npc.target] : Main.player[npc.target];
            float lenSQ = npc.velocity.LengthSquared();
            int targetDir = npc.position.X > target.position.X ? -1 : 1;
            npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = lenSQ > 9f;
            npc.velocity += Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * .28f;
            float appxDist = AppxDistanceToTarget(npc, npcTarget);
            if (npc.collideX || npc.collideY || timer > 180 || appxDist > 550)
            {
                if (targetDir > 0)
                    return nameof(FlierMove1);
                else
                    return nameof(FlierMove2);
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
            return null;
        }
    }
}
