using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Core
{
    internal interface IMonitoringTicker : IMonitoringService<IMonitoringTicker>
    {
        void AddUpdateTicker(Action tickAction);
        void RemoveUpdateTicker(Action tickAction);
        
        void AddValidationTicker(Action tickAction);
        void RemoveValidationTicker(Action tickAction);
    }
}