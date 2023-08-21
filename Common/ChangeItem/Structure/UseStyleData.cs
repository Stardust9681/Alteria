using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Alteria.Common.ChangeItem.Structure
{
    public struct UseStyleData<T>
    {
        public UseStyleData(T val)
        {
            data = val;
        }

        public T data;

        public static implicit operator UseStyleData<T>(T val)=>new UseStyleData<T>(val);
        public static implicit operator T(UseStyleData<T> a) => a.data;

        public Type CurrentType => data?.GetType() ?? null;
        public Type DefiningType => typeof(T);
        public bool TryGetDatatype<X>(out X val)
        {
            if(data is null || !CurrentType.IsAssignableTo(DefiningType))
            {
                val = default(X);
                return false;
            }

            val = (X)Convert.ChangeType(data, typeof(X));
            return true;
        }
    }
}
