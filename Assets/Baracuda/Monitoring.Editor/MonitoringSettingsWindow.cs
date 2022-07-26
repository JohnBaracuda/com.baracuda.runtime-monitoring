// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public class MonitoringSettingsWindow : EditorWindow
    {
        private MonitoringSettingsInspector _inspector;
        private Vector2 _scrollPosition;
        
        public static void Open()
        {
            GetWindow<MonitoringSettingsWindow>("Monitoring").Show();
        }

        private void OnEnable()
        {
            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            _inspector = (MonitoringSettingsInspector) UnityEditor.Editor.CreateEditor(settings);
            _inspector.Refresh();
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
