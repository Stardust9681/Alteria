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
    /// Helper class for AI styles to add functions.
    /// Also some other helper functions for use in AI.
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
            Logging.PublicLogger.Debug("AIStyleType.Load(Mod) : " + Main.netMode);
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

        #region I Hate GlobalNPC Name Please Help Me How To Fix What Name To Pick
        //This is a real cry for help I don't know how to name these things.
        public static T GlobalNPC<T>(NPC npc) where T : GlobalNPC => npc.GetGlobalNPC<T>();
        public static OtherworldNPC GetNPC_1(NPC npc) => GlobalNPC<OtherworldNPC>(npc);
        #endregion
    }
}
