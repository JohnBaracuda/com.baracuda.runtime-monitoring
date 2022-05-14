using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public class MenuItemLayout : MonoBehaviour
    {
        [MenuItem("Tools/Runtime Monitoring/Settings", priority = 2405)]
        private static void OpenMonitorSettingsWindow()
        {
            MonitoringSettingsWindow.Open();
        }
        
        [MenuItem("Tools/Runtime Monitoring/Documentation", priority = 2405)]
        private static void OpenMonitoringDocumentation()
        {
            MonitoringSettingsWindow.Open();
        }
    }
}
