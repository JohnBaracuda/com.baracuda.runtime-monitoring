using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

#if !DISABLE_MONITORING

namespace Baracuda.Monitoring.Editor
{
    internal static class IL2CPPBuildExtensions
    {
        public static bool IsEditorAssembly(this Assembly systemAssembly, UnityEditor.Compilation.Assembly[] unityssemblies)
        {
            for (var i = 0; i < unityssemblies.Length; i++)
            {
                var unityAssembly = unityssemblies[i];

                if (unityAssembly.name != systemAssembly.GetName().Name)
                {
                    continue;
                }
                var intFlag = (int) unityAssembly.flags;
                if (unchecked((uint) intFlag & (uint) AssemblyFlags.EditorAssembly) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAccessible(this Type type)
        {
            var baseTypes = new List<Type> {type};

            while (type.DeclaringType != null)
            {
                baseTypes.Add(type.DeclaringType);
                type = type.DeclaringType;
            }

            for (var i = 0; i < baseTypes.Count; i++)
            {
                var baseType = baseTypes[i];
                if (!baseType.IsPublic && !baseType.IsNestedPublic)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif