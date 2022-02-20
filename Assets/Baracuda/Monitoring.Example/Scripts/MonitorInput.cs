using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class MonitorInput : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        private IMonitoringUI _monitoringUI;

        private void Awake()
        {
            _monitoringUI = GetComponent<IMonitoringUI>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (_monitoringUI.IsActive)
                {
                    _monitoringUI.Hide();
                }
                else
                {
                    _monitoringUI.Show();
                }
            }
        }
    }
}
