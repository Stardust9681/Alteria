using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static CombatPlus.Core.Util.Utils;

namespace CombatPlus.Common.ChangeNPC
{
#nullable enable
    public class AIStyle
    {
        Dictionary<string, AIPhase> phases;
        public readonly int ID;
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
            if (key == null)
                phases.Add((phases.Keys.Count).ToString(), phase);
            else
                phases.Add(key, phase);
        }
        public void Add(Func<NPC, int, string?> a, string? key = null)
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
                Logging.PublicLogger.Error($"[CombatPlus] Tried to modify NPC phase '{key}' out of bounds.", e);
            }
            catch (Exception e)
            {
                Logging.PublicLogger.Error("[CombatPlus] Unknown problem", e);
            }
        }
        public void Update(NPC npc, ref string phase, ref int timer)
        {
            if (phases[phase].Update(npc, timer) is string key and not null)
            {
                timer = 0;
                phase = key;
            }
            timer++;
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
