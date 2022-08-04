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

        public string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                GetComponentInChildren<Text>().text = _filter;
            }
        }
        private string _filter;
        private IMonitoringUI _monitoringUI;
        
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
                button.onClick.AddListener(() => _monitoringUI.ApplyFilter(Filter));
            }
        }
    }
}