using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            MonitoringManager.RegisterTarget(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringManager.UnregisterTarget(this);
        }
    }
}
