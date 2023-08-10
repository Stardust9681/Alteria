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
using OtherworldMod.Core.Util;

namespace OtherworldMod.Common.ChangeNPC.AI
{
#nullable enable
    /// <summary>
    /// Sample AI Style methods under this API.
    /// Demonstrating different attack stages or steps, in addition to multiphasic cycles, and control flow
    /// </summary>
    [Autoload(false)]
    public class AIStyle_EX : AIStyleType
    {
        //This method can be used to return a special target type.
            //See [TODO:Brain of Cthulhu] for more details
        protected override ITargetable SetDefaultTarget(int npcIndex)
        {
            return new Core.Util.NPCTarget<AIStyle_EX>(npcIndex);
        }
        //This methods can be used to return a special Radar type.
            //See [TODO:Creepers] for more details
        protected override IRadar SetDefaultRadar(int npcIndex)
        {
            return new Core.Util.NPCRadar(npcIndex);
        }
        //Update the existing target's information with relevant information from here.
        public override void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar)
        {
            info.Position = Main.npc[npcIndex].position;
        }

        protected override int[] ApplicableNPCs => new int[] { NPCID.BlueSlime };

        public override void Load()
        {
            //Load AI into list of behaviours (look I ain't doing this through reflection)
            AddAI(Phase1Move, Phase1Attack, PhaseTransition, Phase2Move, Phase2Attack1, Phase2Attack2);
        }

        //Standard pre-attack movement
        static string? Phase1Move(NPC npc, int timer)
        {
            //Sample NPC movement
            if (timer < 300)
                npc.velocity = new Vector2(0, -.25f);
            else
                npc.velocity = new Vector2(0, 1);

            //Timer-based cycle
            if (timer > 480)
                return nameof(Phase1Attack);
            //If half life is reached
            if (npc.life < npc.lifeMax * .5f)
                return nameof(PhaseTransition);
            //Don't change anything if no conditions are met
                //return nameof(Phase1Move) here would restart the timer
                //So instead we return null
                //This feature is deliberate
            return null;
        }
        //Charge attack movement
        static string? Phase1Attack(NPC npc, int timer)
        {
            //Find a target and position
            npc.target = PullTarget(npc, out TargetInfo info);

            //Slow down before and after attack
            if (timer != 5)
                npc.velocity *= .99f;
            else //if(timer == 5), charge target position, and enable contact damage
            {
                npc.velocity = npc.DirectionTo(info.Position) * 8f;
                npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = true;
            }

            //Disable contact damage and move on, after some duration
            if (timer > 120)
            {
                npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;
                return nameof(Phase1Move);
            }

            return null;
        }
        //Phase transition between 1 and 2
        static string? PhaseTransition(NPC npc, int timer)
        {
            //Slow down
            npc.velocity *= .8f;
            //Wait 1 sec then move to Phase2
            if (timer > 60)
                return nameof(Phase2Move);
            //Don't move on until timer condition passes
            return null;
        }
        //Standard pre-attack movement for Phase2
        static string? Phase2Move(NPC npc, int timer)
        {
            //Find target
            npc.target = PullTarget(npc, out TargetInfo info);

            //Move towards target (even if target is self)
            npc.velocity.X += .14f * (info.Position.X < npc.position.X ? -1 : 1);
            npc.velocity.Y += .21f * (info.Position.Y < npc.position.Y ? -1 : 1);

            //Move to next phase if timer condition met
            if (timer > 360)
            {
                //Control flow; which action should be used next
                if (AppxDistanceTo(npc, info.Position) < 300)
                    return nameof(Phase2Attack1);
                else
                    return nameof(Phase2Attack2);
            }
            return null;
        }
        //Charge attack movement for Phase2
        static string? Phase2Attack1(NPC npc, int timer)
        {
            ///Stronger variant of <see cref="Phase1Attack(NPC, int)"/>

            npc.target = PullTarget(npc, out TargetInfo info);

            if (timer != 10)
                npc.velocity *= .99f;
            else
            {
                npc.velocity = npc.DirectionTo(info.Position) * 12f;
                npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = true;
            }

            if (timer > 120)
            {
                npc.GetGlobalNPC<OtherworldNPC>().allowContactDmg = false;
                return nameof(Phase1Move);
            }

            return null;
        }
        //Projectile behaviour for Phase2
        static string? Phase2Attack2(NPC npc, int timer)
        {
            //Find target
            npc.target = PullTarget(npc, out TargetInfo info);

            //Short-cut if conditions not met
            if (timer < 60)
                return null;

            //Spawn projectile, move on
            Projectile p = npc.SpawnProjDirect(npc.Center, npc.DirectionTo(info.Position), npc.GetGlobalNPC<OtherworldNPC>().shootProj!.First(), npc.damage / 2, 0, Main.myPlayer);
            p.friendly = npc.friendly; p.hostile = !npc.friendly;
            return nameof(Phase2Move);
        }
    }
}
