
using Baracuda.Monitoring.Management;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.EditorScripts
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
