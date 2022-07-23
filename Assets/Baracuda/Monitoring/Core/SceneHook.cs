// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring.Core
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