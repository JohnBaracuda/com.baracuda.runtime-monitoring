using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class MonitorInput : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        private IMonitoringDisplayHandler _monitoringDisplayHandler;

        private void Awake()
        {
            _monitoringDisplayHandler = GetComponent<IMonitoringDisplayHandler>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (_monitoringDisplayHandler.IsActive)
                {
                    _monitoringDisplayHandler.Hide();
                }
                else
                {
                    _monitoringDisplayHandler.Show();
                }
            }
        }
    }
}
