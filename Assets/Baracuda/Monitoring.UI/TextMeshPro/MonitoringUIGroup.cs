using TMPro;
using UnityEngine;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [RequireComponent(typeof(RectTransform))]
    public class MonitoringUIGroup : MonoBehaviour
    {
        [SerializeField] private TMP_Text groupTitle;
        
        public int ChildCount { get; set; }
        
        public void SetTitle(string title)
        {
            groupTitle.text = title;
        }
    }
}
