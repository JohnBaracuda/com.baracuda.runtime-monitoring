// Copyright (c) 2022 Jonathan Lang
 
namespace Baracuda.Monitoring.Source.Utilities
{
    internal enum ValidationPeriod
    {
        /// <summary>
        /// Validation will take place continuous during runtime.
        /// </summary>
        Runtime = 0,
        
        /// <summary>
        /// Validation will take place once during profiling. If validation fails, no unit profile will be created!
        /// </summary>
        Profiling = 1
    }
}