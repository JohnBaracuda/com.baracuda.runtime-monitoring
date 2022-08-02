using System;
using Baracuda.Monitoring.API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Baracuda.Monitoring.Example.Scripts
{
    [RequireComponent(typeof(Button))]
    public class ButtonFilter : MonoBehaviour
    {
        [SerializeField] private bool isReset;
        
        private IMonitoringUI _monitoringUI;
        private string _filter;
        
        private void Awake()
        {
            _monitoringUI = MonitoringSystems.Resolve<IMonitoringUI>();
            _filter = GetComponentInChildren<Text>().text;

            var button = GetComponent<Button>();

            if (isReset)
            {
                button.onClick.AddListener(() => _monitoringUI.ResetFilter());
            }
            else
            {
                button.onClick.AddListener(() => _monitoringUI.ApplyFilter(_filter));
            }
        }
    }
}