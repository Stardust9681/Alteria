﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Alteria.Core.Util
{
    public static class Utils
    {
        public static float SafeInvert(this float num)
        {
            if (num == 0)
                return float.MaxValue;
            else return 1f / num;
        }
        public static int QuickProj(Entity src, Vector2 pos, Vector2 vel, int type, int damage, float kb = 0, int owner = 0)
        {
            return Projectile.NewProjectile(src.GetSource_FromThis(), pos, vel, type, damage, kb, owner);
        }
        public static Projectile QuickProjDirect(Entity src, Vector2 pos, Vector2 vel, int type, int damage, float kb = 0, int owner = 0)
        {
            return Projectile.NewProjectileDirect(src.GetSource_FromThis(), pos, vel, type, damage, kb, owner);
        }
        public static int SpawnProj(this Entity src, Vector2 pos, Vector2 vel, int type, int damage, float kb = 0, int owner = 0)
        {
            return Projectile.NewProjectile(src.GetSource_FromThis(), pos, vel, type, damage, kb, owner);
        }
        public static Projectile SpawnProjDirect(this Entity src, Vector2 pos, Vector2 vel, int type, int damage, float kb = 0, int owner = 0)
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
        public static float AppxDistance(Vector2 a, Vector2 other)
        {
            return Math.Abs(a.X - other.X) + Math.Abs(a.Y - other.Y);
        }
    }
}
