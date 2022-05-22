using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Internal.Exceptions
{
    /// <summary>
    /// Exception occurs if a custom value processor cannot be found, either because its was misspelled or because it is not static.
    /// </summary>
    internal class MonitoringTargetRegisteredDuplicateException : Exception
    {
        public MonitoringTargetRegisteredDuplicateException(object target) 
            : base($"{target} is already registered as a <b>Monitoring Target</b>!" +
                   $"\nEnsure to not call <b>{nameof(MonitoringManager)}.{nameof(MonitoringManager.RegisterTarget)}</b> " +
                   $"multiple times and don't make calls to " +
                   $"<b>{nameof(MonitoringManager)}.{nameof(MonitoringManager.RegisterTarget)}</b> in classes inheriting from " +
                   $"<b>{nameof(MonitoredBehaviour)}</b>, <b>{nameof(MonitoredObject)}</b> or similar!")
        {
        }
    }
}