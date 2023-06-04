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
#nullable enable
    /// <summary>
    /// <see cref="NPCAIStyleID.Passive"/>
    /// </summary>
    [Autoload(false)]
    public abstract class AIStyle_007 : AIStyleType
    {
        protected override int[] ApplicableNPCs => new int[] { NPCID.Merchant, NPCID.Nurse, NPCID.ArmsDealer, NPCID.Dryad, NPCID.Guide, NPCID.OldMan,
                NPCID.Demolitionist, NPCID.Bunny, NPCID.Clothier, NPCID.GoblinTinkerer, NPCID.Wizard, NPCID.Mechanic, NPCID.SantaClaus,
                NPCID.Penguin, NPCID.PenguinBlack, NPCID.Truffle, NPCID.Steampunker, NPCID.DyeTrader, NPCID.PartyGirl, NPCID.Cyborg,
                NPCID.Painter, NPCID.WitchDoctor, NPCID.Pirate, NPCID.GoldfishWalker, NPCID.Squirrel, NPCID.Mouse, NPCID.BunnySlimed,
                NPCID.BunnyXmas, NPCID.Stylist, NPCID.Frog, NPCID.Duck, NPCID.DuckWhite, NPCID.ScorpionBlack, NPCID.Scorpion,
                NPCID.TravellingMerchant, NPCID.Angler, NPCID.TaxCollector, NPCID.GoldBunny, NPCID.GoldFrog, NPCID.GoldMouse,
                NPCID.SkeletonMerchant, NPCID.SquirrelRed, NPCID.SquirrelGold, NPCID.PartyBunny, NPCID.DD2Bartender, NPCID.Golfer,
                NPCID.GoldGoldfishWalker, NPCID.Seagull, NPCID.Grebe, NPCID.Rat, NPCID.ExplosiveBunny, NPCID.Turtle, NPCID.TurtleJungle,
               NPCID.SeaTurtle, NPCID.BestiaryGirl, NPCID.TownCat, NPCID.TownDog, NPCID.GemSquirrelAmethyst, NPCID.GemSquirrelTopaz,
                NPCID.GemSquirrelSapphire, NPCID.GemSquirrelEmerald, NPCID.GemSquirrelRuby, NPCID.GemSquirrelDiamond,
                NPCID.GemSquirrelAmber, NPCID.GemBunnyAmethyst, NPCID.GemBunnyTopaz, NPCID.GemBunnySapphire, NPCID.GemBunnyEmerald,
                NPCID.GemBunnyRuby, NPCID.GemBunnyDiamond, NPCID.GemBunnyAmber, NPCID.TownBunny, NPCID.Princess };
        public override void Load()
        {
            AddAI(Walk, Jump, NoMove, FindHome, Swim, Fly, PerformAttack);
        }
        //Did you know this one is a pain to do? Emulating item use on NPCs is a pain in the ass, especially since *drawing* needs to be considered x.x
        static string? PerformAttack(NPC npc, int timer)
        {
            int type = NPCID.Sets.AttackType[npc.type];
            if (type == -1)
                return nameof(NoMove);
            int time = NPCID.Sets.AttackTime[npc.type];
            int frames = NPCID.Sets.AttackFrameCount[npc.type];
            if (timer > time)
                return nameof(NoMove);

            npc.velocity.X = 0;

            bool foundTarget = FindTarget(npc, out Vector2 targetPos);
            if (!foundTarget)
                return nameof(NoMove);

            int damage = npc.damage;
            float kb = npc.knockBackResist;
            NPCLoader.TownNPCAttackStrength(npc, ref damage, ref kb);
            int projType = npc.GetGlobalNPC<OtherworldNPC>().shootProj?.FirstOrDefault() ?? 0;
            int delay = time;
            float projSpeed = 0;
            float grav = 0;
            float offset = 0;
            switch (type)
            {
                case 0: //Thrown
                    NPCLoader.TownNPCAttackProj(npc, ref projType, ref delay);
                    if (timer == delay)
                    {
                        NPCLoader.TownNPCAttackProjSpeed(npc, ref projSpeed, ref grav, ref offset);
                        Projectile p = npc.SpawnProjDirect(npc.Center + new Vector2(Main.rand.NextFloat(offset), Main.rand.NextFloat(offset)), npc.DirectionTo(targetPos) * projSpeed, projType, damage, kb);
                        p.friendly = npc.friendly; p.hostile = !npc.friendly;
                        return nameof(Walk);
                    }
                    break;
                case 1: //Shoot
                    NPCLoader.TownNPCAttackProj(npc, ref projType, ref delay);
                    if (timer == delay)
                    {
                        NPCLoader.TownNPCAttackProjSpeed(npc, ref projSpeed, ref grav, ref offset);
                        Projectile p = npc.SpawnProjDirect(npc.Center + new Vector2(Main.rand.NextFloat(offset), Main.rand.NextFloat(offset)), npc.DirectionTo(targetPos) * projSpeed, projType, damage, kb);
                        p.friendly = npc.friendly; p.hostile = !npc.friendly;
                        return nameof(Walk);
                    }
                    break;
                case 2: //Magic
                    NPCLoader.TownNPCAttackProj(npc, ref projType, ref delay);
                    if (timer == delay)
                    {
                        NPCLoader.TownNPCAttackProjSpeed(npc, ref projSpeed, ref grav, ref offset);
                        Projectile p = npc.SpawnProjDirect(npc.Center + new Vector2(Main.rand.NextFloat(offset), Main.rand.NextFloat(offset)), npc.DirectionTo(targetPos) * projSpeed, projType, damage, kb);
                        p.friendly = npc.friendly; p.hostile = !npc.friendly;
                        return nameof(Walk);
                    }
                    else if (timer < delay)
                    {
                        float intensity = 1f;
                        NPCLoader.TownNPCAttackMagic(npc, ref intensity);
                    }
                    break;
                case 3: //Swing
                    int iWidth = 0;
                    int iHeight = 0;
                    NPCLoader.TownNPCAttackSwing(npc, ref iWidth, ref iHeight);
                    break;
                default:
                    break;
            }
            //NPCID.Sets.ActsLikeTownNPC[npc.type]
            return null;
        }
        static string? Walk(NPC npc, int timer)
        {
            int direction;
            if (MathF.Abs(npc.position.X - npc.homeTileX) < 192)
            {
                direction = npc.direction;
            }
            else
            {
                direction = npc.homeTileX < npc.position.X ? -1 : 1;
            }
            if (npc.collideX) //Check for doors ig
                return nameof(Jump);
            if (Main.rand.NextBool(360))
                return nameof(NoMove);
            npc.velocity.X = direction;
            return null;
        }
        static string? NoMove(NPC npc, int timer)
        {
            if(npc.collideY)
                npc.velocity.X = 0;
            //Add check for if NPC can sit
            //Add check for nearby townies
            if (Main.rand.NextBool(3600))
                return nameof(Walk);
            return null;
        }
        static string? Jump(NPC npc, int timer)
        {
            npc.velocity.Y = -6f;
            npc.position.Y += npc.velocity.Y;
            npc.velocity.X = npc.oldVelocity.X;
            return nameof(Walk);
        }
        static string? FindHome(NPC npc, int timer)
        {
            int direction = npc.homeTileX < npc.position.X ? -1 : 1;
            npc.velocity.X = 2 * direction;
            if (npc.collideX)
            {
                npc.velocity.Y = -6f;
                npc.position.Y += npc.velocity.Y;
                npc.velocity.X = npc.oldVelocity.X;
            }
            if (MathF.Abs(npc.homeTileX - npc.position.X) < 8)
                return nameof(NoMove);
            return null;
        }
        static string? Swim(NPC npc, int timer)
        {
            return null;
        }
        static string? Fly(NPC npc, int timer)
        {
            return null;
        }
    }
}
