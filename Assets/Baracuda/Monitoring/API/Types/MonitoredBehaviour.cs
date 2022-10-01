// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            MonitoringSystems.MonitoringManager.RegisterTarget(this);
        }

        protected virtual void OnDestroy()
        {
            MonitoringSystems.MonitoringManager.UnregisterTarget(this);
        }
    }
}