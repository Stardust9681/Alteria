using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OtherworldMod.Common.ChangePlayer;
using OtherworldMod.Common.ChangeItem;
using OtherworldMod.Content;

namespace OtherworldMod.Common.ChangeItem
{
    public partial class OtherworldItem : GlobalItem
    {
        public static class CustomUseStyle
        {
            public const int Swipe = 15;
            public const int SummonStaff = 16;
            public const int ChargeBow = 17;

        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            if (item.useStyle < CustomUseStyle.Swipe)
            {
                base.UseStyle(item, player, heldItemFrame);
                return;
            }

            UseStylePlayer modPlayer = player.GetModPlayer<UseStylePlayer>();
            if (item.useStyle == CustomUseStyle.Swipe)
            {

                return;
            }
            if (item.useStyle == CustomUseStyle.SummonStaff)
            {

                return;
            }
            if (item.useStyle == CustomUseStyle.ChargeBow)
            {

                return;
            }
            }
        }
    }
