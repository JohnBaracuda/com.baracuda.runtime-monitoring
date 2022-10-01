// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            MonitoringSystems.MonitoringManager.RegisterTarget(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringSystems.MonitoringManager.UnregisterTarget(this);
        }
    }
}
