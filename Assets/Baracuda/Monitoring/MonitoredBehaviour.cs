using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            MonitoringUnitManager.RegisterTarget(this);
        }

        protected virtual void OnDestroy()
        {
            MonitoringUnitManager.UnregisterTarget(this);
        }
    }
}