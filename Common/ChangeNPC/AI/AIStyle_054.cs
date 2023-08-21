using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Alteria.Core.Util.Utils;
using static Alteria.Common.ChangeNPC.Utilities.NPCMethods;
using static Alteria.Common.ChangeNPC.Utilities.AlteriaNPCSets;
using Alteria.Core.Util;

namespace Alteria.Common.ChangeNPC.AI
{
    /// <summary>
    ///<see cref="Terraria.ID.NPCAIStyleID.BrainOfCthulhu"/>
    /// </summary>
    public class AIStyle_054 : AIStyleType
    {
        public override void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar)
        {
            
        }
        protected override int[] ApplicableNPCs => new int[] { NPCID.BrainofCthulhu };
        public override void Load()
        {
            
        }
    }
}
