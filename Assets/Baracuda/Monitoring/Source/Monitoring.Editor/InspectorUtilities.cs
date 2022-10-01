// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    internal static class InspectorUtilities
    {
        private static string Copyright { get; } = "© 2022 Jonathan Lang";
        private static string Documentation { get; } = "https://johnbaracuda.com/monitoring.html";
        private static string Repository { get; } = "https://github.com/johnbaracuda/Runtime-Monitoring";
        private static string Website { get; } = "https://johnbaracuda.com/";

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

            const int THICKNESS = 1;
            const int PADDING = 1;
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(PADDING + THICKNESS));
            rect.height = THICKNESS;
            rect.y += PADDING * .5f;
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
            // Documentation
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Documentation", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(Documentation))
            {
                Application.OpenURL(Documentation);
            }
            GUILayout.EndHorizontal();

            // Repository
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Repository", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(Repository))
            {
                Application.OpenURL(Repository);
            }
            GUILayout.EndHorizontal();

            // Website
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Website", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(Website))
            {
                Application.OpenURL(Website);
            }
            GUILayout.EndHorizontal();
        }

        public static void DrawWeblinks()
        {
            // Documentation
            if (GUILayout.Button(new GUIContent("Documentation", Documentation)))
            {
                Application.OpenURL(Documentation);
            }

            // Repository
            if (GUILayout.Button(new GUIContent("GitHub Repository", Repository)))
            {
                Application.OpenURL(Repository);
            }

            // Website
            if (GUILayout.Button(new GUIContent("Website", Website)))
            {
                Application.OpenURL(Website);
            }
        }

        private static bool IsValidPath(string path)
        {
            return path.StartsWith(Application.dataPath);
        }

        public static void DrawCopyrightNotice()
        {
            EditorGUILayout.LabelField(Copyright, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter}, GUILayout.ExpandWidth(true));
        }
    }
}