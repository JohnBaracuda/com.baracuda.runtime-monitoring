// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Monitoring.API;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public class MonitoringSettingsWindow : EditorWindow
    {
        private MonitoringSettings _settings;
        private MonitoringSettingsInspector _inspector;
        private Vector2 _scrollPosition;
        
        public static void Open()
        {
            GetWindow<MonitoringSettingsWindow>("Monitoring").Show();
        }

        private void OnEnable()
        {
            _settings = MonitoringSettings.GetInstance();
            _inspector = UnityEditor.Editor.CreateEditor(_settings) as MonitoringSettingsInspector;
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
