using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            MonitoringUnitManager.RegisterTarget(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringUnitManager.UnregisterTarget(this);
        }
    }
}
