// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Source.Interfaces
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