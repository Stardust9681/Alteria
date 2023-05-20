using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using static OtherworldMod.Common.ChangeNPC.Utilities.NPCMethods;
using static OtherworldMod.Common.ChangeNPC.Utilities.OtherworldNPCSets;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.Utilities;
using OtherworldMod.Common.ChangeNPC.AI;
using OtherworldMod.Common.ChangePlayer;

namespace CombatPlus.Common.ChangeNPC.Structure
{
    public class LootDrop
    {
        List<(int,int)> itemTypes;
        float rootChance;
        IItemDropRuleCondition condition;

        public void AddItem(int type, int stack = 1) => itemTypes.Add((type, stack));
        public void SetChance(float chance) => rootChance = chance;

        float EffectiveChance(int npcType)
        {
            float chance = 1 - MathF.Pow(1 - rootChance, NPC.killCount[npcType] + 1);
            return rootChance;
        }
        (int type, int stack) PickItem(int npcType)
        {
            if (itemTypes != null && itemTypes.Count != 0)
            {
                if (itemTypes.Count == 1)
                    return itemTypes[0];
                return Main.rand.Next(itemTypes);
            }
            return (0,0);
        }
        public bool TestLoot(NPC npc)
        {
            Player interactionPlayer = npc.AnyInteractions() ? Main.player[npc.lastInteraction] : Main.player[npc.FindClosestPlayer()];
            if (PlayerMethods.RollLuck(interactionPlayer) < EffectiveChance(npc.type))
            {
                (int, int) pickedItem = PickItem(npc.type);
                Item.NewItem(npc.GetSource_Loot(), npc.getRect(), pickedItem.Item1, pickedItem.Item2); 
                return true;
            }
            return false;
        }
    }
}
