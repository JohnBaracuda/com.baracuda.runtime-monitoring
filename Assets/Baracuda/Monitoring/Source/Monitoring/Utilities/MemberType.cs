// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Enum contains all member types that can be monitored.
    /// </summary>
    public enum MemberType
    {
        /// <summary>
        /// The monitored member is a Field.
        /// </summary>
        Field,

        /// <summary>
        /// The monitored member is a Property.
        /// </summary>
        Property,

        /// <summary>
        /// The monitored member is an Event.
        /// </summary>
        Event,

        /// <summary>
        /// The monitored member is a Method.
        /// </summary>
        Method
    }
}