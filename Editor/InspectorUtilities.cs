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
        private static string Repository { get; } = "https://github.com/johnbaracuda/com.baracuda.runtime-monitoring";
        private static string Website { get; } = "https://johnbaracuda.com/";

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

        public static void DrawCopyrightNotice()
        {
            EditorGUILayout.LabelField(Copyright, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter}, GUILayout.ExpandWidth(true));
        }
    }
}