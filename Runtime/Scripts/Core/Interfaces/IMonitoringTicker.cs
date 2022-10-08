// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.Interfaces
{
    internal interface IMonitoringTicker : IMonitoringSubsystem<IMonitoringTicker>
    {
        /// <summary>
        /// Toggle validation tick.
        /// </summary>
        bool ValidationTickEnabled { get; set; }
        
        void AddUpdateTicker(IMonitorUnit unit);
        void RemoveUpdateTicker(IMonitorUnit unit);
        
        void AddValidationTicker(Action tickAction);
        void RemoveValidationTicker(Action tickAction);
    }
}