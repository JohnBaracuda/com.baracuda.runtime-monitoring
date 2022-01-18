using System;
using UnityEngine;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Baracuda.PreprocessorDefinitionFiles.Utilities
{
    /// <summary>
    /// Class containing preprocessor symbols and additional related data.
    /// </summary>
    [Serializable]
    public sealed class PreprocessorSymbolData : ISerializationCallbackReceiver
    {
        #region --- [SERIALIZED] ---
        
        [Tooltip("The Scripting Define Symbol that will be added.")]
        [SerializeField] private string symbol = string.Empty;
        [Tooltip("Enable / Disable the usage of the symbol.")]
        [SerializeField] private bool enabled = true;
        [Tooltip("Set the Build Target Group for the symbol. The symbol will only be used if the current build target and the set value match.")]
        [SerializeField] private FlagsBuildTargetGroup targetGroup = PreprocessorDefineUtilities.FlagsBuildTargetCache;
        
        [SerializeField] [HideInInspector] private bool isValid;
        
        #endregion
        
        //--------------------------

        #region --- [PROPERTIES] ---
        
        internal string Symbol => symbol;
        internal bool Enabled => enabled;
        internal FlagsBuildTargetGroup TargetGroup => targetGroup;
        internal bool IsValid
        {
            get => isValid;
            set => isValid = value;
        }

        #endregion

        //--------------------------

        #region --- [SETTER] ---

        internal void SetEnabled(bool value) => enabled = value;

        #endregion
        
        //--------------------------
        
        #region --- [PROPERTY DRAWER HELPER] ---
        
        public const string NAMEOF_SYMBOL = nameof(symbol);
        public const string NAMEOF_ENABLED = nameof(enabled);
        public const string NAMEOF_TARGET = nameof(targetGroup);
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INTERFACE] ---

        public void OnBeforeSerialize()
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                enabled = true;
                targetGroup = targetGroup == FlagsBuildTargetGroup.Unknown
                    ? PreprocessorDefineUtilities.FlagsBuildTargetCache
                    : targetGroup;
            }
        }

        public void OnAfterDeserialize()
        {
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EQUALS] ---

        public override bool Equals(object obj)
            => obj is PreprocessorSymbolData other && Equals(other);

        private bool Equals(PreprocessorSymbolData other)
        {
            return symbol == other.symbol && targetGroup == other.targetGroup;
        }

        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (symbol != null ? symbol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) targetGroup;
                hashCode = (hashCode * 397) ^ isValid.GetHashCode();
                return hashCode;
            }
        }

        #endregion
        
    }
}