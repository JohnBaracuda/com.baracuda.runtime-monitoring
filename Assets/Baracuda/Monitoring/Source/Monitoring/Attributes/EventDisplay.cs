// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Event display options.
    /// </summary>
    [Flags]
    public enum EventDisplay
    {
        /// <summary>
        /// Empty value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Display the subscriber count of the event.
        /// </summary>
        SubCount = 1,

        /// <summary>
        /// Display how many times the event was called.
        /// </summary>
        InvokeCount = 2,

        /// <summary>
        /// Show the actual subscriber count.
        /// </summary>
        TrueCount = 4,

        /// <summary>
        /// Display a list of subscriber.
        /// </summary>
        SubInfo = 8,

        /// <summary>
        /// Display the events signature.
        /// </summary>
        Signature = 16
    }
}