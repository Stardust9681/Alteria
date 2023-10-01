using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Alteria.Common.ChangeBuff
{
    /// <summary>
    /// This is just for simple value changes in buffs
    /// </summary>
    public class BasicBuffChanges : GlobalBuff
    {
        public override void Update(int type, Player player, ref int buffIndex)
        {

            switch (type)
            {
                case BuffID.Ichor:
                    player.statDefense -= 5;
                    break;
                case BuffID.Venom:
                    player.statDefense -= 10;
                    break;
            }
        }

        // This is just like player except for npcs (if you couldn't tell already)
        public override void Update(int type, NPC npc, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.Ichor:
                    npc.defense -= 5;
                    break;
                case BuffID.Venom:
                    npc.defense -= 10;
                    break;
            }
        }
    }

    /*
    IGNORE

    public class ILBuffChanges // DON'T MAKE IT AND INFERFACE YOU DAM IDE
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
    }
     
     */
}