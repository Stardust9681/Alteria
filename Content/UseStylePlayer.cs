using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OtherworldMod.Common.ChangeItem.Structure;

namespace OtherworldMod.Content
{
    public class UseStylePlayer : ModPlayer
    {
        //public object objData = default(object);
        public UseStyleData<object> objData = default(object);
        //public int intData = 0;
        public UseStyleData<int> intData = 0;
        //public float floatData = 0;
        public UseStyleData<float> floatData = 0;
        //public bool boolData = false;
        public UseStyleData<bool> boolData = false;

        public static UseStylePlayer ModPlayer(Player player) => player.GetModPlayer<UseStylePlayer>();
        //UseStylePlayer.ModPlayer(player)
        //player.GetModPlayer<UseStylePlayer>();
        //A whole like, 5 characters shorter :sob:

    }
}
