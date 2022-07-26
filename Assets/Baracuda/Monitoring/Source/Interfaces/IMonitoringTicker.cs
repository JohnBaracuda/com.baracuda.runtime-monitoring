// Copyright (c) 2022 Jonathan Lang
 
using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Source.Interfaces
{
    internal interface IMonitoringTicker : IMonitoringSystem<IMonitoringTicker>
    {
        /// <summary>
        /// Toggle validation tick.
        /// </summary>
        bool ValidationTickEnabled { get; set; }
        
        void AddUpdateTicker(Action tickAction);
        void RemoveUpdateTicker(Action tickAction);
        
        void AddValidationTicker(Action tickAction);
        void RemoveValidationTicker(Action tickAction);
    }
}