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
using Alteria.Common.Structure;
using Alteria.Common.Interface;

namespace Alteria.Common.ChangeNPC.AI
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
            Load();
            foreach (int i in ApplicableNPCs)
            {
                if (i < 0) continue;
                Behaviours[i].On_SetRadar += SetDefaultRadar;
                Behaviours[i].On_SetTarget += SetDefaultTarget;
            }
        }
        public abstract void Load();
        public void Unload()
        {
            foreach (int i in ApplicableNPCs)
            {
                if (i < 0) continue;
                Behaviours[i].On_SetRadar -= SetDefaultRadar;
                Behaviours[i].On_SetTarget -= SetDefaultTarget;
                Behaviours[i].Unload();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="npcIndex"></param>
        /// <param name="radar"></param>
        public abstract void UpdateInfo(ref TargetInfo info, int npcIndex, IRadar radar);

        protected virtual IRadar SetDefaultRadar(int npcIndex) => new Core.Util.NPCRadar(npcIndex);
        protected virtual ITargetable SetDefaultTarget(int npcIndex) => new Core.Util.NPCTarget(npcIndex);

        protected static ITargetable PullTargetDirect(NPC npc, out TargetInfo info)
        {
            return Core.Util.TargetCollective.PullTargetDirect(GetNPC_1(npc).NPCRadar, out info);
        }
        protected static int PullTarget(NPC npc, out TargetInfo info)
        {
            return Core.Util.TargetCollective.PullTarget(GetNPC_1(npc).NPCRadar, out info);
        }

        #region I Hate GlobalNPC Name Please Help Me How To Fix What Name To Pick
        //This is a real cry for help I don't know how to name these things.
        public static T GlobalNPC<T>(NPC npc) where T : GlobalNPC => npc.GetGlobalNPC<T>();
        public static AlteriaNPC GetNPC_1(NPC npc) => GlobalNPC<AlteriaNPC>(npc);
        #endregion
    }
}
