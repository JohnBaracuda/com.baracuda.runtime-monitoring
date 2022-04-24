using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring.Internal
{
    internal class MonitoringUpdateHook : MonoSingleton<MonitoringUpdateHook>
    {
        /*
         * Events   
         */
        
        internal event Action OnUpdate;
        internal event Action OnTick;
        internal event Action OnFixedUpdate;

        /*
         * Tick loop fields   
         */

        private float _tickTimer = 0;
        private const float TICK_INTERVAL_IN_SECONDS = .1f;

        /*
         * Unity Event Methods    
         */

        protected override void Awake()
        {
            base.Awake();
            gameObject.hideFlags = MonitoringSettings.GetInstance().ShowRuntimeObject ? HideFlags.None : HideFlags.HideInHierarchy;
        }

        private void Update()
        {
            OnUpdate?.Invoke();
            Tick();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Tick()
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= TICK_INTERVAL_IN_SECONDS)
            {
                OnTick?.Invoke();
                _tickTimer = 0;
            }
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnUpdate = null;
            OnTick = null;
            OnFixedUpdate = null;
        }
    }
}