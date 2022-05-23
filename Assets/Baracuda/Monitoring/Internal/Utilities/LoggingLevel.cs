// Copyright (c) 2022 Jonathan Lang
namespace Baracuda.Monitoring.Internal.Utilities
{
    public enum LoggingLevel
    {
        /// <summary>
        /// Exceptions will be ignored. 
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Exceptions will be logged as a message.
        /// </summary>
        Message,
        
        /// <summary>
        /// Exceptions will be logged as a warning.
        /// </summary>
        Warning,
        
        /// <summary>
        /// Exceptions will be logged as an error message.
        /// </summary>
        Error,
        
        /// <summary>
        /// Exceptions will be thrown.
        /// </summary>
        Exception
    }
}