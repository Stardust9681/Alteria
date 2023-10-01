using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CombatPlus.Common.ChangeBuff {

    /// <summary>
    /// This is just for simple value changes in buffs
    /// </summary>
    public class BasicBuffChanges : GlobalBuff {
        public override void Update(int type, Player player, ref int buffIndex) {

            switch (type) {
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

        // THIS IS WHERE THE FUN BEGINS
        public override void Load()
        {
            // Venom
            IL_Player.UpdateLifeRegen += ILBuffChanges.IL_Player_UpdateBuffs;
            IL_NPC.UpdateNPC_BuffApplyDOTs += ILBuffChanges.IL_NPC_UpdateBuffs;
        }
    }
}
