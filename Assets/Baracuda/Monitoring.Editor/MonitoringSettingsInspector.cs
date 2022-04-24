using System;
using System.IO;
using System.Reflection;
using Baracuda.Monitoring.Management;
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
        private const string LINK_REPOSITORY = "https://github.com/JohnBaracuda/Member-State-Monitoring";
        private const string LINK_WEBSITE = "https://johnbaracuda.com/";

        /*
         * Serialized Properties   
         */

        private SerializedProperty _enableMonitoring;
        private SerializedProperty _openDisplayOnLoad;
        private SerializedProperty _forceSynchronousLoad;
        private SerializedProperty _monitoringDisplay;
        private SerializedProperty _showRuntimeObject;
        
        private SerializedProperty _logBadImageFormatException;
        private SerializedProperty _logOperationCanceledException;
        private SerializedProperty _logThreadAbortException;
        private SerializedProperty _logUnknownExceptions;
        private SerializedProperty _logProcessorNotFoundException;
        private SerializedProperty _logInvalidProcessorSignatureException;
        
        private SerializedProperty _addClassName;
        private SerializedProperty _appendSymbol;
        private SerializedProperty _groupStaticUnits;
        private SerializedProperty _groupInstanceUnits;
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
            var parsedMemberName = member.StartsWith('_') ? member.Remove(0, 1) : member;
            return serializedObject.FindProperty(parsedMemberName);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- GUI ---
        
        public override void OnInspectorGUI()
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
                EditorGUILayout.PropertyField(_enableMonitoring);
                EditorGUILayout.PropertyField(_openDisplayOnLoad);
                DrawRequired(_monitoringDisplay);
                EditorGUILayout.PropertyField(_showRuntimeObject);
                DrawButtonControls();
                EditorGUILayout.Space();
            }
            
            if (Foldout["Debug"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_logBadImageFormatException);
                EditorGUILayout.PropertyField(_logOperationCanceledException);
                EditorGUILayout.PropertyField(_logThreadAbortException);
                EditorGUILayout.PropertyField(_logUnknownExceptions);
                EditorGUILayout.PropertyField(_logProcessorNotFoundException);
                EditorGUILayout.PropertyField(_logInvalidProcessorSignatureException);
                EditorGUILayout.Space();
            }
            
            if (Foldout["Formatting"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_addClassName);
                EditorGUILayout.PropertyField(_appendSymbol);
                EditorGUILayout.PropertyField(_groupStaticUnits);
                EditorGUILayout.PropertyField(_groupInstanceUnits);
                EditorGUILayout.PropertyField(_humanizeNames);
                EditorGUILayout.PropertyField(_variablePrefixes);
                
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
                DrawFilePath(_filePathIL2CPPTypes, "cs");
                EditorGUILayout.PropertyField(_useIPreprocessBuildWithReport);
                EditorGUILayout.PropertyField(_throwOnTypeGenerationError);
                EditorGUILayout.PropertyField(_preprocessBuildCallbackOrder);
                DrawGenerateAotTypesButton();
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        
        private static void DrawButtonControls()
        {
            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Documentation", GUILayout.Height(25), GUILayout.MinWidth(150)))
            {
                Application.OpenURL(LINK_DOCUMENTATION);
            }
            if (GUILayout.Button("Repository", GUILayout.Height(25), GUILayout.MinWidth(150)))
            {
                Application.OpenURL(LINK_REPOSITORY);
            }
            if (GUILayout.Button("Website", GUILayout.Height(25), GUILayout.MinWidth(150)))
            {
                Application.OpenURL(LINK_WEBSITE);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private static void DrawGenerateAotTypesButton()
        {
            EditorGUILayout.Space();
            DrawLine();
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

        private static void DrawRequired(SerializedProperty property)
        {
            if (property.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox($"{property.displayName} is Required!", MessageType.Error);
            }
            EditorGUILayout.PropertyField(property);
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
        
        private static void DrawFilePath(SerializedProperty property, string fileExtension)
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
            return Path.IsPathFullyQualified(path) && path.StartsWith(Application.dataPath);
        }
        
        #endregion
    }
}
