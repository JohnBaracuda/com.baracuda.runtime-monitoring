using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public class MenuItemLayout : MonoBehaviour
    {
        [MenuItem("Tools/Monitor/Settings", priority = 2405)]
        private static void OpenMonitorSettingsWindow()
        {
            MonitoringSettingsWindow.Open();
        }
        
        [MenuItem("Tools/Monitor/Documentation", priority = 2405)]
        private static void OpenMonitoringDocumentation()
        {
            MonitoringSettingsWindow.Open();
        }
    }
}
