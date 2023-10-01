using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CombatPlus.Common.ChangeBuff
{
    /// <summary>
    /// This is where the methods for IL changes are
    /// </summary>
    /*public class ILBuffChanges // DON'T MAKE IT AND INFERFACE YOU DAM IDE
    {
        public static void IL_Player_UpdateBuffs(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.venom))
                )) return;

            c.Index += 14;
            c.Remove();
            c.EmitLdcI4(-15);
        }

        public static void IL_NPC_UpdateBuffs(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(NPC), nameof(NPC.venom))
                )) return;

            c.Index += 13;
            c.Remove();
            c.EmitLdcI4(-30);
        }
    } fix later */
}
