namespace Baracuda.Monitoring
{
    /// <summary>
    /// Condition type.
    /// </summary>
    public enum Condition
    {
        /// <summary>
        /// Show if the value is false.
        /// </summary>
        False = 0,

        /// <summary>
        /// Show if the value is true.
        /// </summary>
        True = 1,

        /// <summary>
        /// Show if the value is null.
        /// </summary>
        Null = 2,

        /// <summary>
        /// Show if the value not null.
        /// </summary>
        NotNull = 3,

        /// <summary>
        /// Show if the value is a number and not zero (0).
        /// </summary>
        NotZero = 4,

        /// <summary>
        /// Show if the value is a number and zero (0).
        /// </summary>
        Zero = 10,

        /// <summary>
        /// Show if the value is a negative number.
        /// </summary>
        Negative = 5,

        /// <summary>
        /// Show if the value is a positive number.
        /// </summary>
        Positive = 6,

        /// <summary>
        /// Show if the value is a string that is not null or empty.
        /// </summary>
        NotNullOrEmptyString = 7,

        /// <summary>
        /// Show if the value is a string that is not null or white space.
        /// </summary>
        NotNullOrWhiteSpace = 8,

        /// <summary>
        /// Show if the value is a collection that is not null and contains at least 1 item.
        /// </summary>
        CollectionNotEmpty = 9,
    }
}