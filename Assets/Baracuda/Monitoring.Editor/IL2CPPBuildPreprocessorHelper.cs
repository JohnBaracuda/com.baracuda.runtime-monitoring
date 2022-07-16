using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Baracuda.Monitoring.Editor
{
    internal static class IL2CPPBuildPreprocessorHelper
    {
        
        #region --- Fields ---
        
        private static UnityEditor.Compilation.Assembly[] UnityAssemblies { get; }

        static IL2CPPBuildPreprocessorHelper()
        {
            UnityAssemblies = CompilationPipeline.GetAssemblies();
        }

        #endregion

        #region --- Extension Methods ---

        
        public static bool IsEditorAssembly(this Assembly assembly)
        {
            var editorAssemblies = UnityAssemblies;

            for (var i = 0; i < editorAssemblies.Length; i++)
            {
                var unityAssembly = editorAssemblies[i];

                if (unityAssembly.name != assembly.GetName().Name)
                {
                    continue;
                }
#if UNITY_2020_1_OR_NEWER
                if (unityAssembly.flags.HasFlagUnsafe(AssemblyFlags.EditorAssembly))
                {
                    return true;
                }
#else
                var intFlag = (int) unityAssembly.flags;
                if (intFlag.HasFlag32((int)AssemblyFlags.EditorAssembly))
                {
                    return true;
                }
#endif
            }

            return false;
        }
        
        
        private static readonly Dictionary<Type, string> typeCacheFullName = new Dictionary<Type, string>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToGenericTypeStringFullName(this Type type)
        {
            if (typeCacheFullName.TryGetValue(type, out var value))
            {
                return value;
            }

            if (type.IsStatic())
            {
                return typeof(object).FullName?.Replace('+', '.');
            }

            if (type.IsGenericType)
            {
                var builder = ConcurrentStringBuilderPool.Get();
                var argBuilder = ConcurrentStringBuilderPool.Get();

                var arguments = type.GetGenericArguments();

                foreach (var t in arguments)
                {
                    // Let's make sure we get the argument list.
                    var arg = ToGenericTypeStringFullName(t);

                    if (argBuilder.Length > 0)
                    {
                        argBuilder.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        argBuilder.Append(arg);
                    }
                }

                if (argBuilder.Length > 0)
                {
                    Debug.Assert(type.FullName != null, "type.FullName != null");
                    builder.AppendFormat("{0}<{1}>", type.FullName.Split('`')[0],
                        argBuilder);
                }

                var retType = builder.ToString();

                typeCacheFullName.Add(type, retType.Replace('+', '.'));

                ConcurrentStringBuilderPool.ReleaseStringBuilder(builder);
                ConcurrentStringBuilderPool.ReleaseStringBuilder(argBuilder);
                return retType.Replace('+', '.');
            }

            Debug.Assert(type.FullName != null, $"type.FullName != null | {type.Name}, {type.DeclaringType}");
            
            var returnValue = type.FullName.Replace('+', '.');
            typeCacheFullName.Add(type, returnValue);
            return returnValue;
        }
        
        #endregion
    }
}