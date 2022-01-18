using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public class MonitoredScriptableObject : ScriptableObject
    {
        public void Awake()
        {
            MonitoringManager.RegisterTarget(this);
        }

        private void OnDestroy()
        {
            MonitoringManager.UnregisterTarget(this);
        }
    }
}
