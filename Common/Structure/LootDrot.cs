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
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using static Terraria.GameContent.ItemDropRules.Conditions;
using Terraria.Utilities;
using OtherworldMod.Common.ChangeNPC.AI;
using OtherworldMod.Common.ChangePlayer;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using static System.Text.StringBuilder;
using Terraria.ModLoader.IO;

namespace OtherworldMod.Common.Structure
{
    public class LootDrop
    {
        public static LootDrop GenerateOrFindGlobal(LootTable forTable, int itemID, float chance = 1f)
        {
            string name = $"Terraria.Global_{ContentSamples.ItemsByType[itemID].Name}";
            if (forTable.TryGetPool(name, out LootDrop drop))
            {
                return drop;
            }
            return new LootDrop(chance, name);
        }
        public LootDrop WithConditions(params IItemDropRuleCondition[] conds)
        {
            foreach (IItemDropRuleCondition cond in conds)
            {
                Conditions.Add(cond);
            }
            return this;
        }

        public LootDrop(float chance, string internalName) : base()
        {
            Drops = new List<(float Weight, int ItemType, int Min, int Max)>();
            BaseChance = chance;
            attempts = 0;
            Name = internalName;
        }
        public void AddToPool(float weight = 1, int id = 1, int min = 1, int max = 1)
        {
            Drops.Add((weight, id, min, max));
        }
        public bool RemoveFromPool(int index)
        {
            if (index < 0 || index > Drops.Count)
                return false;
            Drops.RemoveAt(index);
            return true;
        }
        public bool ModifyDrop(int index, float? newWeight = null, int? newMin = null, int? newMax = null)
        {
            if (index < 0 || index > Drops.Count)
                return false;
            (float prevWeight, int prevMin, int prevMax) = (Drops[index].Weight, Drops[index].Min, Drops[index].Max);
            Drops[index] = (newWeight ?? prevWeight, Drops[index].ItemType, newMin ?? prevMin, newMax ?? prevMax);
            return true;
        }
        public int IndexOf(Predicate<(float, int, int, int)> predicate)
        {
            for (int i = 0; i < Drops.Count; i++)
            {
                if (predicate.Invoke(Drops[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public string Name
        {
            get;
            private set;
        }
        //Number of attempts since last successful roll
        [DefaultValue(0)]
        private int attempts;
        //The base or root chance for this roll to succeed
        [DefaultValue(1)]
        public float BaseChance
        {
            get;
            internal set;
        }

        //Maximum weight value for all Drops
        public float TotalWeight
        {
            get
            {
                float w = 0;
                foreach ((float W, int, int, int) a in Drops)
                {
                    w += a.W;
                }
                return w;
            }
        }

        public bool DoLoot(DropAttemptInfo info, out Item? item)
        {
            bool Fail(out Item? info)
            {
                info = null;
                return false;
            }

            attempts++; //Increase number of attepmts (attempts always > 0)

            //Check to make sure that all conditions are met for this drop
            foreach (IItemDropRuleCondition condition in Conditions)
            {
                if (!condition.CanDrop(info))
                    return Fail(out item);
            }

            //This section is based on the statistics formula, P'=1-(1-P)^n, where...
            //n = number of attempts
            //P = chance of event occurring (in this case, item dropping)
            //P' = chance that this event would occur in n attempts

            double chance = 1 - BaseChance; //1-P

            //I needed a way to slow the growth, because this number gets very large very quickly without a multiplier here
            chance = 1 - Math.Pow(chance, attempts * BaseChance); //1-()^n

            if (!(chance >= 1 || Main.rand.NextFloat() < chance)) //If the odds are against you
                return Fail(out item);
            attempts = 0; //Reset number of attempts, since we've hit a successful roll
            float weight = Main.rand.NextFloat(TotalWeight);
            float curWeight = 0;
            for (int i = 0; i < Drops.Count; i++)
            {
                if (weight < Drops[i].Weight + curWeight)
                {
                    item = new Item(Drops[i].ItemType, Main.rand.Next(Drops[i].Min, Drops[i].Max+1));
                    return true;
                }
            }
            return Fail(out item);
        }

        public List<IItemDropRuleCondition> Conditions
        {
            get;
        } = new List<IItemDropRuleCondition>();

        public List<(float Weight, int ItemType, int Min, int Max)> Drops
        {
            get;
            private set;
        }
    }

    public class LootTable
    {
        public LootTable()
        {
            table = new List<LootDrop>();
        }
        List<LootDrop> table;
        public int DropsCount => table.Count;

        public void AddDrop(LootDrop drop)
        {
            table.Add(drop);
        }
        public bool RemoveDrop(Predicate<LootDrop> predicate)
        {
            for (int i = 0; i > DropsCount; i++)
            {
                if (predicate.Invoke(table[i]))
                {
                    table.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetPool(string withName, out LootDrop? pool)
        {
            foreach (LootDrop drop in table)
            {
                if (drop.Name.Equals(withName))
                {
                    pool = drop;
                    return true;
                }
            }
            pool = null;
            return false;
        }

        public bool DoLoot(DropAttemptInfo info, out Item[] items)
        {
            bool success = false;
            List<Item> spawnedItems = new List<Item>();
            foreach (LootDrop drop in table)
            {
                if (drop.DoLoot(info, out Item? item))
                {
                    success = true;
                    spawnedItems.Add(item!);
                }
            }
            items = spawnedItems.ToArray();
            return success;
        }
    }

    public static class LootManager
    {
        private static Dictionary<string, LootTable> tables = new Dictionary<string, LootTable>();
        public static bool AddTable(string name, LootTable table)
        {
            bool Fail() => false;
            if (TableExists(name))
                return Fail();
            tables.Add(name, table);
            return true;
        }
        public static bool TryGetTable(string name, out LootTable table)
        {
            bool Fail(out LootTable table)
            {
                table = null;
                return false;
            }
            if (!TableExists(name))
                return Fail(out table);
            table = tables[name];
            return true;
        }
        public static bool TableExists(string name)
        {
            return tables.ContainsKey(name);
        }

        //Region for weak-reference calls
        //I'd really like to make this mod fully compatible with other mods, and easily changed or modified
        #region Weak Refs
        public static bool AddTable_WeakRef(string name)
        {
            return AddTable(name, new LootTable());
        }
        public static bool ModifyTable_AddDrop_WeakRef(string name, float chance, int itemID, int max = 1, int min = 1)
        {
            if (!TryGetTable(name, out LootTable table))
                return false;
            LootDrop drop = new LootDrop(chance, $"Drop_No.{table.DropsCount}");
            drop.AddToPool(1, itemID, min, max);
            table.AddDrop(drop);
            return true;
        }
        public static bool ModifyTable_AddExclusive_WeakRef(string tableName, string dropName, float weight, int itemID, int max = 1, int min = 1)
        {
            if (!TryGetTable(tableName, out LootTable table))
                return false;
            if (!table.TryGetPool(dropName, out LootDrop drop))
                return false;
            drop.AddToPool(weight, itemID, min, max);
            return true;
        }
        public static bool ModifyTable_RemoveExclusive_WeakRef(string tableName, string dropName, int itemID)
        {
            if (!TryGetTable(tableName, out LootTable table))
                return false;
            if (!table.TryGetPool(dropName, out LootDrop drop))
                return false;
            Predicate<(float, int, int, int)> pred = ((float w, int id, int min, int max) param) => {
                if (param.id == itemID)
                    return true;
                return false;
            };
            return drop.RemoveFromPool(drop.IndexOf(pred));
        }
        public static bool ModifyTable_RemoveDrop_WeakRef(string tableName, string dropName)
        {
            if (!TryGetTable(tableName, out LootTable table))
                return false;
            Predicate<LootDrop> pred = (LootDrop drop) => {
                return drop.Name.Equals(dropName);
            };
            return table.RemoveDrop(pred);
        }
        public static bool ModifyTable_ChangeDropRate_WeakRef(string tableName, string dropName, float newChance)
        {
            if (!TryGetTable(tableName, out LootTable table))
                return false;
            if (!table.TryGetPool(dropName, out LootDrop drop))
                return false;
            drop.BaseChance = newChance;
            return true;
        }
        public static bool ModifyTable_ChangeExclusiveMinMax_WeakRef(string tableName, string dropName, int itemID, int max = 1, int min = 1)
        {
            if (!TryGetTable(tableName, out LootTable table))
                return false;
            if (!table.TryGetPool(dropName, out LootDrop drop))
                return false;
            return drop.ModifyDrop(drop.IndexOf(((float, int, int, int) o) => { return o.Item2 == itemID; }), null, min, max);
        }
        #endregion

        #region Initialisation
        private static void InitGlobals()
        {
            LootTable global = new LootTable();

            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.SoulofLight, .33f)
                .WithConditions(new SoulOfLight()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.SoulofNight, .33f)
                .WithConditions(new SoulOfNight()));

            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.HallowedKey, 1 / 2500f)
                .WithConditions(new HallowKeyCondition()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.CorruptionKey, 1 / 2500f)
                .WithConditions(new CorruptKeyCondition()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.CrimsonKey, 1 / 2500f)
                .WithConditions(new CrimsonKeyCondition()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.FrozenKey, 1 / 2500f)
                .WithConditions(new FrozenKeyCondition()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.JungleKey, 1 / 2500f)
                .WithConditions(new JungleKeyCondition()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.DungeonDesertKey, 1 / 2500f)
                .WithConditions(new DesertKeyCondition()));

            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.PirateMap, .01f)
                .WithConditions(new PirateMap()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.MechanicalEye, 1 / 2500f)
                .WithConditions(new ArbitraryCondition((DropAttemptInfo info) => { return !NPC.downedMechBoss1; })));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.MechanicalWorm, 1 / 2500f)
                .WithConditions(new ArbitraryCondition((DropAttemptInfo info) => { return !NPC.downedMechBoss2; })));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.MechanicalSkull, 1 / 2500f)
                .WithConditions(new ArbitraryCondition((DropAttemptInfo info) => { return !NPC.downedMechBoss3; })));

            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.KOCannon, .001f)
                .WithConditions(new IsBloodMoonAndNotFromStatue()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Present, .08f)
                .WithConditions(new IsChristmas()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.GoodieBag, .015f)
                .WithConditions(new HalloweenGoodieBagDrop()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.BloodyMachete, 1 / 2000f)
                .WithConditions(new HalloweenWeapons()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.BladedGlove, 1 / 2000f)
                .WithConditions(new HalloweenWeapons()));

            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Cascade, .0033f)
                .WithConditions(new YoyoCascade()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Amarok, .0033f)
                .WithConditions(new YoyosAmarok()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Yelets, .0033f)
                .WithConditions(new YoyosYelets()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Kraken, .0033f)
                .WithConditions(new YoyosKraken()));
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.HelFire, .0033f)
                .WithConditions(new YoyosHelFire()));

            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Star, .75f)
                .WithConditions(new ArbitraryCondition(x => x.player.statMana < x.player.statManaMax2)));
            if (global.TryGetPool(GetLootFromGlobal(ItemID.Star), out LootDrop star))
            {
                
            }
            global.AddDrop(LootDrop.GenerateOrFindGlobal(global, ItemID.Heart, 1f));
        }
        #endregion
    }
    public class ArbitraryCondition : IItemDropRuleCondition
    {
        public ArbitraryCondition(Func<DropAttemptInfo, bool> pred, string desc = null)
        {
            Description = desc;
            predicate = pred;
        }
        public ArbitraryCondition(Condition root)
        {
            predicate = (DropAttemptInfo info) => { 
                if(info.player.whoAmI == Main.myPlayer)
                    return root.Predicate();
                return false;
            };
            Description = root.Description.Value;
        }
        Func<DropAttemptInfo, bool> predicate;
        private string? Description { get; init; }
        public bool CanDrop(DropAttemptInfo info)
        {
            return predicate.Invoke(info);
        }
        public bool CanShowItemDropInUI() => Description is not null && !Description.Equals(string.Empty);
        public string GetConditionDescription() => Description;
    }
    public class NewItemDropRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules { get; set; }
        public IItemDropRuleCondition? condition;
        LootDrop DropLoot
        {
            get
            {
                if (DropTable?.TryGetPool(lootID, out LootDrop drop) == true)
                {
                    return drop;
                }
                return null;
            }
        }
        LootTable DropTable
        {
            get
            {
                if (LootManager.TryGetTable(tableID, out LootTable table))
                {
                    return table;
                }
                return null;
            }
        }
        string tableID;
        string lootID;
        private int GetDropIndex(DropAttemptInfo info)
        {
            if (DropLoot != null)
            {
                return info.rng.Next(DropLoot.Drops.Count);
            }
            return -1;
        }

        public NewItemDropRule(string table, string drop)
        {
            tableID = table;
            lootID = drop;
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            return condition?.CanDrop(info) ?? true;
        }
        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            if (condition != null)
            {
                DropRateInfoChainFeed ratesInfo2 = ratesInfo.With(1f);
                ratesInfo2.AddCondition(condition);

                (float weight, int id, int min, int max) = DropLoot.Drops[0];
                float dropRate = DropLoot.BaseChance * ratesInfo2.parentDroprateChance;

                drops.Add(new DropRateInfo(id, min, max, dropRate, ratesInfo2.conditions));
                Chains.ReportDroprates(ChainedRules, dropRate, drops, ratesInfo2);
            }
            else
            {
                (float weight, int id, int min, int max) = DropLoot.Drops[0];
                float num = DropLoot.BaseChance;
                float dropRate = num * ratesInfo.parentDroprateChance;
                drops.Add(new DropRateInfo(id, min, max, dropRate, ratesInfo.conditions));
                Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
            }
        }
        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            ItemDropAttemptResult ReturnState(ItemDropAttemptResultState state)
            {
                ItemDropAttemptResult result = default(ItemDropAttemptResult);
                result.State = state;
                return result;
            }

            if (condition != null && !condition!.CanDrop(info))
            {
                return ReturnState(ItemDropAttemptResultState.DoesntFillConditions);
            }

            if(DropLoot.DoLoot(new DropAttemptInfo() { }, out Item? item))
            {
                int index = GetDropIndex(info);
                CommonCode.DropItem(info, item.type, item.stack);
                return ReturnState(ItemDropAttemptResultState.Success);
            }
            return ReturnState(ItemDropAttemptResultState.FailedRandomRoll);
        }
    }
}
