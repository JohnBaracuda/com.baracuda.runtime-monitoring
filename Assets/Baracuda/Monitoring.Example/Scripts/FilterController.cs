// Copyright (c) 2022 Jonathan Lang
 
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Modules;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class FilterController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private LegacyPlayerInput _playerInput;

        [Header("UI")] 
        [SerializeField] private GameObject uiParent;
    
        private void Awake()
        {
            _playerInput.InputModeChanged += OnToggleFilter;
            _playerInput.ClearConsole += ConsoleMonitor.Clear;
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
