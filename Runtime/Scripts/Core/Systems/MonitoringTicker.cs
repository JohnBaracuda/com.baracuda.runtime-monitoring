// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal class MonitoringTicker
    {
        public bool ValidationTickEnabled { get; set; } = true;

        //--------------------------------------------------------------------------------------------------------------

        private readonly List<IMonitorHandle> _activeTickReceiver = new List<IMonitorHandle>(64);
        private event Action ValidationTick;

        private static float updateTimer;
        private static bool tickEnabled;

        //--------------------------------------------------------------------------------------------------------------

        internal MonitoringTicker()
        {
            Monitor.Events.ProfilingCompleted += MonitoringEventsOnProfilingCompleted;
        }

        private void MonitoringEventsOnProfilingCompleted(IReadOnlyList<IMonitorHandle> staticUnits, IReadOnlyList<IMonitorHandle> instanceUnits)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                throw new Exception("Application must be in playmode!");
            }
#endif

            var sceneHook = new GameObject("Monitoring Scene Hook").AddComponent<SceneHook>();

            UnityEngine.Object.DontDestroyOnLoad(sceneHook);

            sceneHook.gameObject.hideFlags = Monitor.Settings.ShowRuntimeMonitoringObject
                ? HideFlags.None
                : HideFlags.HideInHierarchy;

            sceneHook.LateUpdateEvent += Tick;

            tickEnabled = Monitor.UI.Visible;

            Monitor.UI.VisibleStateChanged += visible =>
            {
                tickEnabled = visible;
                if (!visible)
                {
                    return;
                }

                UpdateTick();
                ValidationTick?.Invoke();
            };
        }
        private void Tick(float deltaTime)
        {
            if (!tickEnabled)
            {
                return;
            }

            updateTimer += deltaTime;
            if (updateTimer > .05f)
            {
                updateTimer = 0;
                UpdateTick();

                if (ValidationTickEnabled)
                {
                    ValidationTick?.Invoke();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateTick()
        {
            for (var i = 0; i < _activeTickReceiver.Count; i++)
            {
                _activeTickReceiver[i].Refresh();
            }
        }

        public void AddUpdateTicker(IMonitorHandle handle)
        {
            _activeTickReceiver.Add(handle);
        }

        public void RemoveUpdateTicker(IMonitorHandle handle)
        {
            _activeTickReceiver.Remove(handle);
        }

        public void AddValidationTicker(Action tickAction)
        {
            ValidationTick += tickAction;
        }

        public void RemoveValidationTicker(Action tickAction)
        {
            ValidationTick -= tickAction;
        }
    }
}
