// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Types;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            MonitoringSystems.Resolve<IMonitoringManager>().RegisterTarget(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringSystems.Resolve<IMonitoringManager>().UnregisterTarget(this);
        }
    }
}
