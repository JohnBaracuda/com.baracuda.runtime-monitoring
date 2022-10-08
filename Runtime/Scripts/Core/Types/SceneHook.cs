// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;

namespace Baracuda.Monitoring.Types
{
    internal class SceneHook : MonoBehaviour
    {
        internal event Action<float> LateUpdateEvent;
        
        private void LateUpdate()
        {
            LateUpdateEvent?.Invoke(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            LateUpdateEvent = null;
        }
    }
}