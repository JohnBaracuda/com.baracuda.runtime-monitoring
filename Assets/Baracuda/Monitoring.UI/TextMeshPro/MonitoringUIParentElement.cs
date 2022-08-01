using UnityEngine;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [DisableMonitoring]
    [RequireComponent(typeof(RectTransform))]
    internal abstract class MonitoringUIParentElement : MonoBehaviour
    {
        internal abstract int Order { get; }
    }
}