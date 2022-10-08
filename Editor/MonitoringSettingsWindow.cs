// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Systems;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    /// <summary>
    /// Monitoring settings window.
    /// </summary>
    public class MonitoringSettingsWindow : EditorWindow
    {
        private MonitoringSettingsInspector _inspector;
        private Vector2 _scrollPosition;

        /// <summary>
        /// Open the monitoring settings window.
        /// </summary>
        public static void Open()
        {
            GetWindow<MonitoringSettingsWindow>("Monitoring").Show();
        }

        private void OnEnable()
        {
            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            _inspector = (MonitoringSettingsInspector) UnityEditor.Editor.CreateEditor(settings);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUI.indentLevel = 1;
            _inspector.DrawCustomInspector();
            EditorGUILayout.EndScrollView();

            InspectorUtilities.DrawLine(false);
            InspectorUtilities.DrawCopyrightNotice();
        }
    }
}
