// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public static class InspectorUtilities
    {
        private static Color TextColor => EditorGUIUtility.isProSkin? new Color(0.84f, 0.84f, 0.84f) : Color.black;

        internal static GUIStyle TextStyle()
        {
            var style = GUI.skin.GetStyle("Box");
            style.stretchWidth = true;
            style.normal.textColor = TextColor;
            style.richText = true;
            style.alignment = TextAnchor.UpperLeft;
            return style;
        }
        
        internal static GUIStyle TitleStyle()
        {
            var style = GUI.skin.GetStyle("Box");
            style.normal.textColor = TextColor;
            style.stretchWidth = true;
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;
            return style;
        }
        
        internal static GUIStyle RichTextStyle()
        {
            var style = GUI.skin.GetStyle("Label");
            style.richText = true;
            return style;
        }
        
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
        
        public static void DrawWeblinksWithLabel()
        {
            var plugin = MonitoringSystems.Resolve<IMonitoringPlugin>();
            
            // Documentation
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Documentation", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(plugin.Documentation))
            {
                Application.OpenURL(plugin.Documentation);
            }
            GUILayout.EndHorizontal();
            
            // Repository
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Repository", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(plugin.Repository))
            {
                Application.OpenURL(plugin.Repository);
            }
            GUILayout.EndHorizontal();
            
            // Website
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Website", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(plugin.Website))
            {
                Application.OpenURL(plugin.Website);
            }
            GUILayout.EndHorizontal();
        }
        
        public static void DrawWeblinks()
        {
            var plugin = MonitoringSystems.Resolve<IMonitoringPlugin>();
            
            // Documentation
            if (GUILayout.Button(new GUIContent("Documentation", plugin.Documentation)))
            {
                Application.OpenURL(plugin.Documentation);
            }
            
            // Repository
            if (GUILayout.Button(new GUIContent("GitHub Repository", plugin.Repository)))
            {
                Application.OpenURL(plugin.Repository);
            }
            
            // Website
            if (GUILayout.Button(new GUIContent("Website", plugin.Website)))
            {
                Application.OpenURL(plugin.Website);
            }
        }

        private static bool IsValidPath(string path)
        {
            return path.StartsWith(Application.dataPath);
        }

        public static void DrawCopyrightNotice()
        {
            var copyright = MonitoringSystems.Resolve<IMonitoringPlugin>().Copyright;
            EditorGUILayout.LabelField(copyright, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter}, GUILayout.ExpandWidth(true));
        }
    }
}