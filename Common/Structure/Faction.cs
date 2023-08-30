using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alteria.Common.Structure
{
    [FlagsAttribute]
    public enum Faction : ushort
    {
        None = 0b0000_0000_0000_0000, //NONE - Mostly for logic
								All = 0b1111_1111_1111_1111, //ALL - Mostly for logic

								//Universals:
								UnivFriendly = 0b0000_0000_0000_0001, //All friendly
        UnivHostile = 0b0000_0000_0000_0010, //All hostile
        //0_0_0_0100
        //0_0_0_1000
								UnivsAll = UnivFriendly | UnivHostile,

        //Teams:
        TeamWhite = 0b0000_0000_0001_0000, //No team
        TeamBlue = 0b0000_0000_0010_0000, //Blue team
        TeamRed = 0b0000_0000_0100_0000, //Red team
        TeamGreen = 0b0000_0000_1000_0000, //Green team
        TeamPink = 0b0000_0001_0000_0000, //Pink team
        TeamYellow = 0b0000_0010_0000_0000, //Yellow team
        //0_0100_0_0
        //0_1000_0_0
        TeamsAll = TeamWhite | TeamBlue | TeamRed | TeamGreen | TeamPink | TeamYellow, //ALL TEAMS - Mostly for logic

        //Other flags
        FlagAntiTeam = 0b0001_0000_0000_0000, //Only attacked by/attacks own team(s)
        FlagSupport = 0b0010_0000_0000_0000, //Support entity
        FlagAttack = 0b0100_0000_0000_0000, //Attack entity
        //1000_0_0_0
								FlagsAll = FlagAntiTeam | FlagSupport | FlagAttack, //ALL FLAGS - Mostly for logic
    }

				public static class FactionMethods
				{
								public static Faction ExtractUnivs(this Faction a)
								{
												return a & Faction.UnivsAll;
								}

								public static Faction ExtractTeam(this Faction a)
								{
												return a & Faction.TeamsAll;
								}

								public static Faction ExtractFlags(this Faction a)
								{
												return a & Faction.FlagsAll;
								}

								public static bool IsEnemiesWith(this Faction a, Faction b)
								{
												Faction comp = Faction.UnivsAll | Faction.TeamsAll;

												if ((a & Faction.FlagAntiTeam) == Faction.None)
												{
																return ((a & b) & comp) == Faction.None;
												}

												return ((a & b) & comp) != Faction.None;
								}
								public static bool IsFriendsWith(this Faction a, Faction b)
								{
												Faction comp = Faction.UnivsAll | Faction.TeamsAll;

												if ((a & Faction.FlagAntiTeam) != Faction.None)
												{
																return ((a & b) & comp) == Faction.None;
												}

												return ((a & b) & comp) == Faction.None;
								}
				}
}
