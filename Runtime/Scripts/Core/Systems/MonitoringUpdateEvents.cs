// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Systems
{
    internal class MonitoringUpdateEvents
    {
        #region Fields And Properties

        public bool ValidationUpdateEnabled { get; set; } = true;

        private readonly List<IMonitorHandle> _activeTickReceiver = new List<IMonitorHandle>(64);
        private readonly List<Action> _validationReceiver = new List<Action>(64);

        private static float updateTimer;
        private static bool updateEnabled;

        #endregion


        #region Internal API

        internal void AddUpdateTicker(IMonitorHandle handle)
        {
            _activeTickReceiver.Add(handle);
        }

        internal void RemoveUpdateTicker(IMonitorHandle handle)
        {
            _activeTickReceiver.Remove(handle);
        }

        internal void AddValidationTicker(Action tickAction)
        {
            _validationReceiver.Add(tickAction);
        }

        internal void RemoveValidationTicker(Action tickAction)
        {
            _validationReceiver.Remove(tickAction);
        }

        internal MonitoringUpdateEvents()
        {
            Monitor.Events.ProfilingCompleted += MonitoringEventsOnProfilingCompleted;
        }

        #endregion


        #region Update

        private void MonitoringEventsOnProfilingCompleted(IReadOnlyList<IMonitorHandle> staticUnits,
            IReadOnlyList<IMonitorHandle> instanceUnits)
        {
            Assert.IsTrue(Application.isPlaying, "Application must be in playmode!");

            var sceneHook = new GameObject("Monitoring Scene Hook").AddComponent<SceneHook>();

            sceneHook.sceneUpdateRate = Monitor.Settings.SceneUpdateRate;

            Object.DontDestroyOnLoad(sceneHook);

            sceneHook.gameObject.hideFlags = Monitor.Settings.ShowRuntimeMonitoringObject
                ? HideFlags.None
                : HideFlags.HideInHierarchy;

            sceneHook.SceneUpdateEvent += Tick;

            updateEnabled = Monitor.UI.Visible;

            Monitor.UI.VisibleStateChanged += visible =>
            {
                updateEnabled = visible;
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
            if (!updateEnabled)
            {
                return;
            }

            updateTimer += deltaTime;
            if (!MonitoringSettings.Singleton.UpdatesWithLowTimeScale && updateTimer <= .05f)
            {
                return;
            }

            updateTimer = 0;
            UpdateTick();
            ValidationTick();
        }

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
                    MonitoringLogger.Log($"Error when refreshing {monitorHandle}\n(see next log for more information)",
                        LogType.Warning, false);
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
            if (!ValidationUpdateEnabled)
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

        #endregion
    }
}