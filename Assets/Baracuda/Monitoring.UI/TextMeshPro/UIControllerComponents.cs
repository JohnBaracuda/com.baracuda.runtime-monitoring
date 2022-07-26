using UnityEngine;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [ExecuteAlways]
    internal class UIControllerComponents : MonoBehaviour
    {
        public RectTransform upperLeftTransform;
        public RectTransform upperRightTransform;
        public RectTransform lowerLeftTransform;
        public RectTransform lowerRightTransform;
        
        public MonitoringUIElement elementPrefab;
        public MonitoringUIGroup groupPrefab;
    }
}