namespace OtherworldMod.Common.ChangeNPC.Structure
{
    /// <summary>
    /// Handful of standard rotation states
    /// </summary>
    public enum RotateMode : byte
    {
        /// <summary>
        /// Do not apply rotation to this entity
        /// </summary>
        NoRotation = 0b0000_0001,
        /// <summary>
        /// Apply rotation with a positive relationship to X velocity
        /// </summary>
        XRelative = 0b0000_0010,
        /// <summary>
        /// Apply rotation with a negative relationship to X velocity
        /// </summary>
        XAntiRelative = 0b0000_0100,
        /// <summary>
        /// Apply rotation with a positive relationship to Y velocity
        /// </summary>
        YRelative = 0b0000_1000,
        /// <summary>
        /// Apply rotation with a negative relationship to Y velocity
        /// </summary>
        YAntiRelative = 0b0001_0000,
        /// <summary>
        /// Rotate this entity to face its target position
        /// </summary>
        ToTarget = 0b0010_0000,
        /// <summary>
        /// Rotate this entity to face in the direction of its movement
        /// </summary>
        ToVelocity = 0b0100_0000,
        /// <summary>
        /// Rotate this entity over time, regardless of other circumstances
        /// </summary>
        OverTime = 0b1000_0000
    }
}
