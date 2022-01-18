using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Baracuda.PreprocessorDefinitionFiles.Utilities
{
    /// <summary>
    /// Class containing utility methods that are generally used.
    /// </summary>
    public static class Extensions
    {
        #region --- [COLLECTION EXTENSIONS] ---

        public static List<T> RemoveDuplicates<T>(this IEnumerable<T> target)
        {
            var list = new List<T>();
            foreach (var item in target)
            {
                if(!list.Contains(item))
                    list.Add(item);
            }
            
            return list;
        }

        /// <summary>
        /// Add a value to a collection if it is not already contained.
        /// </summary>
        /// <returns>True if value was added.</returns>
        public static bool AddUnique<T>(this ICollection<T> collection, T value)
        {
            if (collection.Contains(value))
            {
                return false;
            }
            collection.Add(value);
            return true;
        }

        /// <summary>
        /// Add a value to a dictionary if it is not already contained.
        /// </summary>
        /// <returns>True if value was added.</returns>
        public static bool AddUnique<TKey,TValue>(this IDictionary<TKey,TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            dictionary.Add(key, value);
            return true;
        }

        /// <summary>
        /// Remove a value from a collection if contained.
        /// </summary>
        /// <returns>True if value was contained.</returns>
        public static bool TryRemove<T>(this ICollection<T> collection, T value)
        {
            if (!collection.Contains(value)) return false;
            collection.Remove(value);
            return true;
        }

        /// <summary>
        /// Returns true if the collection is either null, empty or contains elements that are null.
        /// </summary>
        /// <param name="collection">Reference to the collection</param>
        /// <typeparam name="T">Type of the generic collection</typeparam>
        /// <returns>True if the collection is either null, empty or contains elements that are null</returns>
        public static bool IsNullOrIncomplete<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count <= 0 || collection.Any(x => x == null);
        }
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [REFLECTION] ---
        
        internal static string GetTooltip<T>(string name)
        {
            var member = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            var attributes = member?.GetCustomAttribute<TooltipAttribute>(true);
            return attributes?.tooltip ?? string.Empty;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [ASSETS] ---

        /// <summary>
        /// Expensive method that will find every asset of a certain type that is located anywhere within the project.
        /// </summary>
        /// <typeparam name="T">Type of the assets/s</typeparam>
        /// <returns></returns>
        internal static List<T> FindAllAssetsOfType<T>() where T : UnityEngine.Object
        {
            var assets = new List<T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null && !string.IsNullOrWhiteSpace(assetPath))
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        #endregion
        
    }
}