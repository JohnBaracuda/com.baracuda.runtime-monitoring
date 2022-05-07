using System;
using System.IO;
using Baracuda.Monitoring.API;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public  class InspectorUtilities
    {
        internal static void DrawLine(bool spaceBefore = true)
        {
            if (spaceBefore)
            {
                EditorGUILayout.Space();
            }

            const int thickness = 1;
            const int padding = 1;
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            rect.height = thickness;
            rect.y += padding * .5f;
            rect.x -= 2;
            rect.width += 4;
            EditorGUI.DrawRect(rect, new Color(.1f, .1f, .1f, .9f));
        }

        internal static void DrawFilePath(SerializedProperty property, string fileExtension)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                var path = property.stringValue;
                
                GUILayout.BeginHorizontal();
                path = EditorGUILayout.TextField(property.displayName, path);
                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    var newPath = EditorUtility.OpenFilePanel("Select File", IsValidPath(path)? path : Application.dataPath, fileExtension);
                    path = !string.IsNullOrWhiteSpace(newPath) ? newPath : path;
                }
                GUILayout.EndHorizontal();

                property.stringValue = IsValidPath(path)? path : Application.dataPath;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
            else
            {
                throw new InvalidCastException("FilePath Property must be a string!");
            }
        }

        private static bool IsValidPath(string path)
        {
            return 
#if UNITY_2021_1_OR_NEWER
                Path.IsPathFullyQualified(path) && 
#endif
                path.StartsWith(Application.dataPath);
        }

        public static void DrawCopyrightNotice()
        {
            EditorGUILayout.LabelField(MonitoringSettings.COPYRIGHT, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter}, GUILayout.ExpandWidth(true));
        }
    }
}