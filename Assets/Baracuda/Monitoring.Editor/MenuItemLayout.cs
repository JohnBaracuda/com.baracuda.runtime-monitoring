// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
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
        
        [MenuItem("Tools/Runtime Monitoring/Setup", priority = 2500)]
        private static void RunMonitoringInstaller()
        {
            MonitoringInstaller.Run();
        }
        
        [MenuItem("Tools/Runtime Monitoring/Documentation", priority = 2407)]
        private static void OpenMonitoringDocumentation()
        {
            MonitoringSettingsWindow.Open();
        }
    }
}
