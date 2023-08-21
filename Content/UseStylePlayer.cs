using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Alteria.Common.ChangeItem.Structure;

//Wonder if this should go into Common.ChangeItem or Common.ChangePlayer, instead of Content
//Let me know I guess
namespace Alteria.Content
{
    public class UseStylePlayer : ModPlayer
    {
        //Custom structure here 'cuz it makes me feel better uwu
        //But uh, for reals, slightly easier use of objData, and slightly better safety for conversion(s)
        public UseStyleData<object> objData = default(object);
        public UseStyleData<int> intData = 0;
        public UseStyleData<float> floatData = 0;
        public UseStyleData<bool> boolData = false;

        /// <summary>
        /// The percent representing how far the player's item animation is
        /// </summary>
        public float UseAnimation => 1f - ((float)Player.itemAnimation / (float)Player.itemAnimationMax);

        /// <summary>
        /// Sets the player's arms' position
        /// </summary>
        /// <param name="rotation">Arm rotation in Radians</param>
        /// <param name="stretch">Stretch value for arm</param>
        /// <param name="frontArm">True to change front arm, False to change back arm</param>
        /// <param name="setDirection">Whether or not to change the direction the player is facing based on rotation value</param>
        public void SetArm(float rotation, Player.CompositeArmStretchAmount stretch, bool frontArm = true, bool setDirection = false)
        {
            if (setDirection)
            {
                if (rotation > MathHelper.PiOver2 && rotation < 3 * MathHelper.PiOver2)
                    Player.direction = -1;
                else
                    Player.direction = 1;
            }
            if (frontArm)
            {
                Player.SetCompositeArmFront(true, stretch, rotation);
            }
            else
            {
                Player.SetCompositeArmBack(true, stretch, rotation);
            }
        }
    }
}
