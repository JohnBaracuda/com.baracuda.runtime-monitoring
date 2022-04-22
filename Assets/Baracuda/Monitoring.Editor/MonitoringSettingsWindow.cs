using System;
using Baracuda.Monitoring.Management;
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
            _settings = MonitoringSettings.Instance();
            _inspector = UnityEditor.Editor.CreateEditor(_settings) as MonitoringSettingsInspector;
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUI.indentLevel = 1;
            _inspector.OnInspectorGUI();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation"))
            {
                Selection.activeObject = MonitoringSettings.Instance();
            }
            if (GUILayout.Button("Select Asset"))
            {
                Selection.activeObject = MonitoringSettings.Instance();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private static void DrawLine(int thickness = 1, int padding = 1)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            rect.height = thickness;
            rect.y += padding * .5f;
            rect.x -= 2;
            rect.width += 4;
            EditorGUI.DrawRect(rect, new Color(.1f, .1f, .1f, .9f));
        }
    }
}
