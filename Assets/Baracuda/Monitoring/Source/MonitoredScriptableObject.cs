// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public class MonitoredScriptableObject : ScriptableObject
    {
        protected virtual void OnEnable()
        {
            MonitoringSystems.Resolve<IMonitoringManager>().RegisterTarget(this);
        }

        protected virtual void OnDisable()
        {
            MonitoringSystems.Resolve<IMonitoringManager>().UnregisterTarget(this);
        }
    }
}
