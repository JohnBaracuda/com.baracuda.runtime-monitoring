// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Internal
{
    internal class MonitoringUpdateHook : MonoSingleton<MonitoringUpdateHook>
    {
        /*
         * Events   
         */
        
        internal event Action OnLateUpdate;
        internal event Action OnTick;

        /*
         * Tick loop fields   
         */

        private float _tickTimer = 0;
        private const float TICK_INTERVAL_IN_SECONDS = .0333f;

        /*
         * Unity Event Methods    
         */

        protected override void Awake()
        {
            base.Awake();
            
            var hideFlag = MonitoringSettings.GetInstance().ShowRuntimeMonitoringObject 
                ? HideFlags.None 
                : HideFlags.HideInHierarchy;
            
            gameObject.hideFlags = hideFlag;
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
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


        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnLateUpdate = null;
            OnTick = null;
        }
    }
}