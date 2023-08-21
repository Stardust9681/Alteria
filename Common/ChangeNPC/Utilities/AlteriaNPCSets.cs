using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Alteria.Core.Util.Utils;

namespace Alteria.Common.ChangeNPC.Utilities
{
    public static class AlteriaNPCSets
    {
        //AI Related
        public static AIStyle[] Behaviours = SetBehaviour();
        private static AIStyle[] SetBehaviour()
        {
            AIStyle[] arr = new AIStyle[NPCLoader.NPCCount];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = new AIStyle(i);
            return arr;
        }
    }
}
