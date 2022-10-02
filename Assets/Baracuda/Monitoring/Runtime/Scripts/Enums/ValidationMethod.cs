namespace Baracuda.Monitoring
{
    /// <summary>
    /// Validation method type.
    /// </summary>
    public enum ValidationMethod
    {
        /// <summary>
        /// Value is validated via the result of an external member.
        /// </summary>
        ByMember,

        /// <summary>
        /// Value is validated via a comparison to another constant value that is passed set via attribute.
        /// </summary>
        Comparison,

        /// <summary>
        /// Value is validated via special condition. (e.g: collection not empty)
        /// </summary>
        Condition
    }
}