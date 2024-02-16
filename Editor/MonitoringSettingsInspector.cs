// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Systems;
using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Editor
{
    [UnityEditor.CustomEditor(typeof(MonitoringSettings))]
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
        private UnityEditor.SerializedProperty _enableMonitoring;
        private UnityEditor.SerializedProperty _updateRate;
        private UnityEditor.SerializedProperty _openDisplayOnLoad;
        private UnityEditor.SerializedProperty _asyncProfiling;
        private UnityEditor.SerializedProperty _updatesWithLowTimeScale;
        private UnityEditor.SerializedProperty _monitoringUIOverride;
        private UnityEditor.SerializedProperty _allowMultipleUIInstances;

        private UnityEditor.SerializedProperty _showRuntimeMonitoringObject;
        private UnityEditor.SerializedProperty _logBadImageFormatException;
        private UnityEditor.SerializedProperty _logOperationCanceledException;
        private UnityEditor.SerializedProperty _logThreadAbortException;
        private UnityEditor.SerializedProperty _logUnknownExceptions;
        private UnityEditor.SerializedProperty _logProcessorNotFoundException;
        private UnityEditor.SerializedProperty _logInvalidProcessorSignatureException;

        private UnityEditor.SerializedProperty _addClassName;
        private UnityEditor.SerializedProperty _appendSymbol;
        private UnityEditor.SerializedProperty _humanizeNames;
        private UnityEditor.SerializedProperty _variablePrefixes;
        private UnityEditor.SerializedProperty _enableRichText;

        private UnityEditor.SerializedProperty _floatFormat;
        private UnityEditor.SerializedProperty _integerFormat;
        private UnityEditor.SerializedProperty _vectorFormat;
        private UnityEditor.SerializedProperty _quaternionFormat;

        private UnityEditor.SerializedProperty _filterLabel;
        private UnityEditor.SerializedProperty _filterStaticOrInstance;
        private UnityEditor.SerializedProperty _filterType;
        private UnityEditor.SerializedProperty _filterDeclaringType;
        private UnityEditor.SerializedProperty _filterMemberType;
        private UnityEditor.SerializedProperty _filterTags;
        private UnityEditor.SerializedProperty _filterInterfaces;

        private UnityEditor.SerializedProperty _filterComparison;
        private UnityEditor.SerializedProperty _filterAppendSymbol;
        private UnityEditor.SerializedProperty _filterNegateSymbol;
        private UnityEditor.SerializedProperty _filterAbsoluteSymbol;
        private UnityEditor.SerializedProperty _filterTagsSymbol;

        private UnityEditor.SerializedProperty _trueColor;
        private UnityEditor.SerializedProperty _falseColor;
        private UnityEditor.SerializedProperty _xColor;
        private UnityEditor.SerializedProperty _yColor;
        private UnityEditor.SerializedProperty _zColor;
        private UnityEditor.SerializedProperty _wColor;
        private UnityEditor.SerializedProperty _classColor;
        private UnityEditor.SerializedProperty _eventColor;
        private UnityEditor.SerializedProperty _sceneNameColor;
        private UnityEditor.SerializedProperty _targetObjectColor;
        private UnityEditor.SerializedProperty _methodColor;
        private UnityEditor.SerializedProperty _outParameterColor;

        private UnityEditor.SerializedProperty _bannedAssemblyPrefixes;
        private UnityEditor.SerializedProperty _bannedAssemblyNames;

        private UnityEditor.SerializedProperty _typeDefinitionsForIL2CPP;
        private UnityEditor.SerializedProperty _generateTypeDefinitionsOnBuild;
        private UnityEditor.SerializedProperty _preprocessBuildCallbackOrder;

        #endregion


        #region Setup

        private void OnEnable()
        {
            if (!IsValid())
            {
                return;
            }
            Foldout = new FoldoutHandler(nameof(MonitoringSettings));
            PopulateSerializedProperties();
        }

        protected void PopulateSerializedProperties()
        {
            const BindingFlags Flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            var fields = GetType().GetFields(Flags);

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.FieldType == typeof(UnityEditor.SerializedProperty))
                {
                    fieldInfo.SetValue(this, FindProperty(fieldInfo.Name));
                }
            }
        }

        private UnityEditor.SerializedProperty FindProperty(string member)
        {
            var parsedMemberName =
                !string.IsNullOrWhiteSpace(member) && member[0] == '_' ? member.Remove(0, 1) : member;
            return serializedObject.FindProperty(parsedMemberName);
        }

        #endregion


        #region GUI

        public void DrawCustomInspector()
        {
            if (!IsValid())
            {
                return;
            }
            serializedObject.Update();
            UnityEditor.EditorGUIUtility.labelWidth = 300;

            if (Application.isPlaying)
            {
                UnityEditor.EditorGUILayout.HelpBox("Some Settings may not update during runtime!",
                    UnityEditor.MessageType.Info);
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["General"])
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_enableMonitoring);
                UnityEditor.EditorGUILayout.PropertyField(_updateRate);
                UnityEditor.EditorGUILayout.PropertyField(_asyncProfiling,
                    new GUIContent("Multi Thread Profiling", _asyncProfiling.tooltip));
                UnityEditor.EditorGUILayout.PropertyField(_updatesWithLowTimeScale);
                UnityEditor.EditorGUILayout.Space();

                UnityEditor.EditorGUILayout.PropertyField(_openDisplayOnLoad,
                    new GUIContent("Visible On Load", _openDisplayOnLoad.tooltip));
                UnityEditor.EditorGUILayout.PropertyField(_allowMultipleUIInstances);
                UnityEditor.EditorGUILayout.PropertyField(_monitoringUIOverride);

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
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["Filtering"])
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_filterLabel);
                UnityEditor.EditorGUILayout.PropertyField(_filterStaticOrInstance);
                UnityEditor.EditorGUILayout.PropertyField(_filterType);
                UnityEditor.EditorGUILayout.PropertyField(_filterDeclaringType);
                UnityEditor.EditorGUILayout.PropertyField(_filterMemberType);
                UnityEditor.EditorGUILayout.PropertyField(_filterTags);
                UnityEditor.EditorGUILayout.PropertyField(_filterInterfaces);
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_filterComparison);
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_filterAppendSymbol);
                UnityEditor.EditorGUILayout.PropertyField(_filterNegateSymbol);
                UnityEditor.EditorGUILayout.PropertyField(_filterAbsoluteSymbol);
                UnityEditor.EditorGUILayout.PropertyField(_filterTagsSymbol);
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["Formatting"])
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_addClassName);
                UnityEditor.EditorGUILayout.PropertyField(_appendSymbol);
                UnityEditor.EditorGUILayout.PropertyField(_humanizeNames);
                UnityEditor.EditorGUILayout.PropertyField(_enableRichText);
                UnityEditor.EditorGUILayout.PropertyField(_variablePrefixes);
                UnityEditor.EditorGUILayout.PropertyField(_trueColor);
                UnityEditor.EditorGUILayout.PropertyField(_falseColor);
                UnityEditor.EditorGUILayout.PropertyField(_xColor);
                UnityEditor.EditorGUILayout.PropertyField(_yColor);
                UnityEditor.EditorGUILayout.PropertyField(_zColor);
                UnityEditor.EditorGUILayout.PropertyField(_wColor);
                UnityEditor.EditorGUILayout.PropertyField(_classColor);
                UnityEditor.EditorGUILayout.PropertyField(_eventColor);
                UnityEditor.EditorGUILayout.PropertyField(_sceneNameColor);
                UnityEditor.EditorGUILayout.PropertyField(_targetObjectColor);
                UnityEditor.EditorGUILayout.PropertyField(_methodColor);
                UnityEditor.EditorGUILayout.PropertyField(_outParameterColor);
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["Debug"])
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_showRuntimeMonitoringObject);
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_logBadImageFormatException);
                UnityEditor.EditorGUILayout.PropertyField(_logOperationCanceledException);
                UnityEditor.EditorGUILayout.PropertyField(_logThreadAbortException);
                UnityEditor.EditorGUILayout.PropertyField(_logUnknownExceptions);
                UnityEditor.EditorGUILayout.PropertyField(_logProcessorNotFoundException);
                UnityEditor.EditorGUILayout.PropertyField(_logInvalidProcessorSignatureException);
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["Assembly Settings"])
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_bannedAssemblyPrefixes);
                UnityEditor.EditorGUILayout.PropertyField(_bannedAssemblyNames);
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["IL2CPP Settings"])
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(_generateTypeDefinitionsOnBuild);
                if (_generateTypeDefinitionsOnBuild.boolValue)
                {
                    if (_typeDefinitionsForIL2CPP.objectReferenceValue == null)
                    {
                        UnityEditor.EditorGUILayout.HelpBox(
                            "If you don't select a TextAsset as your IL2CPP type definition script file, a new .cs file is generated at\n" +
                            "Assets/Baracuda/IL2CPP when an IL2CPP build is triggered. This might change in future releases so be aware of old files.",
                            UnityEditor.MessageType.Warning);
                    }
                    UnityEditor.EditorGUILayout.PropertyField(_typeDefinitionsForIL2CPP);
                    UnityEditor.EditorGUILayout.PropertyField(_preprocessBuildCallbackOrder);
                    DrawGenerateAotTypesButton();
                }
                UnityEditor.EditorGUILayout.Space();
            }

            if (Foldout["Documentation & Links"])
            {
                UnityEditor.EditorGUILayout.Space();
                InspectorUtilities.DrawWeblinksWithLabel();
                UnityEditor.EditorGUILayout.Space();
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
            UnityEditor.EditorGUILayout.Space();
            InspectorUtilities.DrawLine();
            UnityEditor.EditorGUILayout.Space();
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
            UnityEditor.EditorGUILayout.Space();
        }

        #endregion


        #region Helper

        private static void DrawInlinedUIControllerPrefab(Object targetObject)
        {
            InspectorUtilities.DrawLine();
            UnityEditor.EditorGUILayout.Space();
            var editor = CreateEditor(targetObject);
            if (editor == null)
            {
                return;
            }
            UnityEditor.EditorGUI.BeginChangeCheck();
            UnityEditor.EditorGUILayout.BeginVertical("helpbox");
            UnityEditor.EditorGUILayout.Space();
            editor.OnInspectorGUI();
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.EndVertical();
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                UnityEditor.EditorUtility.SetDirty(targetObject);
            }
        }

        private bool IsValid()
        {
            try
            {
                return serializedObject.targetObject != null && serializedObject.targetObjects.Length > 0 &&
                       serializedObject.targetObjects[0] != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}