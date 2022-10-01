// Copyright (c) 2022 Jonathan Lang

using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    internal class MenuItemLayout : MonoBehaviour
    {
        [MenuItem("Tools/Runtime Monitoring", priority = 2405)]
        private static void OpenMonitorSettingsWindow()
        {
            MonitoringSettingsWindow.Open();
        }
    }
}
