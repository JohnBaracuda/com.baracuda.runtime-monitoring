// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal class MonitoringTicker : MonitoredObject
    {
        public bool ValidationTickEnabled { get; set; } = true;

        //--------------------------------------------------------------------------------------------------------------

        private readonly List<IMonitorHandle> _activeTickReceiver = new List<IMonitorHandle>(64);
        private readonly List<Action> _validationReceiver = new List<Action>(64);

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
                ValidationTick();
            };
        }
        private void Tick(float deltaTime)
        {
            if (!tickEnabled)
            {
                return;
            }

            updateTimer += deltaTime;
            if (updateTimer <= .05f)
            {
                return;
            }

            updateTimer = 0;
            UpdateTick();
            ValidationTick();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateTick()
        {
#if DEBUG
            for (var i = 0; i < _activeTickReceiver.Count; i++)
            {
                var monitorHandle = _activeTickReceiver[i];
                try
                {
                    monitorHandle.Refresh();
                }
                catch (Exception exception)
                {
                    Monitor.Logger.Log($"Error when refreshing {monitorHandle}\n(see next log for more information)", LogType.Warning, false);
                    Monitor.Logger.LogException(exception);
                    monitorHandle.Enabled = false;
                }
            }
#else
            for (var i = 0; i < _activeTickReceiver.Count; i++)
            {
                _activeTickReceiver[i].Refresh();
            }
#endif
        }

        private void ValidationTick()
        {
            if (!ValidationTickEnabled)
            {
                return;
            }
#if DEBUG
            try
            {
                for (var i = 0; i < _validationReceiver.Count; i++)
                {
                    _validationReceiver[i]();
                }
            }
            catch (Exception exception)
            {
                Monitor.Logger.LogException(exception);
            }
#else
            for (var i = 0; i < _validationReceiver.Count; i++)
            {
                _validationReceiver[i]();
            }
#endif
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
            _validationReceiver.Add(tickAction);
        }

        public void RemoveValidationTicker(Action tickAction)
        {
            _validationReceiver.Remove(tickAction);
        }
    }
}
