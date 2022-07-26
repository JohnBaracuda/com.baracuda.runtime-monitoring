// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    public class MonitorModuleBase : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = false;

        protected virtual void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            this.RegisterMonitor();
        }

        private void OnDisable()
        {
            this.UnregisterMonitor();
        }
    }
}