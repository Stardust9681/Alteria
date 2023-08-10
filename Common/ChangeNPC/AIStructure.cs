using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using OtherworldMod.Common.Interface;
using OtherworldMod.Common.Structure;

namespace OtherworldMod.Common.ChangeNPC
{
#nullable enable
    //I wonder if I should have designed this as a generic-type class such that...
    //AIStyle<T> where T : Entity
    //Dictionary<string, AIPhase<T>>
    //public void Add(AIPhase<T>, string?=null)
    //public void Add(Func<T, int, string?> a, string?=null)
    //public void Update(T, ref string, ref int)
    //etc

    //Would maybe allow the same system to be used for things like Projectiles and Items, which could be interesting.
    //Items would be a little awkward I think though, might end up using inheritence for that instead.

    public class AIStyle
    {
        Dictionary<string, AIPhase> phases;
        public readonly int ID;
        public int PhaseCount => phases.Count;
        /// <summary>
        /// Whether or not the given AIStyle has a non-default entry
        /// </summary>
        public bool HasEntry
        {
            get;
            private set;
        } = false;
        public AIStyle(int id)
        {
            phases = new Dictionary<string, AIPhase>();
            AIPhase p = new AIPhase();
            p.Add((NPC npc, int timer) => { return phases.Keys.First(x => !string.IsNullOrEmpty(x)); });
            phases.Add("", p);
            ID = id;
        }
        public void Add(AIPhase phase, string? key = null)
        {
            HasEntry = true;
            if (key == null)
                phases.Add((phases.Keys.Count).ToString(), phase);
            else
                phases.Add(key, phase);
        }
        public void Add(Func<NPC, int, string?> a, string key = "")
        {
            HasEntry = true;
            AIPhase p = new AIPhase();
            p.Add(a);
            if (!phases.ContainsKey(key))
            {
                Add(p, key);
            }
            else
            {
                phases[key] = p;
            }
        }
        public void ModifyPhase(string key, Func<NPC, int, string?> func)
        {
            try
            {
                phases[key].Add(func);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logging.PublicLogger.Error($"[OtherworldMod] Tried to modify NPC phase '{key}' out of bounds.", e);
            }
            catch (Exception e)
            {
                Logging.PublicLogger.Error("[OtherworldMod] Unknown problem", e);
            }
        }
        public void Update(NPC npc, ref string? phase, ref int timer)
        {
            if (phase is null)
                phase = "";
            if (phases[phase].Update(npc, timer) is string key and not null)
            {
                timer = 0;
                phase = key;
            }
            timer++;
        }

        //internal delegate void updateTargetInfo(ref TargetInfo info, int npcIndex, IRadar radar);
        internal delegate IRadar setRadar(int npcIndex);
        internal delegate ITargetable setTarget(int npcIndex);

        //internal event updateTargetInfo? On_UpdateTargetInfo;
        //public void UpdateTargetInfo(ref TargetInfo info, int npcIndex, IRadar radar)
        //{
        //    if (On_UpdateTargetInfo is not null)
        //        On_UpdateTargetInfo?.Invoke(ref info, npcIndex, radar);
        //    else
        //    {
        //        NPC npc = Main.npc[npcIndex];
        //        info.Position = npc.position;
        //        info.aggro = AI.AIStyleType.GetNPC_1(npc).aggro;
        //        //info.faction = AI.AIStyleType.GetNPC_1(npc).fac
        //    }
        //}
        internal event setRadar? On_SetRadar;
        public IRadar SetRadar(int npcIndex) => On_SetRadar?.Invoke(npcIndex) ?? new Core.Util.NPCRadar(npcIndex);
        internal event setTarget? On_SetTarget;
        public ITargetable SetTarget(int npcIndex) => On_SetTarget?.Invoke(npcIndex) ?? new Core.Util.NPCTarget(npcIndex);

        /// <summary>
        /// Returns index of the given phase.
        /// </summary>
        /// <remarks>
        /// Used in <see cref="OtherworldNPC"/> for NetSync/Net code
        /// </remarks>
        /// <param name="curPhase"></param>
        /// <returns></returns>
        internal int GetPhaseIndex(string curPhase = "")
        {
            if (!HasEntry)
                return -1;
            int i = 0;
            foreach (string match in phases.Keys)
            {
                if (match.Equals(curPhase))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Returns the phase of a given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal string? PhaseFromIndex(int index)
        {
            if (!HasEntry)
            {
                //Uncomment this line to debug Netsync
                //Logging.PublicLogger.Debug($"RECEIVE: No entries present : {HasEntry} == False");
                return "";
            }
            if (index == -1)
            {
                //Uncomment this line to debug Netsync
                //Logging.PublicLogger.Debug("RECEIVE: IOoB, Index=-1");
                return "";
            }
            int i = 0;
            foreach (string match in phases.Keys)
            {
                if (index == i)
                {
                    Logging.PublicLogger.Debug(i);
                    return match;
                }
                i++;
            }

            //Uncomment this line to debug Netsync
            //Logging.PublicLogger.Debug($"RECEIVE: No match found, max index = {i} : NetMode = {Main.netMode}");
            return "";
        }
        internal void Unload()
        {
            phases.Clear();
        }
    }
    public class AIPhase
    {
        List<Func<NPC, int, string?>> aiActions;
        public AIPhase()
        {
            aiActions = new List<Func<NPC, int, string?>>();
        }

        public void Add(Func<NPC, int, string?> a)
        {
            aiActions.Add(a);
        }
        public string? Update(NPC npc, int timer)
        {
            string? key = null;
            foreach (Func<NPC, int, string?> func in aiActions)
            {
                string? test = func.Invoke(npc, timer);
                if (test?.Equals(key) == false)
                {
                    key = test;
                }
            }
            return key;
        }
    }
#nullable disable
}
