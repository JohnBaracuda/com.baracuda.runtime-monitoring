using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Baracuda.PreprocessorDefinitionFiles.Utilities
{
    internal static class GUIExtensions
    {
        #region --- [FILEDS] ---

        private static ReorderableList _sGlobalCustomList;
        private static ReorderableList _sVersionDefines;
        private static ReorderableList _sCompilerDefines;
        private static ReorderableList _sPlatformDefines;

        private static readonly GUIContent _copyA = new GUIContent("Copy Preset", "Copy To Clipboard");
        private static readonly GUIContent _copyB = new GUIContent("Copy", "Copy To Clipboard");

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DRAW SYMBOLS] ---
        
                
        /// <summary>
        /// Draw GUI elements displaying every active global symbol.
        /// </summary>
        internal static void DrawGlobalSymbols()
        {
            DrawGUISpace(40);
            DrawGUILine();

            EditorGUILayout.LabelField("Globally Defined Symbol", new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            }, GUILayout.Height(25));

            DrawGUIMessage("Note that lists might not contain every available define!");
            DrawGUISpace();

            _sGlobalCustomList.DoLayoutList();
            DrawGUIMessage("Only version defines are of the <b>current version</b> are listed. " +
                           "Older version defines with the <b>OR_NEWER suffix</b> are also viable!");
            _sVersionDefines.DoLayoutList();
            _sCompilerDefines.DoLayoutList();
            _sPlatformDefines.DoLayoutList();
        }
       
        private static Rect ButtonRectA(Rect rect) => new Rect(rect.width - 40, rect.y, 65, rect.height);
        private static Rect ButtonRectB(Rect rect) => new Rect(rect.width - 135, rect.y, 95, rect.height);

        private static void DrawElement(Rect rect, int index, ref string[] element)
        {
            EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, rect.width - 5, rect.height), element[index]);
            // --- 
            _copyA.tooltip = $"Copy the following to your clipboard:\n #if {element[index]} \n\n#endif";
            if (GUI.Button(ButtonRectB(rect), _copyA))
            {
                EditorGUIUtility.systemCopyBuffer = $"#if {element[index]}\n\n#endif";
            }

            // --- 
            _copyB.tooltip = $"Copy '{element[index]}' to clipboard";
            if (GUI.Button(ButtonRectA(rect), _copyB))
            {
                EditorGUIUtility.systemCopyBuffer = element[index];
            }
        }


        [InitializeOnLoadMethod]
        private static void InitializeGUI()
        {
            // --- CUSTOM SYMBOLS
            var symbols = PreprocessorDefineUtilities.GetCustomDefinesOfActiveTargetGroup().ToArray();
            _sGlobalCustomList = new ReorderableList(symbols, typeof(string), false, true, false, false);
            _sGlobalCustomList.drawElementCallback +=
                (rect, index, active, focused) => DrawElement(rect, index, ref symbols);
            _sGlobalCustomList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Custom Defines");

            // --- VERSION SYMBOLS
            var version = PreprocessorDefineUtilities.VersionDefines.ToArray();
            _sVersionDefines = new ReorderableList(version, typeof(string), false, true, false, false);
            _sVersionDefines.drawElementCallback +=
                (rect, index, active, focused) => DrawElement(rect, index, ref version);
            _sVersionDefines.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Version Defines");


            // --- COMPILER SYMBOLS
            var compiler = PreprocessorDefineUtilities.CompilerDefines.ToArray();
            ;
            _sCompilerDefines = new ReorderableList(compiler, typeof(string), false, true, false, false);
            _sCompilerDefines.drawElementCallback +=
                (rect, index, active, focused) => DrawElement(rect, index, ref compiler);
            _sCompilerDefines.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Compiler Defines");


            // --- PLATFORM SYMBOLS
            var platform = PreprocessorDefineUtilities.PlatformDefines.ToArray();
            ;
            _sPlatformDefines = new ReorderableList(platform, typeof(string), false, true, false, false);
            _sPlatformDefines.drawElementCallback +=
                (rect, index, active, focused) => DrawElement(rect, index, ref platform);
            _sPlatformDefines.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Platform Defines");
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [GENERIC GUI ELEMENTS] ---

        private static readonly Color _defaultLightColor = new Color(.8f, .8f, .9f, .5f);
        private static readonly Color _defaultDarkColor = new Color(.2f, .2f, .2f, .5f);

        /// <summary>
        /// Draw Line in Inspector
        /// </summary>
        internal static void DrawGUILine(Color? color = null, int thickness = 1, int padding = 1, bool space = false)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            rect.height = thickness;
            rect.y += (float) padding / 2;
            rect.x -= 2;
            rect.width += 4;
            EditorGUI.DrawRect(rect, color ?? (EditorGUIUtility.isProSkin ? _defaultLightColor : _defaultDarkColor));
        }

        internal static void DrawGUISpace(int space = 10) => GUILayout.Space(space);

        internal static void DrawGUIMessage(string message, int size = 12)
        {
            var style = GUI.skin.GetStyle("HelpBox");
            style.fontSize = size;
            style.richText = true;
            EditorGUILayout.TextArea(message, style);
        }

        internal static void DrawGUIMessage(Rect rect, string message, int size = 12)
        {
            var style = GUI.skin.GetStyle("HelpBox");
            style.fontSize = size;
            style.richText = true;
            GUI.TextArea(rect, message, style);
        }

        #endregion
    }
}