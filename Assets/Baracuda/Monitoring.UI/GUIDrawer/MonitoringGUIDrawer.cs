using UnityEngine;

namespace Baracuda.Monitoring.UI.GUIDrawer
{
    public class MonitoringGUIDrawer : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.TextField(new Rect(0, 0, 300, 100), "text");
        }
    }
}
