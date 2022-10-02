namespace Baracuda.Monitoring
{
    /// <summary>
    /// Comparison type.
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// Show if the current value and the passed 'other' value are equal.
        /// </summary>
        Equals,

        /// <summary>
        /// Show if the current value and the passed 'other' value are not equal.
        /// </summary>
        EqualsNot,

        /// <summary>
        /// Show if the current value is a number that is greater than the passed 'other' value.
        /// </summary>
        Greater,

        /// <summary>
        /// Show if the current value is a number that is greater or equal to the passed 'other' value.
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// Show if the current value is a number that is lesser than the passed 'other' value.
        /// </summary>
        Lesser,

        /// <summary>
        /// Show if the current value is a number that is lesser or equal to the passed 'other' value.
        /// </summary>
        LesserOrEqual
    }
}