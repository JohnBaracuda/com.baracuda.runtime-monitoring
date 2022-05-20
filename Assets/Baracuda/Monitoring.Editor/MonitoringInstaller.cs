// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Editor
{
    internal class MonitoringInstaller : EditorWindow
    {
        #region --- Fields ---

        private Vector2 _scrollPosition;

        private readonly string _setupText =
            "<size=13>The default UIController for this tool is based on the Unity GUI system, which can lead to additional performance overhead. " +
            "I would advise to import and use either the TextMeshPro or UIToolkit based UIController. " +
            "Select the zip containing the .unitypackage via the buttons below.</size>";

        private FoldoutHandler Foldout { get; set; }
        
        private const string TMP_PACKAGE_ID = "com.unity.textmeshpro";
        private static bool lockImport = false;

        #endregion

        #region --- Setup ---

        private static bool DidInstallerRun() => EditorPrefs.GetBool(nameof(MonitoringInstaller), false);

        [InitializeOnLoadMethod]
        private static void ValidateInstaller()
        {
            if (DidInstallerRun())
            {
                return;
            }

            Run();
        }

        public static void Run()
        {
            GetWindow<MonitoringInstaller>("Runtime Monitoring Setup").Show();
        }

        private void OnEnable()
        {
            Foldout = new FoldoutHandler();
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(nameof(MonitoringInstaller), true);
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

            if (Foldout["Additional Packages"])
            {
                EditorGUILayout.Space();

                DrawAdditionalPackages();

                EditorGUILayout.Space();
            }

            if (Foldout["Web Links"])
            {
                EditorGUILayout.Space();

                InspectorUtilities.DrawWeblinks();

                EditorGUILayout.Space();
            }

            if (Foldout["More"])
            {
                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Open Settings", "Tools > Runtime Monitoring > Settings")))
                {
                    MonitoringSettingsWindow.Open();
                }

                EditorGUILayout.Space();
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
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            GUILayout.Label("TextMeshPro Support", InspectorUtilities.TitleStyle());
            
            if (GUILayout.Button(_showPackage))
            {
                MoveToPackagePath("RuntimeMonitoring_TextMeshPro");
            }
            if (GUILayout.Button(_importPackage))
            {
                ImportPackage("RuntimeMonitoring_TextMeshPro");
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button(new GUIContent("Import TextMeshPro", "Import com.unity.textmeshpro")))
            {
                ImportTextMeshPro();
            }
            
            EditorGUILayout.Space();
            GUILayout.EndVertical();
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

        
        private static async void ImportTextMeshPro()
        {
            try
            {
                if (lockImport)
                {
                    return;
                }
                
                lockImport = true;
                
                var result = await AwaitResult(Client.Search(TMP_PACKAGE_ID));

                if (result == StatusCode.Success)
                {
                    Debug.Log(TMP_PACKAGE_ID + " is already installed!");
                    return;
                }
                
                var request = await AwaitResult(Client.Add(TMP_PACKAGE_ID));
                
                if (request == StatusCode.Success)
                {
                    Debug.Log("Installed " + TMP_PACKAGE_ID);
                }
                else if (request >= StatusCode.Failure)
                {
                    
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                lockImport = false;
            }
            
            async Task<StatusCode> AwaitResult(Request request)
            {
                while (!request.IsCompleted)
                {
                    await Task.Delay(25);
                }

                return request.Status;
            }
        }
        

        #endregion
    }
}