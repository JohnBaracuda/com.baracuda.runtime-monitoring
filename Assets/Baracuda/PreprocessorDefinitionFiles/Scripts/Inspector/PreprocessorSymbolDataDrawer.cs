using Baracuda.PreprocessorDefinitionFiles.Utilities;
using UnityEditor;
using UnityEngine;

namespace Baracuda.PreprocessorDefinitionFiles.Inspector
{
    /// <summary>
    /// Custom property drawer class for PreprocessorSymbolData objects.
    /// </summary>
    [CustomPropertyDrawer(typeof(PreprocessorSymbolData))]
    public sealed class PreprocessorSymbolDataDrawer : PropertyDrawer
    {

        #region --- [FIELDS] ---

        private static readonly Color _inactiveColor = new Color(0.53f, 0.53f, 0.53f);
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [ON GUI] ---

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            // Calculate rects
            var enabledRect     = new Rect(position.x, position.y, 20, position.height);
            var appliedRect     = new Rect(position.x + 18, position.y, 18, position.height);
            var textRect        = new Rect(position.x + 30, position.y, 290, position.height);
            var targetLabelRect = new Rect(position.x + 330, position.y, 45, position.height);
            var targetRect      = new Rect(position.x + 375, position.y, position.width - 375, position.height);

            var targetGroup = (FlagsBuildTargetGroup)property.FindPropertyRelative(PreprocessorSymbolData.NAMEOF_TARGET).intValue;
            var color = GUI.contentColor;

            var targetFile = property.serializedObject.targetObject as PreprocessorSymbolDefinitionFile;
            var enabled = property.FindPropertyRelative(PreprocessorSymbolData.NAMEOF_ENABLED).boolValue;
            var symbol = property.FindPropertyRelative(PreprocessorSymbolData.NAMEOF_SYMBOL).stringValue;
            var activeAndEnabled = enabled && targetGroup.HasFlag(PreprocessorDefineUtilities.FlagsBuildTargetCache);
            var isDefined = PreprocessorDefineUtilities.IsSymbolDefined(symbol);
            
            GUI.contentColor = activeAndEnabled ? color : _inactiveColor;

            // Draw fields - pass GUIContent.none to each so they are drawn without labels.
            // Draw empty label fields to display tooltips. 
            
            // Draw enabled field
            EditorGUI.PropertyField(enabledRect, property.FindPropertyRelative(PreprocessorSymbolData.NAMEOF_ENABLED), GUIContent.none);
            EditorGUI.LabelField(enabledRect, new GUIContent("", Extensions.GetTooltip<PreprocessorSymbolData>(PreprocessorSymbolData.NAMEOF_ENABLED)));
            
            // Draw symbol * to indicate that changes need to be applied
            DrawChangesIndicationGUI(targetGroup, isDefined, activeAndEnabled, symbol, appliedRect, color);

            // Draw symbol field
            DrawSymbolGUI(property, textRect);

            // Draw build target field
            DrawBuildTargetGUI(property, color, targetLabelRect, targetRect);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [INDIVIDUAL GUI ELEMENTS] ---
        
        private static void DrawSymbolGUI(SerializedProperty property, Rect textRect)
        {
            EditorGUI.PropertyField(textRect, property.FindPropertyRelative(PreprocessorSymbolData.NAMEOF_SYMBOL),
                GUIContent.none);
            EditorGUI.LabelField(textRect,
                new GUIContent("", Extensions.GetTooltip<PreprocessorSymbolData>(PreprocessorSymbolData.NAMEOF_SYMBOL)));
        }

        //------

        private void DrawBuildTargetGUI(SerializedProperty property, Color color, Rect targetLabelRect, Rect targetRect)
        {
            GUI.contentColor = color;
            EditorGUI.LabelField(targetLabelRect,
                new GUIContent("Target", Extensions.GetTooltip<PreprocessorSymbolData>(PreprocessorSymbolData.NAMEOF_TARGET)));
            EditorGUI.PropertyField(targetRect, property.FindPropertyRelative(PreprocessorSymbolData.NAMEOF_TARGET),
                GUIContent.none);
            EditorGUI.LabelField(targetRect,
                new GUIContent("", Extensions.GetTooltip<PreprocessorSymbolData>(PreprocessorSymbolData.NAMEOF_TARGET)));
        }
        
        //------
        
        /// <summary>
        /// Draws the GUI element that displays if unsaved changes are present. 
        /// </summary>
        private void DrawChangesIndicationGUI(FlagsBuildTargetGroup targetGroup, bool isDefined, bool activeAndEnabled, string symbol, Rect appliedRect, Color color)
        {
            if (isDefined && !activeAndEnabled && targetGroup.HasFlag(PreprocessorDefineUtilities.FlagsBuildTargetCache))
            {
                GUI.contentColor = color;
                EditorGUI.LabelField(appliedRect, new GUIContent("*", $"Changes must be applied!"));
                GUI.contentColor = _inactiveColor;
            }
            else if (!isDefined && activeAndEnabled && targetGroup.HasFlag(PreprocessorDefineUtilities.FlagsBuildTargetCache))
            {
                EditorGUI.LabelField(appliedRect, new GUIContent("*", $"{symbol} is not Defined! Apply to define the symbol"));
            }
            else
            {
                EditorGUI.LabelField(appliedRect, GUIContent.none);
            }
        }

        #endregion
    }
}