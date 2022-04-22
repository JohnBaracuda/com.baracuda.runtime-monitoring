using System;
using System.Reflection;
using Baracuda.Monitoring.Management;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringSettings))]
    public class MonitoringSettingsInspector : UnityEditor.Editor
    {
        private FoldoutHandler Foldout { get; set; }
        private MonitoringSettings _settings;

        #region --- Serialized Properties ---

        private SerializedProperty _enableMonitoring;
        private SerializedProperty _forceSynchronousLoad;
        
        private SerializedProperty _logBadImageFormatException;
        private SerializedProperty _logOperationCanceledException;
        private SerializedProperty _logThreadAbortException;
        private SerializedProperty _logUnknownExceptions;
        private SerializedProperty _logBackfieldNotFoundException;
        private SerializedProperty _logProcessorNotFoundException;
        private SerializedProperty _logInvalidProcessorSignatureException;
        
        private SerializedProperty _addClassName;
        private SerializedProperty _appendSymbol;
        private SerializedProperty _groupStaticUnits;
        private SerializedProperty _groupInstanceUnits;
        private SerializedProperty _humanizeNames;
        private SerializedProperty _variablePrefixes;
        
        private SerializedProperty _optionalStyleSheets;
        private SerializedProperty _instanceUnitStyles;
        private SerializedProperty _instanceGroupStyles;
        private SerializedProperty _instanceLabelStyles;
        private SerializedProperty _staticUnitStyles;
        private SerializedProperty _staticGroupStyles;
        private SerializedProperty _staticLabelStyles;
        
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
            _settings = MonitoringSettings.Instance();

            PopulateSerializedProperties();
        }

        private void OnDisable()
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUIUtility.labelWidth = 300;
            
            if (Foldout["General"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_enableMonitoring);
                EditorGUILayout.PropertyField(_forceSynchronousLoad);
                EditorGUILayout.Space();
            }
            
            if (Foldout["Debug"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_logBadImageFormatException);
                EditorGUILayout.PropertyField(_logOperationCanceledException);
                EditorGUILayout.PropertyField(_logThreadAbortException);
                EditorGUILayout.PropertyField(_logUnknownExceptions);
                EditorGUILayout.PropertyField(_logBackfieldNotFoundException);
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
                EditorGUILayout.Space();
            }
            
            if (Foldout["Style"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_optionalStyleSheets);
                EditorGUILayout.PropertyField(_instanceUnitStyles);
                EditorGUILayout.PropertyField(_instanceGroupStyles);
                EditorGUILayout.PropertyField(_instanceLabelStyles);
                EditorGUILayout.PropertyField(_staticUnitStyles);
                EditorGUILayout.PropertyField(_staticGroupStyles);
                EditorGUILayout.PropertyField(_staticLabelStyles);
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

        #region --- Misc ---

        private static void DrawGenerateAotTypesButton()
        {
            EditorGUILayout.Space();
            DrawLine();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate IL2CPP Ahead Of Time Types", GUILayout.Height(30), GUILayout.MinWidth(250)))
            {
                IL2CPPBuildPreprocessor.GenerateIL2CPPAheadOfTimeTypes();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
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
                    var newPath = EditorUtility.OpenFilePanel("Select File", string.IsNullOrWhiteSpace(path)? path : Application.dataPath, fileExtension);
                    path = !string.IsNullOrWhiteSpace(newPath) ? newPath : path;
                }
                GUILayout.EndHorizontal();

                property.stringValue = path;
            }
            else
            {
                throw new InvalidCastException("Property must be a string!");
            }
        }
        
        #endregion
    }
}
