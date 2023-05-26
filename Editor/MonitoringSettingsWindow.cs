// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Systems;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    /// <summary>
    ///     Monitoring settings window.
    /// </summary>
    public class MonitoringSettingsWindow : UnityEditor.EditorWindow
    {
        private MonitoringSettingsInspector _inspector;
        private Vector2 _scrollPosition;

        /// <summary>
        ///     Open the monitoring settings window.
        /// </summary>
        public static void Open()
        {
            GetWindow<MonitoringSettingsWindow>("Monitoring").Show();
        }

        private void OnEnable()
        {
            if (MonitoringSettings.Singleton != null)
            {
                _inspector =
                    (MonitoringSettingsInspector) UnityEditor.Editor.CreateEditor(MonitoringSettings.Singleton);
            }
        }

        private void OnGUI()
        {
            if (_inspector == null && MonitoringSettings.Singleton == null)
            {
                if (GUILayout.Button("Create Monitoring Settings"))
                {
                    MonitoringSettings.CreateSettingsAsset();
                    _inspector =
                        (MonitoringSettingsInspector) UnityEditor.Editor.CreateEditor(MonitoringSettings.Singleton);
                }
                return;
            }

            _scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(_scrollPosition);
            UnityEditor.EditorGUI.indentLevel = 1;
            _inspector.DrawCustomInspector();
            UnityEditor.EditorGUILayout.EndScrollView();

            InspectorUtilities.DrawLine(false);
            InspectorUtilities.DrawCopyrightNotice();
        }
    }
}