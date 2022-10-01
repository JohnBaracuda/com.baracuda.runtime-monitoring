// Copyright (c) 2022 Jonathan Lang

#if ENABLE_IL2CPP || UNITY_EDITOR

using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Core.IL2CPP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Scripting;
using Assembly = System.Reflection.Assembly;

namespace Baracuda.Monitoring.Editor
{
    internal class IL2CPPBuildPreprocessor : IPreprocessBuildWithReport
    {
        #region --- IPreprocessBuildWithReport ---

        /// <summary>
        ///   <para>Returns the relative callback order for callbacks.  Callbacks with lower values are called before ones with higher values.</para>
        /// </summary>
        public int callbackOrder => MonitoringSystems.MonitoringSettings.PreprocessBuildCallbackOrder;

        /// <summary>
        ///   <para>Implement this function to receive a callback before the build is started.</para>
        /// </summary>
        /// <param name="report">A report containing information about the build, such as its target platform and output path.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!MonitoringSystems.MonitoringSettings.UseIPreprocessBuildWithReport)
            {
                return;
            }

            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            if (PlayerSettings.GetScriptingBackend(group) == ScriptingImplementation.IL2CPP)
            {
                OnPreprocessBuildInternal();
            }
        }

        #endregion

        #region --- Fields ---

        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;
        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private readonly string preserveAttribute = $"[{typeof(PreserveAttribute).FullName}]";
        private readonly string methodImpAttribute = $"[{typeof(MethodImplAttribute).FullName}({typeof(MethodImplOptions).FullName}.{MethodImplOptions.NoOptimization.ToString()})]";

        private readonly string typeDefField = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefField)}";
        private readonly string typeDefProperty = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefProperty)}";
        private readonly string typeDefEvent = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefEvent)}";
        private readonly string typeDefMethod = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefMethod)}";
        private readonly string typeDefOutParameter = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefOutParameter)}";

        private readonly bool throwExceptions = false;

        private readonly UnityEditor.Compilation.Assembly[] unityAssemblies;

        private List<Exception> ExceptionBuffer { get; } = new List<Exception>();
        private List<string> TypeDefFieldBuffer { get; } = new List<string>();
        private List<string> TypeDefPropertyBuffer { get; } = new List<string>();
        private List<string> TypeDefEventBuffer { get; } = new List<string>();
        private List<string> TypeDefMethodBuffer { get; } = new List<string>();
        private List<string> TypeDefOutParameterBuffer { get; } = new List<string>();

        private StatCounter Stats { get; } = new StatCounter();

        private readonly Assembly[] assemblies;

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

        #endregion

        public IL2CPPBuildPreprocessor()
        {
            unityAssemblies = CompilationPipeline.GetAssemblies();
            assemblies = GetFilteredAssemblies();
            throwExceptions = MonitoringSystems.MonitoringSettings.ThrowOnTypeGenerationError;
        }

        private void OnPreprocessBuildInternal()
        {
            var textFile = MonitoringSystems.MonitoringSettings.ScriptFileIL2CPP;
            var filePath = AssetDatabase.GetAssetPath(textFile);
            Debug.Log($"[Monitoring] Generating Type Definitions for IL2CPP at:\n{filePath}");

            for (var i = 0; i < assemblies.Length; i++)
            {
                ProfileAssembly(assemblies[i]);
            }

            var stringBuilder = new StringBuilder(short.MaxValue);

            AppendHeaderText(stringBuilder);
            AppendIfDefBegin(stringBuilder);
            AppendOpenClass(stringBuilder);

            AppendTypeDefinitions(stringBuilder);

            AppendCloseClass(stringBuilder);
            AppendIfDefEnd(stringBuilder);

            AppendStats(stringBuilder);

            WriteContentToFile(filePath, stringBuilder);

            Debug.Log($"[Monitoring] Completed Type Definitions for IL2CPP at:\n{filePath}");
            Debug.Log($"[Monitoring] {Stats}");

            foreach (var exception in ExceptionBuffer)
            {
                Debug.LogException(exception);
            }
        }

        internal static void GenerateIL2CPPAheadOfTimeTypes()
        {
            new IL2CPPBuildPreprocessor().OnPreprocessBuildInternal();
        }

        private void WriteContentToFile(string filePath, StringBuilder stringBuilder)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            stream.Dispose();
            File.WriteAllText(filePath, stringBuilder.ToString());
        }

        #region --- Profile Assmelby ---

        private Assembly[] GetFilteredAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyBuffer = new List<Assembly>(64);

            foreach (var assembly in allAssemblies)
            {
                if (assembly.GetCustomAttribute<DisableMonitoringAttribute>() != null)
                {
                    continue;
                }

                var assemblyName = assembly.GetName().Name;

                if (bannedAssemblyPrefixes.Any(assemblyName.StartsWith))
                {
                    continue;
                }

                if (bannedAssemblyNames.Any(assemblyName.Equals))
                {
                    continue;
                }

                if (assembly.IsEditorAssembly(unityAssemblies))
                {
                    continue;
                }

                assemblyBuffer.Add(assembly);
            }

            return assemblyBuffer.ToArray();
        }

        #endregion

        #region --- Profiling ---

         private void ProfileAssembly(Assembly filteredAssembly)
        {
            foreach (var type in filteredAssembly.GetTypes())
            {
                ProfileType(type);
            }
        }

        private void ProfileType(Type type)
        {
            // Static Fields
            foreach (var fieldInfo in type.GetFields(STATIC_FLAGS))
            {
                if (fieldInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfileFieldInfo(fieldInfo);
                }
            }

            // Instance Fields
            foreach (var fieldInfo in type.GetFields(INSTANCE_FLAGS))
            {
                if (fieldInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfileFieldInfo(fieldInfo);
                }
            }

            // Static Properties
            foreach (var propertyInfo in type.GetProperties(STATIC_FLAGS))
            {
                if (propertyInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfilePropertyInfo(propertyInfo);
                }
            }

            // Instance Properties
            foreach (var propertyInfo in type.GetProperties(INSTANCE_FLAGS))
            {
                if (propertyInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfilePropertyInfo(propertyInfo);
                }
            }

            // Static Events
            foreach (var eventInfo in type.GetEvents(STATIC_FLAGS))
            {
                if (eventInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfileEventInfo(eventInfo);
                }
            }

            // Instance Events
            foreach (var eventInfo in type.GetEvents(INSTANCE_FLAGS))
            {
                if (eventInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfileEventInfo(eventInfo);
                }
            }

            // Static Methods
            foreach (var methodInfo in type.GetMethods(STATIC_FLAGS))
            {
                if (methodInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfileMethodInfo(methodInfo);
                }
            }

            // Instance Methods
            foreach (var methodInfo in type.GetMethods(INSTANCE_FLAGS))
            {
                if (methodInfo.GetCustomAttribute<MonitorAttribute>(true) != null)
                {
                    ProfileMethodInfo(methodInfo);
                }
            }
        }

        #endregion

        #region --- Profile Types ---

        private void ProfileFieldInfo(FieldInfo fieldInfo)
        {
            try
            {
                Assert.IsNotNull(fieldInfo.DeclaringType);

                var declaring = fieldInfo.DeclaringType.IsValueType ? typeof(ValueType) : fieldInfo.DeclaringType;
                var monitored = fieldInfo.FieldType.GetUnderlying();

                if (declaring.IsGenericTypeDefinition)
                {
                    var subTypes = declaring.GetAllTypesImplementingOpenGenericType(assemblies);

                    foreach (var subType in subTypes)
                    {
                        var subField = fieldInfo.IsStatic
                            ? subType.GetField(fieldInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                            : subType.GetField(fieldInfo.Name);
                        ProfileFieldInfo(subField);
                    }
                    return;
                }

                var usableDeclaring = (declaring.IsAccessible() && !declaring.IsStatic())
                    ? declaring
                    : declaring.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefField}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(fieldInfo.IsStatic ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Fields", "MemberInfo");
                Stats.IncrementStat($"Monitored Fields {(fieldInfo.IsStatic ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

                TypeDefFieldBuffer.AddUnique(typeDef);
            }
            catch (Exception exception)
            {
                if (throwExceptions)
                {
                    throw;
                }
                ExceptionBuffer.Add(exception);
            }
        }

        private void ProfilePropertyInfo(PropertyInfo propertyInfo)
        {
            try
            {
                Assert.IsNotNull(propertyInfo.DeclaringType);

                var declaring = propertyInfo.DeclaringType.IsValueType ? typeof(ValueType) : propertyInfo.DeclaringType;
                var monitored = propertyInfo.PropertyType.GetUnderlying();;

                if (declaring.IsGenericTypeDefinition)
                {
                    var subTypes = declaring.GetAllTypesImplementingOpenGenericType(assemblies);

                    foreach (var subType in subTypes)
                    {
                        var subProperty = propertyInfo.IsStatic()
                            ? subType.GetProperty(propertyInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                            : subType.GetProperty(propertyInfo.Name);
                        ProfilePropertyInfo(subProperty);
                    }
                    return;
                }

                var usableDeclaring = (declaring.IsAccessible() && !declaring.IsStatic())
                    ? declaring
                    : declaring.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefProperty}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(propertyInfo.IsStatic() ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Properties", "MemberInfo");
                Stats.IncrementStat($"Monitored Properties {(propertyInfo.IsStatic() ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");
                TypeDefPropertyBuffer.AddUnique(typeDef);
            }
            catch (Exception exception)
            {
                if (throwExceptions)
                {
                    throw;
                }
                ExceptionBuffer.Add(exception);
            }
        }

        private void ProfileEventInfo(EventInfo eventInfo)
        {
            try
            {
                Assert.IsNotNull(eventInfo.DeclaringType);

                var declaring = eventInfo.DeclaringType.IsValueType ? typeof(ValueType) : eventInfo.DeclaringType;
                var monitored = eventInfo.EventHandlerType.GetUnderlying();

                if (declaring.IsGenericTypeDefinition)
                {
                    var subTypes = declaring.GetAllTypesImplementingOpenGenericType(assemblies);

                    foreach (var subType in subTypes)
                    {
                        var subEvent = eventInfo.IsStatic()
                            ? subType.GetEvent(eventInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                            : subType.GetEvent(eventInfo.Name);
                        ProfileEventInfo(subEvent);
                    }
                    return;
                }

                var usableDeclaring = (declaring.IsAccessible() && !declaring.IsStatic())
                    ? declaring
                    : declaring.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefEvent}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(eventInfo.IsStatic() ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Events", "MemberInfo");
                Stats.IncrementStat($"Monitored Events {(eventInfo.IsStatic() ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

                TypeDefEventBuffer.AddUnique(typeDef);
            }
            catch (Exception exception)
            {
                if (throwExceptions)
                {
                    throw;
                }
                ExceptionBuffer.Add(exception);
            }
        }

        private void ProfileMethodInfo(MethodInfo methodInfo)
        {
            try
            {
                Assert.IsNotNull(methodInfo.DeclaringType);

                var declaring = methodInfo.DeclaringType.IsValueType ? typeof(ValueType) : methodInfo.DeclaringType;
                var monitored = methodInfo.ReturnType.GetUnderlying();;

                if (declaring.IsGenericTypeDefinition)
                {
                    var subTypes = declaring.GetAllTypesImplementingOpenGenericType(assemblies);

                    foreach (var subType in subTypes)
                    {
                        var subMethod = methodInfo.IsStatic
                            ? subType.GetMethod(methodInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                            : subType.GetMethod(methodInfo.Name);
                        ProfileMethodInfo(subMethod);
                    }
                    return;
                }

                if (monitored != typeof(void))
                {
                    TypeDefWithReturnValue(methodInfo, declaring, monitored);
                }
                else
                {
                    TypeDefVoid(methodInfo, declaring);
                }

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    ProfileParameterInfo(parameterInfo);
                }
            }
            catch (Exception exception)
            {
                if (throwExceptions)
                {
                    throw;
                }
                ExceptionBuffer.Add(exception);
            }

            void TypeDefVoid(MethodInfo method, Type declaring)
            {
                var usableDeclaring = (declaring.IsAccessible() && !declaring.IsStatic())
                    ? declaring
                    : declaring.GetReplacement();

                var typeDef = $"{typeDefMethod}<{usableDeclaring.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(method.IsStatic ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Methods", "MemberInfo");
                Stats.IncrementStat($"Monitored Methods {(method.IsStatic ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored void", "Monitored Types");

                TypeDefMethodBuffer.AddUnique(typeDef);
            }

            void TypeDefWithReturnValue(MethodInfo method, Type type, Type monitored)
            {
                var usableDeclaring = (type.IsAccessible() && !type.IsStatic())
                    ? type
                    : type.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefMethod}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(method.IsStatic ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Methods", "MemberInfo");
                Stats.IncrementStat($"Monitored Methods {(method.IsStatic ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

                TypeDefMethodBuffer.AddUnique(typeDef);
            }
        }

        private void ProfileParameterInfo(ParameterInfo parameterInfo)
        {
            try
            {
                if (!parameterInfo.IsOut)
                {
                    return;
                }

                var type = parameterInfo.ParameterType.GetUnderlying();

                var usableType = type.IsAccessible() && !type.IsStatic() && !type.IsReadonlyRefStruct()
                    ? type
                    : type.GetReplacement();

                var typeDef = $"{typeDefOutParameter}<{usableType.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(usableType.IsStatic() ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Out Parameter", "MemberInfo");
                Stats.IncrementStat($"Monitored Out Parameter {(usableType.IsStatic() ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {type.HumanizedName()}", "Monitored Types");

                TypeDefOutParameterBuffer.AddUnique(typeDef);
            }
            catch (Exception exception)
            {
                if (throwExceptions)
                {
                    throw;
                }
                ExceptionBuffer.Add(exception);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Append Definitions ---

        private void AppendTypeDefinitions(StringBuilder stringBuilder)
        {
            AppendComment(stringBuilder, "Value Processor Method Definitions");
            AppendLineBreak(stringBuilder);

            stringBuilder.Append("\n    ");
            stringBuilder.Append(preserveAttribute);
            stringBuilder.Append("\n    ");
            stringBuilder.Append(methodImpAttribute);
            stringBuilder.Append("\n    ");
            stringBuilder.Append("private static void AOT()");
            stringBuilder.Append("\n    ");
            stringBuilder.Append("{");

            TypeDefFieldBuffer.Sort();
            TypeDefPropertyBuffer.Sort();
            TypeDefEventBuffer.Sort();
            TypeDefMethodBuffer.Sort();

            AppendComment(stringBuilder, " Field type definitions", 8);
            for (var i = 0; i < TypeDefFieldBuffer.Count; i++)
            {
                var definition = TypeDefFieldBuffer[i];
                stringBuilder.Append('\n');
                stringBuilder.Append(new string(' ', 8));
                stringBuilder.Append(definition);
            }

            stringBuilder.Append('\n');
            AppendComment(stringBuilder, " Property type definitions", 8);
            for (var i = 0; i < TypeDefPropertyBuffer.Count; i++)
            {
                var definition = TypeDefPropertyBuffer[i];
                stringBuilder.Append('\n');
                stringBuilder.Append(new string(' ', 8));
                stringBuilder.Append(definition);
            }

            stringBuilder.Append('\n');
            AppendComment(stringBuilder, " Event type definitions", 8);
            for (var i = 0; i < TypeDefEventBuffer.Count; i++)
            {
                var definition = TypeDefEventBuffer[i];
                stringBuilder.Append('\n');
                stringBuilder.Append(new string(' ', 8));
                stringBuilder.Append(definition);
            }

            stringBuilder.Append('\n');
            AppendComment(stringBuilder, " Method type definitions", 8);
            for (var i = 0; i < TypeDefMethodBuffer.Count; i++)
            {
                var definition = TypeDefMethodBuffer[i];
                stringBuilder.Append('\n');
                stringBuilder.Append(new string(' ', 8));
                stringBuilder.Append(definition);
            }

            stringBuilder.Append('\n');
            AppendComment(stringBuilder, " Out Parameter type definitions", 8);
            for (var i = 0; i < TypeDefOutParameterBuffer.Count; i++)
            {
                var definition = TypeDefOutParameterBuffer[i];
                stringBuilder.Append('\n');
                stringBuilder.Append(new string(' ', 8));
                stringBuilder.Append(definition);
            }

            stringBuilder.Append("\n    ");
            stringBuilder.Append("}");
        }

        #endregion

        #region --- Append Meta ---

        private void AppendHeaderText(StringBuilder stringBuilder)
        {
            AppendCopyrightNote(stringBuilder);
            stringBuilder.Append('\n');
            AppendAutogeneratedMessage(stringBuilder);

            stringBuilder.Append('\n');
            stringBuilder.Append("//Runtime Monitoring");
            stringBuilder.Append('\n');
            stringBuilder.Append("//File generated: ");
            stringBuilder.Append(DateTime.Now.ToString("u"));
            stringBuilder.Append('\n');
            stringBuilder.Append("//Please dont change the contents of this file. Otherwise IL2CPP runtime may not work with runtime monitoring!");
            stringBuilder.Append('\n');
            stringBuilder.Append("//Ensure that this file is located in Assembly-CSharp. Otherwise this file may not compile.");
            stringBuilder.Append('\n');
            stringBuilder.Append("//If this file contains any errors please contact me and/or create an issue in the linked repository.");
            stringBuilder.Append('\n');
            stringBuilder.Append("//https://github.com/JohnBaracuda/Runtime-Monitoring");
            stringBuilder.Append('\n');
        }

        private void AppendCopyrightNote(StringBuilder stringBuilder)
        {
            stringBuilder.Append("// Copyright (c) 2022 Jonathan Lang\n");
        }

        private void AppendAutogeneratedMessage(StringBuilder stringBuilder)
        {
            stringBuilder.Append("//---------- ----------------------------- ----------\n");
            stringBuilder.Append("//---------- !!! AUTOGENERATED CONTENT !!! ----------\n");
            stringBuilder.Append("//---------- ----------------------------- ----------\n");
        }

        private void AppendIfDefBegin(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append("#if ENABLE_IL2CPP && !DISABLE_MONITORING");
            stringBuilder.Append('\n');
        }

        private void AppendIfDefEnd(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append("#endif //ENABLE_IL2CPP && !DISABLE_MONITORING");
            stringBuilder.Append('\n');
        }

        private void AppendOpenClass(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append("internal class IL2CPP_AOT");
            stringBuilder.Append('\n');
            stringBuilder.Append('{');
        }

        private void AppendCloseClass(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append('}');
            stringBuilder.Append('\n');
        }

        #endregion

        #region --- Append Stats ---

        private void AppendStats(StringBuilder stringBuilder)
        {
            AppendComment(stringBuilder, new string('-', 118), 0);
            AppendLineBreak(stringBuilder, 2);
            stringBuilder.Append(Stats.ToString(true));
            AppendComment(stringBuilder, new string('-', 118), 0);
            AppendLineBreak(stringBuilder);
            AppendComment(stringBuilder, "If this file contains any errors please contact me and/or create an issue in the linked repository.", 0);
            AppendComment(stringBuilder, "https://github.com/JohnBaracuda/Runtime-Monitoring", 0);
        }

        #endregion

        #region --- Append Misc ---

        private void AppendLineBreak(StringBuilder stringBuilder, int breaks = 1)
        {
            for (var i = 0; i < breaks; i++)
            {
                stringBuilder.Append('\n');
            }
        }

        private void AppendComment(StringBuilder stringBuilder, string comment, int indent = 4)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append(new string(' ', indent));
            stringBuilder.Append("//");
            stringBuilder.Append(comment);
        }

        private void AppendLine(StringBuilder stringBuilder)
        {
            stringBuilder.Append("    //");
            stringBuilder.Append(new string('-', 114));
            stringBuilder.Append('\n');
        }

        #endregion
    }
}
#endif
