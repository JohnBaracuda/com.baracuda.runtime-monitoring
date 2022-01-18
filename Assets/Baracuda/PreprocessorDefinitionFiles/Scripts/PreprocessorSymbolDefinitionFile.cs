using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.PreprocessorDefinitionFiles.Utilities;
using UnityEngine;

namespace Baracuda.PreprocessorDefinitionFiles
{
    /// <summary>
    /// Instances of this class will manage and store individual preprocessor definition symbols.
    /// </summary>
    [CreateAssetMenu(menuName = "Preprocessor Definition", fileName = "Preprocessor-Definition", order = 89)]
    public sealed class PreprocessorSymbolDefinitionFile : ScriptableObject, IDisposable
    {
        #region --- [FIELDS & PROPERTIES] ---
        
        /// <summary>
        /// The list storing the preprocessor symbols of this object.
        /// </summary>
        [Tooltip("List containing script symbolData definitions")]
        [SerializeField] [HideInInspector] private List<PreprocessorSymbolData> scriptSymbolDefinitions;
        
        /// <summary>
        /// Get a collection of preprocessor symbols contained within data objects storing additional information.
        /// </summary>
        public IEnumerable<PreprocessorSymbolData> LocalSymbols =>
            scriptSymbolDefinitions?.Where(x => !string.IsNullOrWhiteSpace(x.Symbol)) ?? Array.Empty<PreprocessorSymbolData>();
        
        /// <summary>
        /// List contains symbols that will be removed when changes to the object are applied.
        /// </summary>
        [SerializeField] [HideInInspector] private List<string> symbolToRemove = new List<string>();
        
        /// <summary>
        /// List contains symbols of the last applied state of the object. This list is used to determine which symbols
        /// will be added and which symbols will be removed.
        /// </summary>
        [SerializeField] [HideInInspector] private List<string> symbolCache = new List<string>();
        
        /// <summary>
        /// Name of the list object used for reflection by editor scripts.
        /// </summary>
        internal const string SYMBOLS_PROPERTY_NAME = nameof(scriptSymbolDefinitions);
    
        #endregion
    
        //------------------------------------------------------------------------------------------------------------------

        #region --- [PREPROCESSOR SYMBOL UPDATES] ---

        /// <summary>
        /// Returns a collection of 'valid' symbols that will be applied. 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetValidPreprocessorDefines() =>
            from localSymbol in LocalSymbols
            where localSymbol.Enabled && localSymbol.IsValid && localSymbol.TargetGroup.HasFlag(PreprocessorDefineUtilities.FlagsBuildTargetCache)
            select localSymbol.Symbol;
        
        
        
        /// <summary>
        /// Get symbols stored in this object and apply them on a global scale.
        /// This will also remove symbols that were deactivated.
        /// </summary>
        internal void ApplyPreprocessorDefines()
        {
            UpdateAndValidateSymbolCache();
            
            // Get a list of all global symbols.
            var oldDefines = PreprocessorDefineUtilities.GetCustomDefinesOfActiveTargetGroup().ToList();
            
            // Create a new list and add the valid symbols of this object.
            var newDefines = new List<string>(GetValidPreprocessorDefines());

            // Iterate old definitions and remove the once that need to be removed.
            foreach (var symbol in oldDefines)
            {
                if (!symbolToRemove.Contains(symbol))
                {
                    newDefines.Add(symbol);
                }
                else if (PreprocessorDefineUtilities.IsSymbolElevated(symbol))
                {
                    newDefines.Add(symbol);
                }
            }
            
            // Determine which symbols were added.
            var symbolsToAdd = newDefines.Where(newSymbol => !oldDefines.Contains(newSymbol));

            // ReSharper disable once PossibleMultipleEnumeration
            if (symbolsToAdd.Any() || symbolToRemove.Any())
            {
                // Apply the updated defines on a global scale.
                // ReSharper disable once PossibleMultipleEnumeration
                PreprocessorDefineUtilities.SetCustomDefinesOfActiveTargetGroup(newDefines.RemoveDuplicates(),symbolsToAdd,symbolToRemove);
            }
            
            // Clear the symbol cache
            symbolToRemove.Clear();
        }

        /// <summary>
        /// Remove all preprocessor defines managed and contained in this object.
        /// </summary>
        public void RemovePreprocessorDefines()
        {
            var currentSymbols = PreprocessorDefineUtilities.GetCustomDefinesOfActiveTargetGroup().ToList();
            var removedSymbols = currentSymbols.Where(symbol => GetValidPreprocessorDefines().Contains(symbol)).ToList();
            var updatedSymbols = currentSymbols.Where(symbol => !GetValidPreprocessorDefines().Contains(symbol)).ToList();

            scriptSymbolDefinitions = null;
            PreprocessorDefineUtilities.SetCustomDefinesOfActiveTargetGroup(updatedSymbols, null, removedSymbols);
            
        }

        public void RemovePreprocessorSymbol(PreprocessorSymbolData symbol)
        {
            scriptSymbolDefinitions.TryRemove(symbol);
        }
        
        public void RemovePreprocessorSymbol(string symbol)
        {
            for (short i = 0; i < scriptSymbolDefinitions.Count; i++)
            {
                if (scriptSymbolDefinitions[i].Symbol == symbol)
                {
                    scriptSymbolDefinitions.RemoveAt(i);
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [ENABLE / DESTROY] ---

        private void OnEnable()
        {
            PreprocessorSymbolDefinitionSettings.AddScriptDefineSymbolFile(this);
        }

        /// <summary>
        /// Remove symbols handled by this file if necessary before it is deleted.
        /// </summary>
        public void Dispose()
        {
            if (PreprocessorSymbolDefinitionSettings.RemoveSymbolsOnDelete)
            {
                RemovePreprocessorDefines();
            }
            PreprocessorSymbolDefinitionSettings.RemoveScriptDefineSymbolFile(this);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---
        
        private void UpdateAndValidateSymbolCache()
        {
            foreach (var cachedSymbol in symbolCache)
            {
                if (!GetValidPreprocessorDefines().Contains(cachedSymbol) && !symbolToRemove.Contains(cachedSymbol))
                {
                    symbolToRemove.Add(cachedSymbol);
                }
            }
            symbolCache.Clear();
            symbolCache = new List<string>(GetValidPreprocessorDefines());
        }

        #endregion
    }
}
