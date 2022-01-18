using System.Collections.Generic;
using System.Linq;
using Baracuda.PreprocessorDefinitionFiles.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Baracuda.PreprocessorDefinitionFiles
{
    /// <summary>
    /// Settings file managing and elevated symbols and options regarding definition files.
    /// </summary>
    public sealed class PreprocessorSymbolDefinitionSettings : ScriptableObject
    {
        #region --- [SERIALIZE] ---

        [SerializeField] [HideInInspector] private bool removeSymbolsOnDelete = true;
        
        [SerializeField] [HideInInspector] private List<string> elevatedSymbols = new List<string>();
        [SerializeField] [HideInInspector] private List<PreprocessorSymbolDefinitionFile> scriptDefineSymbolFiles = null;
#if UNITY_2020_2_OR_NEWER
        [SerializeField] [HideInInspector] private bool saveOnCompile = true;
#endif
        [SerializeField] [HideInInspector] private bool logMessages = true;

        [SerializeField] [HideInInspector] private bool showAllDefinedSymbols = true;
        #endregion
        
        //---------

        #region --- [PROPERTIES] ---

        internal const string NAME_ELEVATED_SYMBOLS = nameof(elevatedSymbols);
        internal const string NAME_SDS_FILES = nameof(scriptDefineSymbolFiles);

        /// <summary>
        /// Get a list of currently elevated symbols.
        /// </summary>
        public static List<string> ElevatedSymbols => Instance.elevatedSymbols;
        
        /// <summary>
        /// Removes the content of a Preprocessor Symbol Definition File when it is deleted.
        /// If this option is not enabled the symbols of a deleted file will be elevated and must be removed manually
        /// </summary>
        public static bool RemoveSymbolsOnDelete
        {
            get => Instance.removeSymbolsOnDelete;
            set => Instance.removeSymbolsOnDelete = value;
        } 
        
        /// <summary>
        /// When enabled, lists of all defined symbols will be displayed in the inspector of the settings file as well as
        /// the inspector of Preprocessor Symbol Definition Files
        /// </summary>
        public static bool ShowAllDefinedSymbols
        {
            get => Instance.showAllDefinedSymbols;
            set => Instance.showAllDefinedSymbols = value;
        } 
        
#if UNITY_2020_2_OR_NEWER
        /// <summary>
        /// When enabled, unsaved changes will be applied when scripts are recompiling.
        /// </summary>
        public static bool SaveOnCompile
        {
            get => Instance.saveOnCompile;
            set => Instance.saveOnCompile = value;
        }
#endif

        /// <summary>
        /// When enabled, messages will be logged when symbols are removed, added or elevated.
        /// </summary>
        public static bool LogMessages
        {
            get => Instance.logMessages;
            set => Instance.logMessages = value;
        }
        
        /// <summary>
        /// Get a list of all ScriptDefineSymbolFiles located in the project.
        /// </summary>
        public static ICollection<PreprocessorSymbolDefinitionFile> ScriptDefineSymbolFiles =>
            Instance.scriptDefineSymbolFiles.IsNullOrIncomplete()
                ? Instance.scriptDefineSymbolFiles = Extensions.FindAllAssetsOfType<PreprocessorSymbolDefinitionFile>()
                : Instance.scriptDefineSymbolFiles;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [ELEVATED SYMBOLS] ---

        public static void RemoveElevatedSymbol(string symbol)
        {
            Instance.elevatedSymbols.TryRemove(symbol);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FILECACHING] ---

        public static void FindAllPreprocessorSymbolDefinitionFiles()
        {
            Instance.scriptDefineSymbolFiles?.Clear();
            foreach (var file in Extensions.FindAllAssetsOfType<PreprocessorSymbolDefinitionFile>())
            {
                if(file == null) continue;
                AddScriptDefineSymbolFile(file);
            }
        }

        public static void AddScriptDefineSymbolFile(PreprocessorSymbolDefinitionFile file)
        {
#if UNITY_2020_1_OR_NEWER
            (Instance.scriptDefineSymbolFiles ??= new List<PreprocessorSymbolDefinitionFile>()).AddUnique(file);
#else
             (Instance.scriptDefineSymbolFiles ?? (Instance.scriptDefineSymbolFiles = new List<PreprocessorSymbolDefinitionFile>())).AddUnique(file);
#endif
        }
        
        public static void RemoveScriptDefineSymbolFile(PreprocessorSymbolDefinitionFile file)
        {
#if UNITY_2020_1_OR_NEWER
            (Instance.scriptDefineSymbolFiles ??= new List<PreprocessorSymbolDefinitionFile>()).TryRemove(file);
#else
             (Instance.scriptDefineSymbolFiles ?? (Instance.scriptDefineSymbolFiles = new List<PreprocessorSymbolDefinitionFile>())).TryRemove(file);
#endif
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SINGLETON] ---

        public static PreprocessorSymbolDefinitionSettings Instance
#if UNITY_2020_1_OR_NEWER 
            => _instance ??= Extensions.FindAllAssetsOfType<PreprocessorSymbolDefinitionSettings>().FirstOrDefault() ?? CreateInstanceAsset();
#else
            => _instance ? _instance : _instance = Extensions.FindAllAssetsOfType<PreprocessorSymbolDefinitionSettings>().FirstOrDefault() ?? CreateInstanceAsset();
#endif
        
        
        private static PreprocessorSymbolDefinitionSettings _instance = null;

        private static PreprocessorSymbolDefinitionSettings CreateInstanceAsset()
        {
            var asset = CreateInstance<PreprocessorSymbolDefinitionSettings>();
            AssetDatabase.CreateAsset(asset, CreateFilePath());
            AssetDatabase.SaveAssets();
            return asset;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [MISCELLANEOUS] ---

        public static void Select()
        {
            Selection.activeObject = Instance;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FILECREATION] ---
     
        /// <summary>
        /// Returns the best fitting filepath for the configuration asset.
        /// </summary>
        /// <returns></returns>
        private static string CreateFilePath()
        {
            foreach (var path in _preferredPaths)
            {
                if (Directory.Exists(path))
                    return $"{path}/{FILENAME_ASSET}";
            }

            return _defaultPath;
        }
        
        private const string FILENAME_ASSET = "Preprocessor-Definition-Settings.asset";
        private static readonly string _defaultPath = $"Assets/{FILENAME_ASSET}";
        
        
        private static readonly string[] _preferredPaths =
        {
            "Assets/PreprocessorDefinitionFiles",
            "Assets/Baracuda/PreprocessorDefinitionFiles",
            "Assets/Modules/PreprocessorDefinitionFiles",
            "Assets/Plugins/PreprocessorDefinitionFiles",
            "Assets/Plugins/Baracuda/PreprocessorDefinitionFiles",
        };
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [ON BEFORE COMPILE: AUTOSAVE] ---
        
        private void OnEnable()
        {
            if (AssetDatabase.GetAssetPath(this) == _defaultPath)
            {
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(this), CreateFilePath());
            }
#if UNITY_2020_2_OR_NEWER
            UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnCompilationStarted;
#endif
        }

        private void OnDisable()
        {
#if UNITY_2020_2_OR_NEWER
            UnityEditor.Compilation.CompilationPipeline.compilationStarted -= OnCompilationStarted;
#endif
        }

#if UNITY_2020_2_OR_NEWER
        private static void OnCompilationStarted(object obj)
        {
            if(SaveOnCompile)
                PreprocessorDefineUtilities.ApplyAndUpdateAllDefinitionFiles();
        }
#endif
        #endregion

    }

    internal static class Documentation
    {
        private const string ONLINE_DOCS = "https://johnbaracuda.com/ppsdf.html";
        private static GUIContent DocsButtonGUIContent(string selector) => 
            new GUIContent("Documentation", $"Open the online documentation for this asset.\n{ONLINE_DOCS}#{selector}");
        
        internal static void OpenOnlineDocumentation(string selector = null)
        {
            if (selector is null)
            {
                Application.OpenURL(ONLINE_DOCS);
            }
            else
            {
                Application.OpenURL($"{ONLINE_DOCS}#{selector}");   
            }
        }

        /// <summary>
        /// Draw a button that will open the online documentation for this asset.
        /// </summary>
        internal static void DrawDocumentationButton(string selector = null)
        {
            if (GUILayout.Button(DocsButtonGUIContent(selector)))
            {
                OpenOnlineDocumentation(selector);
            }
        }
    }
}
