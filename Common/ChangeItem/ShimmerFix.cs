using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using MonoMod.Cil;

namespace CombatPlus.Common.ChangeItem
{
    public class ShimmerFix : GlobalItem
    {
        private List<int> CannotShimmer = new List<int>()
        {
            // Put whatever items you don't want here
        };

        public override void Load()
        {
            Terraria.On_Item.CanShimmer += On_Item_CanShimmer;
        }

        private bool On_Item_CanShimmer(On_Item.orig_CanShimmer orig, Item self)
        {
            foreach (int Item in CannotShimmer)
                if (self.type == Item)
                    return false;

            orig.Invoke(self);
            return true; // The award for the most useless piece of code goes to...
        }
    }
}
