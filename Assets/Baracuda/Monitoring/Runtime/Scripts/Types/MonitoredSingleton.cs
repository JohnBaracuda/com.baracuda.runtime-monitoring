// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring
{
    /// <inheritdoc />
    public abstract class MonitoredSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            MonitoringSystems.Manager.RegisterTarget(this);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringSystems.Manager.UnregisterTarget(this);
        }
    }
}
