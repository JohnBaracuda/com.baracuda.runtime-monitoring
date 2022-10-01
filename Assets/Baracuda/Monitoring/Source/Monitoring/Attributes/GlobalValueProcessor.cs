// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a method as a Global Value Processor.
    /// Please ensure that a methods marked as a global value processor accept an IFormatData as their first argument,
    /// the type you want to process as a second argument and return a string!<br/>
    /// Target method must be static and should be pure, meaning that calling them should not affect
    /// state outside of their inputs and outputs.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Method)]
    public class GlobalValueProcessor : Attribute
    {
        /// <summary>
        /// Mark a method as a Global Value Processor.
        /// Please ensure that a methods marked as a global value processor accept an IFormatData as their first argument,
        /// the type you want to process as a second argument and return a string!<br/>
        /// Target method must be static and should be pure, meaning that calling them should not affect
        /// state outside of their inputs and outputs.
        /// </summary>
        public GlobalValueProcessor()
        {
        }
    }
}