using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CombatPlus.Common;
using CombatPlus.Content;
using CombatPlus.Common.ChangePlayer;

namespace CombatPlus.Common.ChangeItem
{
    public partial class CombatItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public enum StackType : byte
        {
            COMMON_STACK = 0, //999
            EXPLOSIVE = 1, //99
            CONSUMABLE = 2, //30
            NOSTACK = 3, //1
        }
        public StackType stack;
        public override void SetDefaults(Item item)
        {
            if (item.type < ItemID.Count)
            {
                if (item.maxStack == 1)
                    item.GetGlobalItem<CombatItem>().stack = StackType.NOSTACK; //Weapons, Tools, nonstackables
                else if (item.createTile != -1 || !item.consumable && item.material)
                    item.GetGlobalItem<CombatItem>().stack = StackType.COMMON_STACK; //Tiles, Ammo, Most Materials
                else if (item.shoot != 0)
                    item.GetGlobalItem<CombatItem>().stack = StackType.EXPLOSIVE; //Bombs, Grenades, Dynamite, Scarabs, Liquid Bombs, etc.
                else
                    item.GetGlobalItem<CombatItem>().stack = StackType.CONSUMABLE; //Potions
            }
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.DamageType == DamageClass.Summon)
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (player.altFunctionUse == 2 && item.DamageType == DamageClass.Summon)
            {
                int manaCost = player.GetManaCost(item); //Get the current mana cost of the weapon via GetManaCost (thus also calling ModifyManaCost)
                manaCost *= 3;
                return player.statMana > manaCost;
            }
            return base.CanUseItem(item, player);
        }
        public override bool? UseItem(Item item, Player player)
        {
            if (item.potion)
            {
                CombatPlayer modPlayer = player.GetModPlayer<CombatPlayer>();
                modPlayer.heal = item.healLife;
                modPlayer.healTimer = 1;
            }
            if (player.altFunctionUse == 2 && item.DamageType == DamageClass.Summon)
            { }
            return base.UseItem(item, player);
        }
        public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
        {
            if (player.altFunctionUse == 2 && item.DamageType == DamageClass.Summon)
            {
                int minionType = item.shoot; //The projectile (minion) type to be fired
                int count = player.ownedProjectileCounts[minionType]; //Number of that minion that the player currently has active
                mult += count * .3f;
                //All summon staves use 10 mana by default...
                //As the number of summoned minions increases, the cost to summon the next one also increases
                //Additionally, the cost to use the alt ability increases..
                //Issue, that only goes to 33, meaning it could be used 6 times before mana runs out...
            }
            base.ModifyManaCost(item, player, ref reduce, ref mult);
            return;
        }
    }
}
