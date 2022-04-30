using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public class MonitoredScriptableObject : ScriptableObject
    {
        protected virtual void OnEnable()
        {
            MonitoringUnitManager.RegisterTarget(this);
        }

        protected virtual void OnDisable()
        {
            MonitoringUnitManager.UnregisterTarget(this);
        }
    }
}
