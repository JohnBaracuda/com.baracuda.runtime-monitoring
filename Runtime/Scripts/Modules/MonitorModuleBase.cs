// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    /// <summary>
    /// Base type for some automatically monitored MonoBehaviours
    /// </summary>
    public class MonitorModuleBase : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = false;

        /// <summary>
        /// Awake method.
        /// </summary>
        protected virtual void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            Monitor.StartMonitoring(this);
        }

        private void OnDisable()
        {
            Monitor.StopMonitoring(this);
        }
    }
}