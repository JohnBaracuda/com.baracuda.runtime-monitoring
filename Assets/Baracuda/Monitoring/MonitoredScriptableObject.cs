using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public class MonitoredScriptableObject : ScriptableObject
    {
        protected virtual void OnEnable()
        {
            MonitoringManager.RegisterTarget(this);
        }

        protected virtual void OnDisable()
        {
            MonitoringManager.UnregisterTarget(this);
        }
    }
}
