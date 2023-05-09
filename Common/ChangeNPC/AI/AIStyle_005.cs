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
    public class AIStyle_005
    {
        public void Load()
        {
        }
        public void Unload()
        {
        }

        //cw circle around player
        string? FlierMove1(NPC npc, int timer)
        {
            return null;
        }
        //ccw circle around player
        string? FlierMove2(NPC npc, int timer)
        {
            return null;
        }
        //move quickly towards player
        string? FlierAttack1(NPC npc, int timer)
        {
            return null;
        }
        //fire projectile(s) if applicable
        string? FlierAttack2(NPC npc, int timer)
        {
            return null;
        }
    }
}
