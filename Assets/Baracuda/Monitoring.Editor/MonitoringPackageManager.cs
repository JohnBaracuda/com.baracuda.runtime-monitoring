// Copyright (c) 2022 Jonathan Lang
using System;
using System.Linq;
using System.Threading.Tasks;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Editor
{
    internal class MonitoringPackageManager : EditorWindow
    {
        #region --- Fields ---

        private Vector2 _scrollPosition;

        private readonly string _setupText =
            "<size=13>The default UIController for this tool is based on the IMGUI system, which can lead to additional performance overhead. " +
            "I would advise to import and use either the TextMeshPro or UIToolkit based Solutions instead.</size>";

        private FoldoutHandler Foldout { get; set; }
        
        #endregion

        #region --- Setup ---

        private static bool DidInstallerRun() => EditorPrefs.GetBool(nameof(MonitoringPackageManager), false);

        [InitializeOnLoadMethod]
        private static void ValidateInstaller()
        {
            if (DidInstallerRun())
            {
                return;
            }

            Open();
        }

        public static void Open()
        {
            GetWindow<MonitoringPackageManager>("Monitoring Packages").Show();
        }

        private void OnEnable()
        {
            Foldout = new FoldoutHandler();
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(nameof(MonitoringPackageManager), true);
        }

        #endregion

        #region --- GUI ---

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            if (Foldout["Welcome"])
            {
                EditorGUILayout.Space();

                EditorGUILayout.TextArea(_setupText, InspectorUtilities.TextStyle());

                EditorGUILayout.Space();
            }

            if (Foldout["Packages"])
            {
                EditorGUILayout.Space();

                DrawAdditionalPackages();

                EditorGUILayout.Space();
            }

            if (Foldout["Web Links & More"])
            {
                EditorGUILayout.Space();

                InspectorUtilities.DrawWeblinks();

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Open Settings", "Tools > Runtime Monitoring > Settings")))
                {
                    MonitoringSettingsWindow.Open();
                }

                InspectorUtilities.DrawLine();
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Confirm", GUILayout.Height(30)))
            {
                Close();
            }

            InspectorUtilities.DrawLine(false);
            InspectorUtilities.DrawCopyrightNotice();
        }

        private readonly GUIContent _showPackage =
            new GUIContent("Show Package", "Open and select the package in the project window");

        private readonly GUIContent _importPackage = 
            new GUIContent("Import Package", "Import the package into your project. This will open an import dialogue.");

        private void DrawAdditionalPackages()
        {
            
            // IMGUI
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            GUILayout.Label("IMGUI Package (default)", InspectorUtilities.TitleStyle());
            
            if (GUILayout.Button(_showPackage))
            {
                MoveToPackagePath("RuntimeMonitoring_IMGUI");
            }
            if (GUILayout.Button(_importPackage))
            {
                ImportPackage("RuntimeMonitoring_IMGUI");
            }
            
            EditorGUILayout.Space();
            GUILayout.EndVertical();
            
            // uGUI TextMeshPro
            EditorGUILayout.Space();
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            GUILayout.Label("TextMeshPro Package", InspectorUtilities.TitleStyle());
            
            if (GUILayout.Button(_showPackage))
            {
                MoveToPackagePath("RuntimeMonitoring_TextMeshPro");
            }
            if (GUILayout.Button(_importPackage))
            {
                ImportPackage("RuntimeMonitoring_TextMeshPro");
            }
            
            EditorGUILayout.Space();
            GUILayout.EndVertical();
            
            // UIToolkit
            EditorGUILayout.Space();
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            GUILayout.Label("UIToolkit (Unity 2021.1 Or Higher)", InspectorUtilities.TitleStyle());
            
            if (GUILayout.Button(_showPackage))
            {
                MoveToPackagePath("RuntimeMonitoring_UIToolkit");
            }

            if (GUILayout.Button(_importPackage))
            {
                ImportPackage("RuntimeMonitoring_UIToolkit");
            }
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        #endregion

        #region --- Package Installation ---

        private static string GetPackagePath(string packageName)
        {
            var assets = AssetDatabase.FindAssets(packageName);
            var assetGuid = assets.FirstOrDefault();
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            return assetPath;
        }
        
        private static void MoveToPackagePath(string packageName)
        {
            var assetPath = GetPackagePath(packageName);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                Debug.LogError($"Operation failed! <b>{packageName}.unitypackage</b> was not found! Did you delete or exclude this file on import?");
            }
        }

        private static void ImportPackage(string packageName)
        {
            var assetPath = GetPackagePath(packageName);
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                AssetDatabase.ImportPackage(assetPath, true);
            }
            else
            {
                Debug.LogError($"Import failed! <b>{packageName}.unitypackage</b> was not found! Did you delete or exclude this file on import?");
            }
        }

        #endregion
    }
}