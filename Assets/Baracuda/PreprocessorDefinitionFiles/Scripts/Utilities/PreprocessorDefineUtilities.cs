using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Baracuda.PreprocessorDefinitionFiles.Utilities
{
    /// <summary>
    /// Class containing utility methods that are directly used for the handling of preprocessor definition files.
    /// </summary>
    public static class PreprocessorDefineUtilities
    {
        
        #region --- [PUBLIC DEFINES] ---

        /// <summary>
        /// Get a list of current compiler defines.
        /// </summary>
        public static readonly IReadOnlyList<string> CompilerDefines = new List<string>
        {
#if CSHARP_7_3_OR_NEWER
            "CSHARP_7_3_OR_NEWER",
#endif
#if ENABLE_MONO
            "ENABLE_MONO",
#endif
#if ENABLE_IL2CPP
            "ENABLE_IL2CPP",
#endif
#if NET_2_0
            "NET_2_0",
#endif
#if NET_2_0_SUBSET
            "NET_2_0_SUBSET",
#endif
#if NET_LEGACY
            "NET_LEGACY",
#endif
#if NET_4_6
            "NET_4_6",
#endif
#if NET_STANDARD_2_0
            "NET_STANDARD_2_0",
#endif
#if ENABLE_WINMD_SUPPORT
            "ENABLE_WINMD_SUPPORT",
#endif
#if ENABLE_INPUT_SYSTEM
            "ENABLE_INPUT_SYSTEM",
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            "ENABLE_LEGACY_INPUT_MANAGER",
#endif
        };

        /// <summary>
        /// Get a list of current platform defines.
        /// </summary>
        public static readonly IReadOnlyList<string> PlatformDefines = new List<string>
        {
#if UNITY_EDITOR
            "UNITY_EDITOR",
#endif
#if UNITY_EDITOR_WIN
            "UNITY_EDITOR_WIN",
#endif
#if UNITY_EDITOR_OSX
            "UNITY_EDITOR_OSX",
#endif
#if UNITY_EDITOR_LINUX
            "UNITY_EDITOR_LINUX",
#endif
#if UNITY_STANDALONE_OSX
            "UNITY_STANDALONE_OSX",
#endif
#if UNITY_STANDALONE_WIN
            "UNITY_STANDALONE_WIN",
#endif
#if UNITY_STANDALONE_LINUX
            "UNITY_STANDALONE_LINUX",
#endif
#if UNITY_STANDALONE
            "UNITY_STANDALONE",
#endif
#if UNITY_WII
            "UNITY_WII",
#endif
#if UNITY_IOS
            "UNITY_IOS",
#endif
#if UNITY_IPHONE
            "UNITY_IPHONE",
#endif
#if UNITY_ANDROID
            "UNITY_ANDROID",
#endif
#if UNITY_PS4
            "UNITY_PS4",
#endif
#if UNITY_XBOXONE
            "UNITY_XBOXONE",
#endif
#if UNITY_LUMIN
            "UNITY_LUMIN",
#endif
#if UNITY_TIZEN
            "UNITY_TIZEN",
#endif
#if UNITY_TVOS
            "UNITY_TVOS",
#endif
#if UNITY_WSA
            "UNITY_WSA",
#endif
#if UNITY_WSA_10_0
            "UNITY_WSA_10_0",
#endif
#if UNITY_WINRT
            "UNITY_WINRT",
#endif
#if UNITY_WINRT_10_0
            "UNITY_WINRT_10_0",
#endif
#if UNITY_WEBGL
            "UNITY_WEBGL",
#endif
#if UNITY_FACEBOOK
            "UNITY_FACEBOOK",
#endif
#if UNITY_ANALYTICS
            "UNITY_ANALYTICS",
#endif
#if UNITY_ASSERTIONS
            "UNITY_ASSERTIONS",
#endif
#if UNITY_64
            "UNITY_64",
#endif
        };
        
        
        /// <summary>
        /// Get a list of the "current" Version defines.
        /// </summary>
        public static IEnumerable<string> VersionDefines => _sVersionDefines ?? CreateVersionDefines();

        
        /// <summary>
        /// Backend field
        /// </summary>
        private static List<string> _sVersionDefines = null;
        private static List<string> CreateVersionDefines()
        {
            if (_sVersionDefines != null) throw new InvalidOperationException(
                $"Only allowed to call {nameof(CreateVersionDefines)} when {nameof(_sVersionDefines)} is null or empty!");
            
            _sVersionDefines = new List<string>();
            var splitVersion = Application.unityVersion.Split('.');
            _sVersionDefines.Add($"UNITY_{splitVersion[0]}"); // eg UNITY_2021
            _sVersionDefines.Add($"UNITY_{splitVersion[0]}_{splitVersion[1]}"); // eg UNITY_2021_1
            _sVersionDefines.Add($"UNITY_{splitVersion[0]}_{splitVersion[1]}_OR_NEWER"); // eg UNITY_2021_1_OR_NEWER

            return _sVersionDefines;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [SYMBOL ELEVATION] ---

        /// <summary>
        /// Creates a list of symbols with an elevated security level.
        /// Those symbols will not be handles by Definition files. 
        /// </summary>
        [InitializeOnLoadMethod]
        internal static void ElevateIndependentSymbols()
        {
            _sIsProSkin = EditorGUIUtility.isProSkin;
            
            var files = PreprocessorSymbolDefinitionSettings.ScriptDefineSymbolFiles;
            if(files.IsNullOrIncomplete()) return;
            
            // Create a temp list containing every symbol defined in a PreprocessorSymbolDefinitionFile.
            var localSymbols = new List<string>();
            foreach (var sdsFile in files)
            {
                localSymbols.AddRange(sdsFile.LocalSymbols.Select(local => local.Symbol));
            }
            

            foreach (var global in GetCustomDefinesOfActiveTargetGroup())
            {
                if (IsGlobalSymbolSecurityRequired(global, localSymbols))
                {
                    if (PreprocessorSymbolDefinitionSettings.ElevatedSymbols.AddUnique(global) && PreprocessorSymbolDefinitionSettings.LogMessages)
                    {
                        Debug.Log($"Elevating Scripting Define Symbol:  [{ColorElevated}{global}</color>]");
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the passed symbol should have an elevated security level.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsGlobalSymbolSecurityRequired(string global, IEnumerable<string> localSymbols)
        {
            return !string.IsNullOrWhiteSpace(global) && localSymbols.All(local => !global.Equals(local));
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [BUILD TARGET] ---
        
        /// <summary>
        /// Get the current Build Target Group of the application.
        /// </summary>
        internal static BuildTargetGroup BuildTarget
        {
            get
            {
                BuildTargetCache = EditorUserBuildSettings.selectedBuildTargetGroup;
                FlagsBuildTargetCache = AsSingleFlags(BuildTargetCache);
                return (BuildTargetCache);
            }
        }

        /// <summary>
        /// Get the current Build Target Group of the application using a cached value that can be addressed during GUI events.
        /// </summary>
        internal static BuildTargetGroup BuildTargetCache { get; private set; }
        
        internal static FlagsBuildTargetGroup FlagsBuildTargetCache { get; private set; }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [GET SET GLOBAL DEFINES] ---

        /// <summary>
        /// Returns a collection of currently applied and active Scripting Define Symbols.
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<string> GetCustomDefinesOfActiveTargetGroup()
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTarget).Split(';');
        }
        
        private static bool _sIsProSkin = true;
        
        private static string ColorElevated 
            => _sIsProSkin? "<color=#75e09c>" : "<color=#1e5438>";
        
        private static string ColorAdded 
            => _sIsProSkin? "<color=#4babeb>" : "<color=#204c69>";
        
        private static string ColorRemoved 
            => _sIsProSkin? "<color=#eb4b6b>" : "<color=#99183d>";
        
        /// <summary>
        /// Sets the passed collection of symbols and applies them on a global scale.
        /// </summary>
        /// <returns></returns>
        internal static void SetCustomDefinesOfActiveTargetGroup(IEnumerable<string> preprocessorDefines, IEnumerable<string> addedSymbols = null, IEnumerable<string> removedSymbols = null)
        {
            if (PreprocessorSymbolDefinitionSettings.LogMessages)
            {
                foreach (var addedSymbol in addedSymbols ?? new string[0])
                {
                    Debug.Log($"Adding [{ColorAdded}{addedSymbol}</color>] to Scripting Define Symbols");
                }
                foreach (var removedSymbol in removedSymbols ?? new string[0])
                {
                    Debug.Log($"Removing [{ColorRemoved}{removedSymbol}</color>] from Scripting Define Symbols");
                }
            }
            
#if UNITY_2020_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTarget, preprocessorDefines as string[] ?? preprocessorDefines.ToArray());
#else
                        
            var symbolString = preprocessorDefines.Aggregate(string.Empty, (current, symbol) => $"{current}{symbol};");
            if (symbolString.Length <= 0)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTarget, string.Empty);
                return;
            }
            symbolString = symbolString.Remove(symbolString.Length - 1);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTarget, symbolString);
#endif
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE] ---

        /// <summary>
        /// Find all ScriptDefinitionSymbol files and update their symbols.
        /// </summary>
        internal static void ApplyAndUpdateAllDefinitionFiles()
        {
            foreach (var definitionFiles in PreprocessorSymbolDefinitionSettings.ScriptDefineSymbolFiles)
            {
                definitionFiles.ApplyPreprocessorDefines();
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SYMBOL VALIDATION] ---

        /// <summary>
        /// Returns true if the symbol is defined on a global scale.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSymbolDefined(PreprocessorSymbolData preprocessorSymbolData)
            => IsSymbolDefined(preprocessorSymbolData.Symbol);
        
        /// <summary>
        /// Returns true if the symbol is defined on a global scale.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSymbolDefined(string symbol) 
            => GetCustomDefinesOfActiveTargetGroup().Any(defined => defined.Equals(symbol));

        /// <summary>
        /// Returns true if the symbol is defined on a global scale and is not managed by any Preprocessor Definition File.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSymbolElevated(string symbol) 
            => PreprocessorSymbolDefinitionSettings.ElevatedSymbols.Any(symbol.Equals);

        
        /// <summary>
        /// Returns true if the symbol is defined in multiple places.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSymbolDefinedElsewhere(string symbol, PreprocessorSymbolDefinitionFile file)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return false;
            
            foreach (var sdsFile in PreprocessorSymbolDefinitionSettings.ScriptDefineSymbolFiles)
            {
                if (sdsFile == file) continue;
                
                // Check if the symbol is already defined in another file.
                foreach (var fileSymbol in sdsFile.LocalSymbols)
                {
                    if (fileSymbol.Symbol == symbol) 
                        return true;
                }
            }
            
            return false;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HELPER] ---

        private static FlagsBuildTargetGroup AsSingleFlags(BuildTargetGroup current)
        {
            foreach (FlagsBuildTargetGroup value in Enum.GetValues(typeof(FlagsBuildTargetGroup)))
            {
                if (current.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
            
            return FlagsBuildTargetGroup.Unknown;
        }

        #endregion
    }
}
