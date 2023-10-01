using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace CombatPlus.Common
{
    /// <summary>
    /// Should I set this as mod player?
    /// </summary>
    public class EarlyTemple : ModPlayer
    {
        public override void Load()
        {
            IL_Player.ItemCheck_UseTeleportRod += IL_Remove_Wall_Condition;
            IL_Player.CanDoWireStuffHere += IL_Remove_Wall_Condition;
            IL_Wiring.HitWireSingle += IL_Remove_Wall_Condition;

            IL_Wiring.DeActive += IL_Remove_Brick_Condition;
        }

        private void IL_Remove_Wall_Condition(ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(
                i => i.MatchLdcI4(87)
                )) return;

            c.Remove();
            c.EmitLdcI4(-1);
        }

        private void IL_Remove_Brick_Condition(ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(
                i => i.MatchLdcI4(226)
                )) return;

            c.Remove();
            c.EmitLdcI4(-1);
        }
    }
}
