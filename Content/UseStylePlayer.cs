using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CombatPlus.Content
{
    public class UseStylePlayer : ModPlayer
    {
        public object objData = default(object);
        public int intData = 0;
        public float floatData = 0;
        public bool boolData = false;

        public static UseStylePlayer ModPlayer(Player player) => player.GetModPlayer<UseStylePlayer>();
        //UseStylePlayer.ModPlayer(player)
        //player.GetModPlayer<UseStylePlayer>();
        //A whole like, 5 characters shorter :sob:

    }
}
