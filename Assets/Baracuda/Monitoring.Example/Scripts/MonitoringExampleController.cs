// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Modules;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class MonitoringExampleController : MonoBehaviour
    {
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
            
            MonitoringSystems.Resolve<IMonitoringManager>().ProfilingCompleted += OnProfilingCompleted;
        }

        private void OnProfilingCompleted(IReadOnlyList<IMonitorUnit> monitorUnits, IReadOnlyList<IMonitorUnit> readOnlyList)
        {
            var monitoringSettings = MonitoringSystems.Resolve<IMonitoringSettings>();
            var utils = MonitoringSystems.Resolve<IMonitoringUtility>();
            
            foreach (var customTag in utils.GetAllTags())
            {
                Instantiate(buttonPrefab, tagButtonContainer).Filter = monitoringSettings.FilterTagsSymbol + customTag;
            }
            
            foreach (var typeString in utils.GetAllTypeStrings())
            {
                Instantiate(buttonPrefab, typeStringButtonContainer).Filter =  typeString;
            }
        }

        private void OnToggleMonitoring()
        {
            MonitoringSystems.Resolve<IMonitoringUI>().ToggleDisplay();
        }

        private void OnToggleFilter(InputMode inputMode)
        {
            uiParent.SetActive(inputMode == InputMode.UserInterface);
        }
    
        public void OnInputChanged(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MonitoringSystems.Resolve<IMonitoringUI>().ResetFilter();
            }
            else
            {
                MonitoringSystems.Resolve<IMonitoringUI>().ApplyFilter(input);
            }
        }
    }
}
