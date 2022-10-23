// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.IL2CPP;
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
        #region IPreprocessBuildWithReport

        /// <summary>
        ///   <para>Returns the relative callback order for callbacks.  Callbacks with lower values are called before ones with higher values.</para>
        /// </summary>
        public int callbackOrder => Monitor.Settings.PreprocessBuildCallbackOrder;

        /// <summary>
        ///   <para>Implement this function to receive a callback before the build is started.</para>
        /// </summary>
        /// <param name="report">A report containing information about the build, such as its target platform and output path.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Monitor.Settings.UseIPreprocessBuildWithReport)
            {
                return;
            }

            if (Monitor.Settings.IsEditorOnly)
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

        #region Fields

        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;
        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private readonly string preserveAttribute = $"[{typeof(PreserveAttribute).FullName}]";
        private readonly string methodImpAttribute = $"[{typeof(MethodImplAttribute).FullName}({typeof(MethodImplOptions).FullName}.{MethodImplOptions.NoOptimization.ToString()})]";

        private readonly string typeDefField = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefField)}";
        private readonly string typeDefProperty = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefProperty)}";
        private readonly string typeDefEvent = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefEvent)}";
        private readonly string typeDefMethod = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefMethod)}";
        private readonly string typeDefOutParameter = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefOutParameter)}";

        private readonly string typeDefStructArray = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefStructTypeArray)}";
        private readonly string typeDefArray = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefArray)}";
        private readonly string typeDefDictionary = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefDictionary)}";
        private readonly string typeDefEnumerable = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefEnumerable)}";
        private readonly string typeDefList = $"{typeof(IL2CPPTypeDefinitions).FullName}.{nameof(IL2CPPTypeDefinitions.TypeDefList)}";

        private readonly UnityEditor.Compilation.Assembly[] unityAssemblies;

        private List<Exception> ExceptionBuffer { get; } = new List<Exception>();
        private TypeBuffer TypeDefFieldBuffer { get; } = new TypeBuffer("  [Field Definitions] ");
        private TypeBuffer TypeDefPropertyBuffer { get; } = new TypeBuffer("  [Property Definitions] ");
        private TypeBuffer TypeDefEventBuffer { get; } = new TypeBuffer("  [Event Definitions] ");
        private TypeBuffer TypeDefMethodBuffer { get; } = new TypeBuffer("  [Method Definitions] ");
        private TypeBuffer TypeDefOutParameterBuffer { get; } = new TypeBuffer("  [Out Parameter Definitions] ");
        private TypeBuffer TypeDefCollectionBuffer { get; } = new TypeBuffer("  [Collection Definitions] ");

        private class TypeBuffer
        {
            public TypeBuffer(string name)
            {
                Name = name;
            }

            public readonly string Name;

            public readonly List<string> Definitions = new List<string>();
            public List<string> GetComments(string typeDef) => _definitions[typeDef];

            private readonly Dictionary<string, List<string>> _definitions = new Dictionary<string, List<string>>();
            public void Add(string typeDef, string comment)
            {
                if (_definitions.TryGetValue(typeDef, out var commentsList))
                {
                    commentsList.AddUnique($" {comment}");
                }
                else
                {
                    Definitions.Add(typeDef);
                    _definitions.Add(typeDef, new List<string> {$" {comment}"});
                }
            }
        }

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

        #region Setup

        public IL2CPPBuildPreprocessor()
        {
            unityAssemblies = CompilationPipeline.GetAssemblies();
            assemblies = GetFilteredAssemblies();
        }

        private void OnPreprocessBuildInternal()
        {
            Debug.Log($"[Monitoring] Generating Type Definitions for IL2CPP");

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

            WriteContentToFile(stringBuilder);

            if (ExceptionBuffer.Any())
            {
                foreach (var exception in ExceptionBuffer)
                {
                    Debug.LogException(exception);
                }

                throw new OperationCanceledException("Cancel Build Process");
            }
            Debug.Log($"[Monitoring] Created Type Definitions for IL2CPP");
        }

        internal static void GenerateIL2CPPAheadOfTimeTypes()
        {
            new IL2CPPBuildPreprocessor().OnPreprocessBuildInternal();
        }

        private void WriteContentToFile(StringBuilder stringBuilder)
        {
            var textFile = Monitor.Settings.TypeDefinitionsForIL2CPP;
            var filePath = textFile
                ? AssetDatabase.GetAssetPath(textFile)
                : Application.dataPath + "/Baracuda/IL2CPP/TYPE_DEFINITIONS_FOR_IL2CPP.cs";

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
            File.WriteAllText(filePath, stringBuilder.ToString());
            AssetDatabase.Refresh();
        }

        #endregion

        #region Profile Assmelby

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

        #region Profiling

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

        #region Profile Types

        private void ProfileFieldInfo(FieldInfo fieldInfo)
        {
            try
            {
                Assert.IsNotNull(fieldInfo.DeclaringType);

                var declaring = fieldInfo.DeclaringType.IsValueType ? typeof(ValueType) : fieldInfo.DeclaringType;
                var monitored = fieldInfo.FieldType.GetUnderlying();

                CheckCollectionType(monitored);

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
                    ? declaring.AsObjectPointer()
                    : declaring.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored.AsObjectPointer()
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefField}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(fieldInfo.IsStatic ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Fields", "MemberInfo");
                Stats.IncrementStat($"Monitored Fields {(fieldInfo.IsStatic ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

                TypeDefFieldBuffer.Add(typeDef, fieldInfo.GetDescription());
            }
            catch (Exception exception)
            {
                ExceptionBuffer.Add(exception);
            }
        }

        private void ProfilePropertyInfo(PropertyInfo propertyInfo)
        {
            try
            {
                Assert.IsNotNull(propertyInfo.DeclaringType);

                var declaring = propertyInfo.DeclaringType.IsValueType ? typeof(ValueType) : propertyInfo.DeclaringType;
                var monitored = propertyInfo.PropertyType.GetUnderlying();

                CheckCollectionType(monitored);

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
                    ? declaring.AsObjectPointer()
                    : declaring.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored.AsObjectPointer()
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefProperty}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(propertyInfo.IsStatic() ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Properties", "MemberInfo");
                Stats.IncrementStat($"Monitored Properties {(propertyInfo.IsStatic() ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");
                TypeDefPropertyBuffer.Add(typeDef, propertyInfo.GetDescription());
            }
            catch (Exception exception)
            {
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

                CheckCollectionType(monitored);

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
                    ? declaring.AsObjectPointer()
                    : declaring.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored.AsDelegatePointer()
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefEvent}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(eventInfo.IsStatic() ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Events", "MemberInfo");
                Stats.IncrementStat($"Monitored Events {(eventInfo.IsStatic() ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

                TypeDefEventBuffer.Add(typeDef, eventInfo.GetDescription());
            }
            catch (Exception exception)
            {
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

                CheckCollectionType(monitored);

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
                ExceptionBuffer.Add(exception);
            }

            void TypeDefVoid(MethodInfo method, Type declaring)
            {
                var usableDeclaring = (declaring.IsAccessible() && !declaring.IsStatic())
                    ? declaring.AsObjectPointer()
                    : declaring.GetReplacement();

                var typeDef = $"{typeDefMethod}<{usableDeclaring.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(method.IsStatic ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Methods", "MemberInfo");
                Stats.IncrementStat($"Monitored Methods {(method.IsStatic ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored void", "Monitored Types");

                TypeDefMethodBuffer.Add(typeDef, method.GetDescription());
            }

            void TypeDefWithReturnValue(MethodInfo method, Type type, Type monitored)
            {
                var usableDeclaring = (type.IsAccessible() && !type.IsStatic())
                    ? type.AsObjectPointer()
                    : type.GetReplacement();

                var usableMonitored = (monitored.IsAccessible() && !monitored.IsStatic())
                    ? monitored.AsObjectPointer()
                    : monitored.GetReplacement();

                var typeDef = $"{typeDefMethod}<{usableDeclaring.ToTypeDefString()}, {usableMonitored.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(method.IsStatic ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Methods", "MemberInfo");
                Stats.IncrementStat($"Monitored Methods {(method.IsStatic ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

                TypeDefMethodBuffer.Add(typeDef, method.GetDescription());
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
                    ? type.AsObjectPointer()
                    : type.GetReplacement();

                CheckCollectionType(usableType);

                var typeDef = $"{typeDefOutParameter}<{usableType.ToTypeDefString()}>();";

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(usableType.IsStatic() ? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Out Parameter", "MemberInfo");
                Stats.IncrementStat($"Monitored Out Parameter {(usableType.IsStatic() ? "Static" : "Instance")}", "MemberInfo");
                Stats.IncrementStat($"Monitored {type.HumanizedName()}", "Monitored Types");

                TypeDefOutParameterBuffer.Add(typeDef, parameterInfo.GetDescription());
            }
            catch (Exception exception)
            {
                ExceptionBuffer.Add(exception);
            }
        }

        #endregion

        #region Profile Collections

        private void CheckCollectionType(Type type)
        {
            try
            {
                if (type.IsValueTypeArray())
                {
                    ProcessStructArray(type);
                }
                if (type.IsArray)
                {
                    ProcessArray(type);
                }
                if (type.IsGenericIDictionary())
                {
                    ProcessDictionary(type);
                }
                if (type.IsGenericIList())
                {
                    ProcessList(type);
                }
                if (type.IsGenericIEnumerable())
                {
                    ProcessEnumerable(type);
                }
            }
            catch (Exception exception)
            {
                ExceptionBuffer.Add(exception);
            }

            void ProcessList(Type listType)
            {
                var usableType = listType.GetGenericArguments()[0].AsObjectPointer();
                if (usableType.IsAccessible())
                {
                    var typeDef = $"{typeDefList}<{usableType.ToTypeDefString()}>();";
                    TypeDefCollectionBuffer.Add(typeDef, listType.ToTypeDefString());
                }
                else
                {
                    Debug.LogWarning($"[Monitoring] The monitored type {listType.ToTypeDefString()} is not accessible! \n" +
                                     $"Make sure to use the TypeDefAttribute to create an internal type definition for IL2CPP.");
                }
            }

            void ProcessStructArray(Type arrayType)
            {
                var usableType = arrayType.GetElementType();
                if (usableType.IsAccessible())
                {
                    var typeDef = $"{typeDefStructArray}<{usableType.ToTypeDefString()}>();";
                    TypeDefCollectionBuffer.Add(typeDef, arrayType.ToTypeDefString());
                }
                else
                {
                    Debug.LogWarning($"[Monitoring] The monitored type {arrayType.ToTypeDefString()} is not accessible! \n" +
                                     $"Make sure to use the TypeDefAttribute to create an internal type definition for IL2CPP.");
                }
            }

            void ProcessArray(Type arrayType)
            {
                var usableType = arrayType.GetElementType().AsObjectPointer();
                if (usableType.IsAccessible())
                {
                    var typeDef = $"{typeDefArray}<{usableType.ToTypeDefString()}>();";
                    TypeDefCollectionBuffer.Add(typeDef, arrayType.ToTypeDefString());
                }
                else
                {
                    Debug.LogWarning($"[Monitoring] The monitored type {arrayType.ToTypeDefString()} is not accessible! \n" +
                                     $"Make sure to use the TypeDefAttribute to create an internal type definition for IL2CPP.");
                }
            }

            void ProcessDictionary(Type dictionaryType)
            {
                var usableKeyType = dictionaryType.GetGenericArguments()[0].AsObjectPointer();
                var usableValueType = dictionaryType.GetGenericArguments()[1].AsObjectPointer();
                if (!usableKeyType.IsAccessible())
                {
                    Debug.LogWarning(
                        $"[Monitoring] The monitored type {dictionaryType.ToTypeDefString()} is not accessible! \n" +
                        $"Make sure to use the TypeDefAttribute to create an internal type definition for IL2CPP.");
                    return;
                }
                if (!usableValueType.IsAccessible())
                {
                    Debug.LogWarning(
                        $"[Monitoring] The monitored type {dictionaryType.ToTypeDefString()} is not accessible! \n" +
                        $"Make sure to use the TypeDefAttribute to create an internal type definition for IL2CPP.");
                    return;
                }
                var typeDef =
                    $"{typeDefDictionary}<{usableKeyType.ToTypeDefString()}, {usableValueType.ToTypeDefString()}>();";
                TypeDefCollectionBuffer.Add(typeDef, dictionaryType.ToTypeDefString());
            }

            void ProcessEnumerable(Type enumerableType)
            {
                var usableType = enumerableType.GetGenericArguments()[0].AsObjectPointer();
                if (usableType.IsAccessible())
                {
                    var typeDef = $"{typeDefEnumerable}<{usableType.ToTypeDefString()}>();";
                    TypeDefCollectionBuffer.Add(typeDef, enumerableType.ToTypeDefString());
                }
                else
                {
                    Debug.LogWarning($"[Monitoring] The monitored type {enumerableType.ToTypeDefString()} is not accessible! \n" +
                                     $"Make sure to use the TypeDefAttribute to create an internal type definition for IL2CPP.");
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Append Definitions

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

            TypeDefFieldBuffer.Definitions.Sort();
            TypeDefPropertyBuffer.Definitions.Sort();
            TypeDefEventBuffer.Definitions.Sort();
            TypeDefMethodBuffer.Definitions.Sort();
            TypeDefOutParameterBuffer.Definitions.Sort();
            TypeDefCollectionBuffer.Definitions.Sort();

            AppendBuffer(stringBuilder, TypeDefFieldBuffer);
            AppendBuffer(stringBuilder, TypeDefPropertyBuffer);
            AppendBuffer(stringBuilder, TypeDefEventBuffer);
            AppendBuffer(stringBuilder, TypeDefMethodBuffer);
            AppendBuffer(stringBuilder, TypeDefOutParameterBuffer);
            AppendBuffer(stringBuilder, TypeDefCollectionBuffer);
            stringBuilder.Append("    }");
        }

        private void AppendBuffer(StringBuilder stringBuilder, TypeBuffer buffer)
        {
            if (buffer.Definitions.Any())
            {
                AppendComment(stringBuilder, buffer.Name, 8);
                var definitions = buffer.Definitions;
                for (var i = 0; i < definitions.Count; i++)
                {
                    var definition = definitions[i];
                    stringBuilder.Append('\n');
                    foreach (var comment in buffer.GetComments(definition))
                    {
                        AppendComment(stringBuilder, comment, 8);
                    }
                    stringBuilder.Append('\n');
                    stringBuilder.Append(new string(' ', 8));
                    stringBuilder.Append(definition);
                }
                stringBuilder.Append('\n');
            }
        }

        #endregion

        #region Append Meta

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
            stringBuilder.Append("//---------- -------------------------- -------\n");
            stringBuilder.Append("//---------- !!! AUTOGENERATED CONTENT !!! -------\n");
            stringBuilder.Append("//---------- -------------------------- -------\n");
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
            stringBuilder.Append("internal class IL2CPP_AOT_");

            stringBuilder.Append(RandomString(20));
            stringBuilder.Append('\n');
            stringBuilder.Append('{');

            string RandomString(int length)
            {
                var random = new System.Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        private void AppendCloseClass(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append('}');
            stringBuilder.Append('\n');
        }

        #endregion

        #region Append Stats

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

        #region Append Misc

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
