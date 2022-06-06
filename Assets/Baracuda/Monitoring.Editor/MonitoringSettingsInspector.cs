// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringSettings))]
    public class MonitoringSettingsInspector : UnityEditor.Editor
    {
        #region --- Data ---

        /*
         * Private Fields 
         */
        
        private FoldoutHandler Foldout { get; set; }


        /*
         * Serialized Properties   
         */

#pragma warning disable CS0649
#pragma warning disable CS0169
        private SerializedProperty _enableMonitoring;
        private SerializedProperty _autoInstantiateUI;
        private SerializedProperty _openDisplayOnLoad;
        private SerializedProperty _asyncProfiling;
        private SerializedProperty _monitoringUIController;
        
        private SerializedProperty _showRuntimeMonitoringObject; 
        private SerializedProperty _showRuntimeUIController;
        private SerializedProperty _logBadImageFormatException;
        private SerializedProperty _logOperationCanceledException;
        private SerializedProperty _logThreadAbortException;
        private SerializedProperty _logUnknownExceptions;
        private SerializedProperty _logProcessorNotFoundException;
        private SerializedProperty _logInvalidProcessorSignatureException;
        
        private SerializedProperty _addClassName;
        private SerializedProperty _appendSymbol;
        private SerializedProperty _humanizeNames;
        private SerializedProperty _variablePrefixes;

        private SerializedProperty _floatFormat;
        private SerializedProperty _integerFormat;
        private SerializedProperty _vectorFormat;
        private SerializedProperty _quaternionFormat;
        
        private SerializedProperty _trueColor;
        private SerializedProperty _falseColor;
        private SerializedProperty _xColor;
        private SerializedProperty _yColor;
        private SerializedProperty _zColor;
        private SerializedProperty _wColor;
        private SerializedProperty _classColor;
        private SerializedProperty _eventColor;
        
        private SerializedProperty _bannedAssemblyPrefixes;
        private SerializedProperty _bannedAssemblyNames;
        
        private SerializedProperty _filePathIL2CPPTypes;
        private SerializedProperty _useIPreprocessBuildWithReport;
        private SerializedProperty _throwOnTypeGenerationError;
        private SerializedProperty _preprocessBuildCallbackOrder;

        #endregion

        #region --- Setup ---

        private void OnEnable()
        {
            Foldout = new FoldoutHandler(nameof(MonitoringSettings));
            PopulateSerializedProperties();
            PopulateAvailableUIController();
            
            AssetDatabase.importPackageCompleted -= ImportPackageCompleted;
            AssetDatabase.importPackageCompleted += ImportPackageCompleted;
        }

        private void OnDisable()
        {
            AssetDatabase.importPackageCompleted -= ImportPackageCompleted;
        }

        internal void Refresh()
        {
            PopulateAvailableUIController();
        }
        
        private void ImportPackageCompleted(string packageName)
        {
            PopulateAvailableUIController();
            
            if (!_availableUIController.Any())
            {
                return;
            }

            if (!packageName.StartsWith("RuntimeMonitoring"))
            {
                return;
            }
            
            for (var i = 0; i < _availableUIController.Count; i++)
            {
                var controllerName = _availableUIController[i].name.Replace("MonitoringUIController_", "");
                var package =  packageName.Replace("RuntimeMonitoring_", "");
                if (package == controllerName)
                {
                    _monitoringUIController.objectReferenceValue = _availableUIController[i];
                    _monitoringUIController.serializedObject.ApplyModifiedProperties();
                    break;
                }
            }
        }

        protected void PopulateSerializedProperties()
        {
            const BindingFlags FLAGS =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            var fields = GetType().GetFields(FLAGS);

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.FieldType == typeof(SerializedProperty))
                {
                    fieldInfo.SetValue(this, FindProperty(fieldInfo.Name));
                }
            }
        }
        
        private SerializedProperty FindProperty(string member)
        {
            var parsedMemberName = member.BeginsWith('_') ? member.Remove(0, 1) : member;
            return serializedObject.FindProperty(parsedMemberName);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- GUI ---

        public void DrawCustomInspector()
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = 300;

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Some Settings may not update during runtime!", MessageType.Info);
                EditorGUILayout.Space();
            }
            
            if (Foldout["General"])
            {
                EditorGUILayout.Space();
                DrawEnableMonitoring();
                DrawAsyncProfiling();
                EditorGUILayout.PropertyField(_openDisplayOnLoad);
                EditorGUILayout.Space();
            }

            if (Foldout["UI Controller"])
            {
                DrawUIController();
            }

            if (Foldout["Optional Packages"])
            {
                DrawAdditionalPackages();
            }
            
            if (Foldout["Formatting"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_addClassName);
                EditorGUILayout.PropertyField(_appendSymbol);
                EditorGUILayout.PropertyField(_humanizeNames);
                EditorGUILayout.PropertyField(_variablePrefixes);
                EditorGUILayout.Space();
            }
          
            if (Foldout["Color"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_trueColor);
                EditorGUILayout.PropertyField(_falseColor);
                EditorGUILayout.PropertyField(_xColor);
                EditorGUILayout.PropertyField(_yColor);
                EditorGUILayout.PropertyField(_zColor);
                EditorGUILayout.PropertyField(_wColor);
                EditorGUILayout.PropertyField(_classColor);
                EditorGUILayout.PropertyField(_eventColor);
                EditorGUILayout.Space();
            }
            
            if (Foldout["Debug"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_showRuntimeMonitoringObject);
                EditorGUILayout.PropertyField(_showRuntimeUIController);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_logBadImageFormatException);
                EditorGUILayout.PropertyField(_logOperationCanceledException);
                EditorGUILayout.PropertyField(_logThreadAbortException);
                EditorGUILayout.PropertyField(_logUnknownExceptions);
                EditorGUILayout.PropertyField(_logProcessorNotFoundException);
                EditorGUILayout.PropertyField(_logInvalidProcessorSignatureException);
                EditorGUILayout.Space();
            }
            
            if (Foldout["Assembly Settings"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_bannedAssemblyPrefixes);
                EditorGUILayout.PropertyField(_bannedAssemblyNames);
                EditorGUILayout.Space();
            }
            
            if (Foldout["IL2CPP Settings"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Make sure to delete old files when changing paths!", MessageType.None);
                InspectorUtilities.DrawFilePath(_filePathIL2CPPTypes, "cs");
                EditorGUILayout.PropertyField(_useIPreprocessBuildWithReport);
                EditorGUILayout.PropertyField(_throwOnTypeGenerationError);
                EditorGUILayout.PropertyField(_preprocessBuildCallbackOrder);
                DrawGenerateAotTypesButton();
                EditorGUILayout.Space();
            }
            
            if (Foldout["Documentation & Links"])
            {
                EditorGUILayout.Space();
                InspectorUtilities.DrawWeblinksWithLabel();
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                Foldout.SaveState();
            }
        }

        private void DrawUIController()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_autoInstantiateUI);
            var isNotNull = IsRefFieldValueNull(_monitoringUIController);
            DrawSelectableUIController();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(80), GUILayout.Height(25)))
            {
                Refresh();
            }
            EditorGUILayout.EndHorizontal();

            if (isNotNull)
            {
                if (Application.isPlaying)
                {
                    var targetObject = MonitoringUI.GetActiveUIController();
                    DrawInlinedUIControllerPrefab(targetObject);
                }
                else
                {
                    var targetObject = _monitoringUIController.objectReferenceValue;
                    DrawInlinedUIControllerPrefab(targetObject);
                }
            }
            EditorGUILayout.Space();
        }

        private void DrawEnableMonitoring()
        {
#if DISABLE_MONITORING
                EditorGUILayout.HelpBox("The symbol 'DISABLE_MONITORING' is active!", MessageType.Info);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_enableMonitoring);
                GUI.enabled = true;
#else
            EditorGUILayout.PropertyField(_enableMonitoring);
#endif
        }
        
             
        private void DrawAsyncProfiling()
        {
#if UNITY_WEBGL
            GUI.enabled = false;
            EditorGUILayout.Toggle(new GUIContent(_asyncProfiling.displayName, _asyncProfiling.tooltip), false);
            GUI.enabled = true;
#else
            EditorGUILayout.PropertyField(_asyncProfiling);
#endif
        }
        
        
        public override void OnInspectorGUI()
        {
            DrawCustomInspector();
            InspectorUtilities.DrawLine(false);
            InspectorUtilities.DrawCopyrightNotice();
        }
        
        private static void DrawGenerateAotTypesButton()
        {
            EditorGUILayout.Space();
            InspectorUtilities.DrawLine();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate Code", GUILayout.Height(25), GUILayout.MinWidth(150)))
            {
                IL2CPPBuildPreprocessor.GenerateIL2CPPAheadOfTimeTypes();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        #endregion

        #region --- Misc ---

        private static bool IsRefFieldValueNull(SerializedProperty property)
        {
            var isNull = property.objectReferenceValue == null;
            if (isNull)
            {
                EditorGUILayout.HelpBox($"{property.displayName} is Required!", MessageType.Error);
            }
            EditorGUILayout.PropertyField(property);
            return !isNull;
        }
        
        
        private void DrawInlinedUIControllerPrefab(Object targetObject)
        {
            InspectorUtilities.DrawLine();
            EditorGUILayout.Space();
            var editor = CreateEditor(targetObject);
            if (editor == null)
            {
                return;
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.Space();
            editor.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(targetObject);
            }
        }

        #endregion

        #region --- UI Controller Selection ---

        private void DrawSelectableUIController()
        {
            try
            {
                if (!_availableUIController.Any())
                {
                    return;
                }
                GUILayout.BeginVertical(GUI.skin.box);
                InspectorUtilities.DrawLine(false);
                for (var i = 0; i < _availableUIController.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    var isSelected = _monitoringUIController.objectReferenceValue == _availableUIController[i];
                    GUILayout.Label(BoldIf(_availableUIController[i].name, isSelected), InspectorUtilities.RichTextStyle(), GUILayout.Width(EditorGUIUtility.labelWidth - 7f));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Set Active UIController", GUILayout.Width(200)))
                    {
                        _monitoringUIController.objectReferenceValue = _availableUIController[i];
                        _monitoringUIController.serializedObject.ApplyModifiedProperties();
                    }
                    if (GUILayout.Button("Select", GUILayout.Width(75)))
                    {
                        Selection.activeObject = _availableUIController[i];
                        EditorGUIUtility.PingObject(_availableUIController[i]);
                    }
                    GUILayout.EndHorizontal();
                }
                InspectorUtilities.DrawLine(false);
                GUILayout.EndVertical();

                string BoldIf(string content, bool condition)
                {
                    return condition ? $"<b>{content}</b>" : content;
                }
            }
            catch (MissingReferenceException)
            {
                PopulateSerializedProperties();
            }
        }
        
        private readonly List<MonitoringUIController> _availableUIController = new List<MonitoringUIController>(10);
        
        private void PopulateAvailableUIController()
        {
            _availableUIController.Clear();
            var guids = AssetDatabase.FindAssets("t:Prefab");
            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var controller = gameObject.GetComponent<MonitoringUIController>();
                if (controller != null)
                {
                    _availableUIController.Add(controller);
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Package GUI ---

        private readonly GUIContent _showPackage =
            new GUIContent("Show Package", "Open and select the package in the project window");

        private readonly GUIContent _importPackage = 
            new GUIContent("Import Package", "Import the package into your project. This will open an import dialogue.");

        private void DrawAdditionalPackages()
        {
            EditorGUILayout.Space();
            const int HORIZONTAL_THRESHOLD = 700;
            var currentViewWidth = EditorGUIUtility.currentViewWidth -20;
            var makeHorizontal = currentViewWidth > HORIZONTAL_THRESHOLD;
            var verticalWidth = makeHorizontal? currentViewWidth / 3 : currentViewWidth;
            
            if (makeHorizontal)
            {
                GUILayout.BeginHorizontal();
            }
            
            // IMGUI
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(verticalWidth));
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
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(verticalWidth));
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
            GUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(verticalWidth));
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
            
            if (makeHorizontal)
            {
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
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
