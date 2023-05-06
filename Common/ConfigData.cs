using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CombatPlus.Common
{
    //This can maybe be done with IComparer/IComparable, or a Predicate of sorts to determine what fits under the given constraints
    public struct ConfigData<T>
    {
        public ConfigRequirement<T> Req { get; private set; }
        public T value;
        public ConfigData(T val)
        {
            value = val;
            Req = new ConfigRequirement<T>();
        }
        public bool CheckRule(T val)
        {
            if (!Req.hasRequirement)
                return true;
            return Req.StuckValue.Equals(val);
        }
        public bool AddRule(Mod mod, T val)
        {
            if (CheckRule(val))
            {
                Req = new ConfigRequirement<T>(val, mod);
                value = val;
                return true;
            }
            return false;
        }
        public bool AddRule(string modName, T val)
        {
            if (CheckRule(val))
            {
                Req = new ConfigRequirement<T>(val, modName);
                value = val;
                return true;
            }
            return false;
        }
        public ConfigData<X> ForceType<X>()
        {
            AsType<X>(out ConfigData<X> cData);
            return cData;
        }
        public bool AsType<X>(out ConfigData<X> cData)
        {
            try
            {
                object? obj = Convert.ChangeType(value, typeof(X));
                if (obj != null)
                {
                    cData = new ConfigData<X>((X)obj);
                    if (this.Req.hasRequirement)
                    {
                        bool ret = this.Req.AsType(out ConfigRequirement<X> cReq);
                        cData.Req = cReq;
                        if (!ret)
                        {
                            Logging.PublicLogger.Error("Tried to convert CombatPlus config data to incompatible type");
                            return false;
                        }
                    }
                    else
                    {
                        cData.Req = default(ConfigRequirement<X>);
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                cData = default(ConfigData<X>);
                Logging.PublicLogger.Error("Tried to convert CombatPlus config data to incompatible type", e);
                return false;
            }
            cData = default(ConfigData<X>);
            Logging.PublicLogger.Error("Tried to convert CombatPlus config data to incompatible type");
            return false;
        }

        public static implicit operator T(ConfigData<T> cData) => cData.value;
    }
    public struct ConfigRequirement<T>
    {
        public readonly bool hasRequirement;
#nullable enable
        public T StuckValue { get; init; }
#nullable disable
        public string ModName { get; init; }

        public ConfigRequirement()
        {
            StuckValue = default(T);
            ModName = "Terraria";
            hasRequirement = false;
        }
        public ConfigRequirement(T val, string modName = "Terraria")
        {
            ModName = modName;
            StuckValue = val;
            hasRequirement = true;
        }
        public ConfigRequirement(T val, Mod mod)
        {
            ModName = mod.Name;
            StuckValue = val;
            hasRequirement = true;
        }
        public ConfigRequirement<X> ForceType<X>()
        {
            AsType<X>(out ConfigRequirement<X> cReq);
            return cReq;
        }
        public bool AsType<X>(out ConfigRequirement<X> cReq)
        {
            try
            {
                object? obj = Convert.ChangeType(StuckValue, typeof(X));
                if (obj != null)
                {
                    cReq = new ConfigRequirement<X>((X)obj, ModName);
                    return true;
                }
            }
            catch (Exception e)
            {
                cReq = default(ConfigRequirement<X>);
                return false;
            }
            cReq = default(ConfigRequirement<X>);
            return false;
        }
    }
}
