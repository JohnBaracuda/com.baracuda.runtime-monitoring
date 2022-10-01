// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring.Types
{
    /// <summary>
    /// Base class for monitored MonoBehaviours.
    /// </summary>
    public abstract class MonitoredBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Ensure to call base.Awake when overriding this method.
        /// </summary>
        protected virtual void Awake()
        {
            MonitoringSystems.MonitoringManager.RegisterTarget(this);
        }

        /// <summary>
        /// Ensure to call base.OnDestroy when overriding this method.
        /// </summary>
        protected virtual void OnDestroy()
        {
            MonitoringSystems.MonitoringManager.UnregisterTarget(this);
        }
    }
}