using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Alteria.Core.Util.Utils;

namespace Alteria.Common.ChangePlayer
{
    public class AlteriaPlayer : ModPlayer
    {
        public short healTimer;
        public int heal;

        public short manaTimer;
        public int mana;

        public override void OnEnterWorld()
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                ModPacket packet = Alteria.Instance.GetPacket(8);
                packet.Write(1);
                packet.Send();
            }
            else
                Core.Util.TargetCollective.AddTarget(new Core.Util.PlayerTarget(Player.whoAmI));
        }
        public void SetHeal(int healValue)
        {

        }
        public override void UpdateLifeRegen()
        {
            int FrontLoadFunc(float x, int H)
            {
                return (int)(-(x - 5) * H * .16f);
            }

            if (healTimer > 0)
            {
                int a = FrontLoadFunc(healTimer / 60f, heal);
                Player.lifeRegen += a;
                if (healTimer % 60 == 0)
                {
                    int b = FrontLoadFunc(healTimer / 60f - 1, heal);
                    int c = a / 3 + b / 5;
                    Player.HealEffect(c, false);
                }
                healTimer++;
                if (healTimer > 300)
                    healTimer = -1;
            }

            if (manaTimer > 0)
            {
                int a = FrontLoadFunc(manaTimer / 60f, mana);
                Player.manaRegen += a;
                if (manaTimer % 60 == 0)
                {
                    int b = FrontLoadFunc(manaTimer / 60f - 1, mana);
                    int c = a / 3 + b / 5;
                    Player.HealEffect(c, false);
                }
                manaTimer++;
                if (manaTimer > 300)
                    manaTimer = -1;
            }
        }
        public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
        {
            /*if (quickHeal)
            {
                mana = healValue;
                manaTimer = 1;
                healValue = 0;
            }*/
        }
    }
}
