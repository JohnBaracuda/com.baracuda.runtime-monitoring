using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace Baracuda.Reflection
{
    public static class AssemblyManagement
    {
        #region --- [FIELDS] ---

        private static readonly string[] _bannedAssemblyPrefixes = new string[]
        {
            "Newtonsoft",
            "netstandard",
            "System",
            "Unity",
            "Microsoft",
            "Mono.",
            "mscorlib",
            "NSubstitute",
            "nunit.",
            "JetBrains",
            "GeNa."
        };

        private static readonly string[] _bannedAssemblyNames = new string[]
        {
            "mcs",
            "AssetStoreTools",
            "PPv2URPConverters"
        };
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [ASSEMBLY FILTERING] ---

        /// <summary>
        /// Method will initialize and filter all available assemblies only leaving custom assemblies.
        /// Precompiled unity and system assemblies as well as some other known assemblies will be excluded by default.
        /// </summary>
        /// <param name="excludeNames">Custom array of names of assemblies that should be excluded from the result</param>
        /// <param name="excludePrefixes">Custom array of prefixes for names of assemblies that should be excluded from the result</param>
        /// <returns></returns>
        public static Assembly[] GetFilteredAssemblies(string[] excludeNames, string[] excludePrefixes)
        {
            var sw = Stopwatch.StartNew();
            var filteredAssemblies = new List<Assembly>(30);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                if (IsAssemblyValidForReflection(assemblies[i], excludeNames, excludePrefixes))
                {
                    filteredAssemblies.Add(assemblies[i]);
                }
            }

            Debug.Log(sw.ElapsedMilliseconds);
            return filteredAssemblies.ToArray();
        }
        
        private static bool IsAssemblyValidForReflection(Assembly assembly, IReadOnlyList<string> excludeNames, IReadOnlyList<string> excludePrefixes)
        {
            var assemblyFullName = assembly.FullName;
            for (var i = 0; i < _bannedAssemblyPrefixes.Length; i++)
            {
                var prefix = _bannedAssemblyPrefixes[i];
                if (assemblyFullName.StartsWith(prefix))
                {
                    return false;
                }
            }
            for (var i = 0; i < excludePrefixes.Count; i++)
            {
                var prefix = excludePrefixes[i];
                if (assemblyFullName.StartsWith(prefix))
                {
                    return false;
                }
            }

            var assemblyShortName = assembly.GetName().Name;
            for (var i = 0; i < _bannedAssemblyNames.Length; i++)
            {
                var name = _bannedAssemblyNames[i];
                if (assemblyShortName == name)
                {
                    return false;
                }
            }
            for (var i = 0; i < excludeNames.Count; i++)
            {
                var name = excludeNames[i];
                if (assemblyShortName == name)
                {
                    return false;
                }
            }

            return true;
        }
        
        #endregion
    }
}
