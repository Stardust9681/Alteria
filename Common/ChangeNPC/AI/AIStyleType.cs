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
    /// Helper class for AI styles to add functions
    /// </summary>
    public abstract class AIStyleType : ILoadable
    {
        protected abstract int[] ApplicableNPCs { get; }
        protected void AddAI(params Func<NPC, int, string?>[] acts)
        {
            foreach (int i in ApplicableNPCs)
            {
                if (i < 0) continue;
                foreach(Func<NPC, int, string?> act in acts)
                    Behaviours[i].Add(act, act.Method.Name);
            }
        }
        public void Load(Mod mod)
        {
            Load();
        }
        public abstract void Load();
        public void Unload()
        {
            foreach (int i in ApplicableNPCs)
            {
                if (i < 0) continue;
                Behaviours[i].Unload();
            }
        }
    }
}
