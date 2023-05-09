using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace OtherworldMod.Core.Util
{
    public static class Utils
    {
        public static int QuickProj(Entity src, Vector2 pos, Vector2 vel, int type, int damage, float kb = 0, int owner = 0)
        {
            return Projectile.NewProjectile(src.GetSource_FromThis(), pos, vel, type, damage, kb, owner);
        }
        public static Projectile QuickProjDirect(Entity src, Vector2 pos, Vector2 vel, int type, int damage, float kb = 0, int owner = 0)
        {
            return Projectile.NewProjectileDirect(src.GetSource_FromThis(), pos, vel, type, damage, kb, owner);
        }
        public static T FirstOrDefault<T>(this T[] arr)
        {
            try
            {
                return arr[0];
            }
            catch
            {
                return default(T);
            }
        }
    }
}
