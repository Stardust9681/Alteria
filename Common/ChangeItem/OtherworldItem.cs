using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Alteria.Common;
using Alteria.Content;
using Alteria.Common.ChangePlayer;

namespace Alteria.Common.ChangeItem
{
    public partial class OtherworldItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
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
                AlteriaPlayer modPlayer = player.GetModPlayer<AlteriaPlayer>();
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
