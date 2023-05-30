namespace OtherworldMod.Common.ChangeNPC.Structure
{
    /// <summary>
    /// 
    /// </summary>
    [System.Flags]
    public enum TargetMode
    {
        /// <summary>
        /// Do not run targetting
        /// </summary>
        NoTarget = 0b0000_0000,

        /// <summary>
        /// Default targetting. Will target players or friendly NPCs if hostile, will target hostile NPCs if friendly
        /// </summary>
        Default = 0b0000_0001,
        /// <summary>
        /// Only target players
        /// </summary>
        PlayerOnly = 0b0000_0010,
        /// <summary>
        /// Only target NPCs
        /// </summary>
        NPCOnly = 0b0000_0100,
        /// <summary>
        /// Target any entities that aren't of the same faction
        /// </summary>
        Any = 0b0000_1000,
        /// <summary>
        /// Target any entity
        /// </summary>
        AnyIgnoreFriends = 0b0001_0000,
    }
}
