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
    /// <summary>
    /// Sample AI Style methods under this API.
    /// Demonstrating different attack stages or steps, in addition to multi-phase cycles.
    /// </summary>
    [Autoload(false)]
    public abstract class AIStyle_EX : AIStyleType
    {
        //TODO:
            //Phase1Move
            //Phase1Attack
            //PhaseTransition
            //Phase2Move
            //Phase2Attack1
            //Phase2Attack2

        //Want to have a clean reference for other developers to look at and emulate.
    }
}
