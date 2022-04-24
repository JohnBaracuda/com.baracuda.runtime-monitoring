using System;
using System.Collections.Generic;
using System.Reflection;

namespace Baracuda.Monitoring.Internal.Reflection
{
    public static class AssemblyManagement
    {
        #region --- Assembly Filtering ---

        /*
         *  Assembly Filter Data   
         */

        private static readonly string[] bannedAssemblyPrefixes = new string[]
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

        private static readonly string[] bannedAssemblyNames = new string[]
        {
            "mcs",
            "AssetStoreTools",
            "PPv2URPConverters"
        };

        /*
         *  Assembly Filter Process
         */

        /// <summary>
        /// Method will initialize and filter all available assemblies only leaving custom assemblies.
        /// Precompiled unity and system assemblies as well as some other known assemblies will be excluded by default.
        /// </summary>
        /// <param name="excludeNames">Custom array of names of assemblies that should be excluded from the result</param>
        /// <param name="excludePrefixes">Custom array of prefixes for names of assemblies that should be excluded from the result</param>
        public static Assembly[] GetFilteredAssemblies(string[] excludeNames = null, string[] excludePrefixes = null)
        {
            var filteredAssemblies = new List<Assembly>(30);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                if (IsAssemblyValidForReflection(assemblies[i],
                        excludeNames ?? Array.Empty<string>(),
                        excludePrefixes ?? Array.Empty<string>()))
                {
                    filteredAssemblies.Add(assemblies[i]);
                }
            }

            return filteredAssemblies.ToArray();
        }

        private static bool IsAssemblyValidForReflection(Assembly assembly, IReadOnlyList<string> excludeNames,
            IReadOnlyList<string> excludePrefixes)
        {
            var assemblyFullName = assembly.FullName;
            for (var i = 0; i < bannedAssemblyPrefixes.Length; i++)
            {
                var prefix = bannedAssemblyPrefixes[i];
                if (!string.IsNullOrWhiteSpace(prefix) && assemblyFullName.StartsWith(prefix))
                {
                    return false;
                }
            }

            for (var i = 0; i < excludePrefixes.Count; i++)
            {
                var prefix = excludePrefixes[i];
                if (!string.IsNullOrWhiteSpace(prefix) && assemblyFullName.StartsWith(prefix))
                {
                    return false;
                }
            }

            var assemblyShortName = assembly.GetName().Name;
            for (var i = 0; i < bannedAssemblyNames.Length; i++)
            {
                var name = bannedAssemblyNames[i];
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
