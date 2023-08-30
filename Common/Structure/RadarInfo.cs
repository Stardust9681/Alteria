using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Alteria.Common.Structure
{
    public struct RadarInfo
    {
        public Vector2 Position;
        public bool IgnoreMisdirect;
        public float AggroFactor;
        public bool IgnoreTiles;
        public bool IgnoreLiquids;
        public Faction Faction;

        public RadarInfo()
        {
            Position = Vector2.Zero;
            IgnoreMisdirect = false;
            AggroFactor = 1f;
            IgnoreTiles = true;
            IgnoreLiquids = true;
            Faction = Faction.None;
        }
        public RadarInfo(Vector2 pos = default, bool igMis = false, float aggro = 1f, bool igTile = true, bool igLiq = true, Faction fac = Faction.None)
        {
            Position = pos;
            IgnoreMisdirect = igMis;
            AggroFactor = aggro;
            IgnoreTiles = igTile;
            IgnoreLiquids = igLiq;
            Faction = fac;
        }
    }
}
