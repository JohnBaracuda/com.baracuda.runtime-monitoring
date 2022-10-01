// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring
{
    public class MonitoredScriptableObject : ScriptableObject
    {
        protected virtual void OnEnable()
        {
            MonitoringSystems.MonitoringManager.RegisterTarget(this);
        }

        protected virtual void OnDisable()
        {
            MonitoringSystems.MonitoringManager.UnregisterTarget(this);
        }
    }
}
