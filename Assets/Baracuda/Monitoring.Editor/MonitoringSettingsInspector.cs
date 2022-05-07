using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEditor;
using UnityEngine;

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
         * Constant Web Links   
         */

        private const string LINK_DOCUMENTATION = "https://johnbaracuda.com/monitoring.html";
        private const string LINK_REPOSITORY = "https://github.com/johnbaracuda/Runtime-Monitoring";
        private const string LINK_WEBSITE = "https://johnbaracuda.com/";

        /*
         * Serialized Properties   
         */

        private SerializedProperty _enableMonitoring;
        private SerializedProperty _openDisplayOnLoad;
        private SerializedProperty _forceSynchronousLoad;
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
        
        private SerializedProperty _classColor;
        private SerializedProperty _trueColor;
        private SerializedProperty _falseColor;
        private SerializedProperty _xColor;
        private SerializedProperty _yColor;
        private SerializedProperty _zColor;
        private SerializedProperty _wColor;
        
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
        }

        private void OnDisable()
        {
            SaveState();
        }

        public void SaveState()
        {
            Foldout.SaveState();
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
#if DISABLE_MONITORING
                EditorGUILayout.HelpBox("The symbol 'DISABLE_MONITORING' is active!", MessageType.Info);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_enableMonitoring);
                GUI.enabled = true;
#else
                EditorGUILayout.PropertyField(_enableMonitoring);
                #endif
                EditorGUILayout.PropertyField(_openDisplayOnLoad);
                EditorGUILayout.Space();
            }

            if (Foldout["UI Controller"])
            {
                EditorGUILayout.Space();
                if (DrawUIControllerRefField(_monitoringUIController))
                {
                    DrawInlinedUIController();
                }
                EditorGUILayout.Space();
            }
            
            if (Foldout["Formatting"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_addClassName);
                EditorGUILayout.PropertyField(_appendSymbol);
                EditorGUILayout.PropertyField(_humanizeNames);
                EditorGUILayout.PropertyField(_variablePrefixes);
                InspectorUtilities.DrawLine();
                EditorGUILayout.PropertyField(_floatFormat);
                EditorGUILayout.PropertyField(_integerFormat);
                EditorGUILayout.PropertyField(_vectorFormat);
                EditorGUILayout.PropertyField(_quaternionFormat);
                EditorGUILayout.Space();
            }
          
            if (Foldout["Color"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_classColor);
                EditorGUILayout.PropertyField(_trueColor);
                EditorGUILayout.PropertyField(_falseColor);
                EditorGUILayout.PropertyField(_xColor);
                EditorGUILayout.PropertyField(_yColor);
                EditorGUILayout.PropertyField(_zColor);
                EditorGUILayout.PropertyField(_wColor);
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
                DrawWeblinks();
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        public override void OnInspectorGUI()
        {
            DrawCustomInspector();
            InspectorUtilities.DrawLine(false);
            InspectorUtilities.DrawCopyrightNotice();
        }

        private static void DrawWeblinks()
        {
            // Documentation
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Documentation", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(LINK_DOCUMENTATION))
            {
                Application.OpenURL(LINK_DOCUMENTATION);
            }
            GUILayout.EndHorizontal();
            
            // Repository
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Repository", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(LINK_REPOSITORY))
            {
                Application.OpenURL(LINK_REPOSITORY);
            }
            GUILayout.EndHorizontal();
            
            // Website
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Website", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(LINK_WEBSITE))
            {
                Application.OpenURL(LINK_WEBSITE);
            }
            GUILayout.EndHorizontal();
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

        private static bool DrawUIControllerRefField(SerializedProperty property)
        {
            var isNull = property.objectReferenceValue == null;
            if (isNull)
            {
                EditorGUILayout.HelpBox($"{property.displayName} is Required!", MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property);
            // if (GUILayout.Button("Select", GUILayout.Width(60)))
            // {
            //     // Draw custom select menu...
            // }
            EditorGUILayout.EndHorizontal();
            return !isNull;
        }
        
        
        private void DrawInlinedUIController()
        {
            EditorGUILayout.Space();
            InspectorUtilities.DrawLine();
            EditorGUILayout.Space();
            var targetObject = _monitoringUIController.objectReferenceValue;
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
    }
}
