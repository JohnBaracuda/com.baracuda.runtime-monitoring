using Baracuda.Monitoring.Management;
using UnityEditor;
using UnityEngine;

namespace Monitoring.Editor
{
    public class MenuItemLayout : MonoBehaviour
    {
        [MenuItem("Tools/Monitor/Settings", priority = 2405)]
        private static void OpenMonitorSettingsWindow()
        {
            Selection.activeObject = MonitoringSettings.Instance();
        }
    }
}
