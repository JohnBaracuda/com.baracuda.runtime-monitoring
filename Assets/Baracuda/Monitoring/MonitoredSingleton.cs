// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            MonitoringManager.RegisterTarget(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringManager.UnregisterTarget(this);
        }
    }
}
