using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alteria.Common.Structure
{
    [FlagsAttribute]
    public enum Faction : short
    {
        //Universals:
        UnivNoFac = 0b0000_0000_0000_0000, //No friendly, no hostile
        UnivFriendly = 0b0000_0000_0000_0001, //All friendly
        UnivHostile = 0b0000_0000_0000_0010, //All hostile
        //0_0_0_0100
        //0_0_0_1000

        //Teams:
        TeamWhite = 0b0000_0000_0001_0000, //No team
        TeamBlue = 0b0000_0000_0010_0000, //Blue team
        TeamRed = 0b0000_0000_0100_0000, //Red team
        TeamGreen = 0b0000_0000_1000_0000, //Green team
        TeamPink = 0b0000_0001_0000_0000, //Pink team
        TeamYellow = 0b0000_0010_0000_0000, //Yellow team
        //0_0100_0_0
        //0_1000_0_0
        TeamsAll = TeamWhite | TeamBlue | TeamRed | TeamGreen | TeamPink | TeamYellow,

        //Other flags
        FlagAntiTeam = 0b0001_0000_0000_0000, //Only attacked by/attacks own team(s)
        FlagSupport = 0b0010_0000_0000_0000, //Support entity
        FlagAttack = 0b0100_0000_0000_0000, //Attack entity
        //1000_0_0_0
    }
}
