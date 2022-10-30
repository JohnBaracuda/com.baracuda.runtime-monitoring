// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring;
using Baracuda.Monitoring.IL2CPP;
using Baracuda.Monitoring.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace Baracuda.Example.Scripts
{
    public class MonitoringExampleController : MonoBehaviour
    {
        [TypeDef(typeof(MonitoredStruct<int>))]

        [Header("Input")]
        [SerializeField] private LegacyPlayerInput playerInput;

        [Header("UI")]
        [SerializeField] private GameObject uiParent;

        [Header("Automation")]
        [SerializeField] private Transform tagButtonContainer;
        [SerializeField] private Transform typeStringButtonContainer;
        [SerializeField] private ButtonFilter buttonPrefab;

        private void Awake()
        {
            playerInput.InputModeChanged += OnToggleFilter;
            playerInput.ToggleMonitoring += OnToggleMonitoring;
            playerInput.ClearConsole += ConsoleMonitor.Clear;

            Monitor.Events.ProfilingCompleted += OnProfilingCompleted;
        }

        private void OnProfilingCompleted(IReadOnlyList<IMonitorHandle> monitorUnits, IReadOnlyList<IMonitorHandle> readOnlyList)
        {
            var monitoringSettings = Monitor.Settings;

            foreach (var customTag in Monitor.Registry.UsedTags)
            {
                Instantiate(buttonPrefab, tagButtonContainer).Filter = $"{monitoringSettings.FilterTagsSymbol.ToString()}{customTag}";
            }

            foreach (var typeString in Monitor.Registry.UsedTypeNames)
            {
                Instantiate(buttonPrefab, typeStringButtonContainer).Filter = typeString;
            }
        }

        private void OnToggleMonitoring()
        {
            Monitor.UI.Visible = !Monitor.UI.Visible;
        }

        private void OnToggleFilter(InputMode inputMode)
        {
            uiParent.SetActive(inputMode == InputMode.UserInterface);
        }

        public void OnInputChanged(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Monitor.UI.ResetFilter();
            }
            else
            {
                Monitor.UI.ApplyFilter(input);
            }
        }
    }
}
