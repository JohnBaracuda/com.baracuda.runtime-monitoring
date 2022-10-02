// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Base class for monitored ScriptableObjects.
    /// </summary>
    public class MonitoredScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Ensure to call base.OnEnable when overriding this method.
        /// </summary>
        protected virtual void OnEnable()
        {
            MonitoringSystems.Manager.RegisterTarget(this);
        }

        /// <summary>
        /// Ensure to call base.OnDisable when overriding this method.
        /// </summary>
        protected virtual void OnDisable()
        {
            MonitoringSystems.Manager.UnregisterTarget(this);
        }
    }
}
