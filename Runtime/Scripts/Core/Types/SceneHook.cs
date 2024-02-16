// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;

namespace Baracuda.Monitoring.Types
{
    internal class SceneHook : MonoBehaviour
    {
        public UpdateRate sceneUpdateRate;
        internal event Action<float> SceneUpdateEvent;

        private void Update()
        {
            if (sceneUpdateRate == UpdateRate.Update)
                SceneUpdateEvent?.Invoke(Time.deltaTime);
        }
        private void LateUpdate()
        {

            if (sceneUpdateRate == UpdateRate.LateUpdate)
                SceneUpdateEvent?.Invoke(Time.deltaTime);
        }

        private void OnDestroy()
        {
            SceneUpdateEvent = null;
        }

        private void FixedUpdate()
        {
            if (sceneUpdateRate == UpdateRate.FixedUpdate)
                SceneUpdateEvent?.Invoke(Time.fixedDeltaTime);
        }
    }
}