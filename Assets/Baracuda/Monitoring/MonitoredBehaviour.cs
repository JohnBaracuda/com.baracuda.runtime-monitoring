// Copyright (c) 2022 Jonathan Lang
using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            MonitoringManager.RegisterTarget(this);
        }

        protected virtual void OnDestroy()
        {
            MonitoringManager.UnregisterTarget(this);
        }
    }
}