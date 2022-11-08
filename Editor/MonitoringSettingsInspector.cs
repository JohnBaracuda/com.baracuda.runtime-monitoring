// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Systems;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringSettings))]
    internal class MonitoringSettingsInspector : UnityEditor.Editor
    {
        #region Data

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
        private SerializedProperty _openDisplayOnLoad;
        private SerializedProperty _asyncProfiling;
        private SerializedProperty _monitoringUIOverride;
        private SerializedProperty _allowMultipleUIInstances;

        private SerializedProperty _showRuntimeMonitoringObject;
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
        private SerializedProperty _enableRichText;

        private SerializedProperty _floatFormat;
        private SerializedProperty _integerFormat;
        private SerializedProperty _vectorFormat;
        private SerializedProperty _quaternionFormat;

        private SerializedProperty _filterLabel;
        private SerializedProperty _filterStaticOrInstance;
        private SerializedProperty _filterType;
        private SerializedProperty _filterDeclaringType;
        private SerializedProperty _filterMemberType;
        private SerializedProperty _filterTags;
        private SerializedProperty _filterInterfaces;

        private SerializedProperty _filterComparison;
        private SerializedProperty _filterAppendSymbol;
        private SerializedProperty _filterNegateSymbol;
        private SerializedProperty _filterAbsoluteSymbol;
        private SerializedProperty _filterTagsSymbol;

        private SerializedProperty _trueColor;
        private SerializedProperty _falseColor;
        private SerializedProperty _xColor;
        private SerializedProperty _yColor;
        private SerializedProperty _zColor;
        private SerializedProperty _wColor;
        private SerializedProperty _classColor;
        private SerializedProperty _eventColor;
        private SerializedProperty _sceneNameColor;
        private SerializedProperty _targetObjectColor;
        private SerializedProperty _methodColor;
        private SerializedProperty _outParameterColor;

        private SerializedProperty _bannedAssemblyPrefixes;
        private SerializedProperty _bannedAssemblyNames;

        private SerializedProperty _typeDefinitionsForIL2CPP;
        private SerializedProperty _generateTypeDefinitionsOnBuild;
        private SerializedProperty _preprocessBuildCallbackOrder;

        #endregion

        #region Setup

        private void OnEnable()
        {
            Foldout = new FoldoutHandler(nameof(MonitoringSettings));
            PopulateSerializedProperties();
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
            var parsedMemberName = (!string.IsNullOrWhiteSpace(member) && member[0] == '_') ? member.Remove(0, 1) : member;
            return serializedObject.FindProperty(parsedMemberName);
        }

        #endregion

        #region GUI

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
                EditorGUILayout.PropertyField(_enableMonitoring);
                EditorGUILayout.PropertyField(_asyncProfiling, new GUIContent("Multi Thread Profiling", _asyncProfiling.tooltip));
                EditorGUILayout.Space();
            }

            if (Foldout["UI Display"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_openDisplayOnLoad, new GUIContent("Visible On Load", _openDisplayOnLoad.tooltip));
                EditorGUILayout.PropertyField(_allowMultipleUIInstances);
                EditorGUILayout.PropertyField(_monitoringUIOverride);

                if (_monitoringUIOverride.objectReferenceValue != null)
                {
                    if (Application.isPlaying && Monitor.UI.GetCurrent<MonitoringUI>() is Object targetObject)
                    {
                        DrawInlinedUIControllerPrefab(targetObject);
                    }
                    else
                    {
                        targetObject = _monitoringUIOverride.objectReferenceValue;
                        DrawInlinedUIControllerPrefab(targetObject);
                    }
                }
                EditorGUILayout.Space();
            }

            if (Foldout["Filtering"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_filterLabel);
                EditorGUILayout.PropertyField(_filterStaticOrInstance);
                EditorGUILayout.PropertyField(_filterType);
                EditorGUILayout.PropertyField(_filterDeclaringType);
                EditorGUILayout.PropertyField(_filterMemberType);
                EditorGUILayout.PropertyField(_filterTags);
                EditorGUILayout.PropertyField(_filterInterfaces);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_filterComparison);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_filterAppendSymbol);
                EditorGUILayout.PropertyField(_filterNegateSymbol);
                EditorGUILayout.PropertyField(_filterAbsoluteSymbol);
                EditorGUILayout.PropertyField(_filterTagsSymbol);
                EditorGUILayout.Space();
            }

            if (Foldout["Format & Style"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_addClassName);
                EditorGUILayout.PropertyField(_appendSymbol);
                EditorGUILayout.PropertyField(_humanizeNames);
                EditorGUILayout.PropertyField(_enableRichText);
                EditorGUILayout.PropertyField(_variablePrefixes);
                EditorGUILayout.PropertyField(_trueColor);
                EditorGUILayout.PropertyField(_falseColor);
                EditorGUILayout.PropertyField(_xColor);
                EditorGUILayout.PropertyField(_yColor);
                EditorGUILayout.PropertyField(_zColor);
                EditorGUILayout.PropertyField(_wColor);
                EditorGUILayout.PropertyField(_classColor);
                EditorGUILayout.PropertyField(_eventColor);
                EditorGUILayout.PropertyField(_sceneNameColor);
                EditorGUILayout.PropertyField(_targetObjectColor);
                EditorGUILayout.PropertyField(_methodColor);
                EditorGUILayout.PropertyField(_outParameterColor);
                EditorGUILayout.Space();
            }

            if (Foldout["Debug"])
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_showRuntimeMonitoringObject);
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
                EditorGUILayout.PropertyField(_generateTypeDefinitionsOnBuild);
                if (_generateTypeDefinitionsOnBuild.boolValue)
                {
                    if (_typeDefinitionsForIL2CPP.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("If you don't select a TextAsset as your IL2CPP type definition script file, a new .cs file is generated at\n" +
                                                "Assets/Baracuda/IL2CPP when an IL2CPP build is triggered. This might change in future releases so be aware of old files.", MessageType.Warning);
                    }
                    EditorGUILayout.PropertyField(_typeDefinitionsForIL2CPP);
                    EditorGUILayout.PropertyField(_preprocessBuildCallbackOrder);
                    DrawGenerateAotTypesButton();
                }
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
#if UNITY_EDITOR
                IL2CPPBuildPreprocessor.GenerateIL2CPPAheadOfTimeTypes();
#endif
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        #endregion

        #region Misc

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
    }
}
