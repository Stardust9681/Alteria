using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;

namespace OtherworldMod.Common.ChangePlayer
{
    public static class PlayerMethods
    {
        public static float RollLuck(Player player)
        {
            float playerLuck = player.luck;
            return Main.rand.NextFloat() * (1-playerLuck);
        }
    }
}
