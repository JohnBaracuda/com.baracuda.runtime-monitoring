// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.IL2CPP;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Types;
using Baracuda.Utilities.Extensions;
using Baracuda.Utilities.Pooling;
using Baracuda.Utilities.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Scripting;
using Assembly = System.Reflection.Assembly;

namespace Baracuda.Monitoring.Editor
{
    public class IL2CPPBuildPreprocessor : IPreprocessBuildWithReport
    {
        #region --- Public API ---

        /// <summary>
        /// Call this method to manually generate AOT types fort IL2CPP scripting backend.
        /// You can set the filepath of the target script file in the monitoring settings.
        /// </summary>
        public static void GenerateIL2CPPAheadOfTimeTypes()
        {
#if !DISABLE_MONITORING
            OnPreprocessBuildInternal();
#endif
        }

        #endregion

        #region --- Interface ---

        public int callbackOrder => MonitoringSystems.Resolve<IMonitoringSettings>().PreprocessBuildCallbackOrder;

        public void OnPreprocessBuild(BuildReport report)
        {
#if !DISABLE_MONITORING
            if (!MonitoringSystems.Resolve<IMonitoringSettings>().UseIPreprocessBuildWithReport)
            {
                return;
            }

            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            if (PlayerSettings.GetScriptingBackend(group) == ScriptingImplementation.IL2CPP)
            {
                OnPreprocessBuildInternal();
            }
#endif
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields ---

        /*
         * Const fields
         */

        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public |
                                                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly;


        private static readonly string preserveAttribute = $"[{typeof(PreserveAttribute).FullName}]";
        private static readonly string methodImpAttribute = $"[{typeof(MethodImplAttribute).FullName}({typeof(MethodImplOptions).FullName}.{MethodImplOptions.NoOptimization.ToString()})]";
        private static readonly string aotBridgeClass = typeof(AOTBridge).FullName;

        /*
         * Queues & Caches
         */

        private static readonly List<string> fieldProfileDefinitions = new List<string>();
        private static readonly List<string> propertyProfileDefinitions = new List<string>();
        private static readonly List<string> eventProfileDefinitions = new List<string>();
        private static readonly List<string> methodProfileDefinitions = new List<string>();

        private static readonly HashSet<Type> uniqueTypeDefinitions = new HashSet<Type>();
        private static readonly HashSet<Type> uniqueMonitoredTypes = new HashSet<Type>();
        private static readonly List<string> signatureDefinitions = new List<string>();

        private static readonly List<string> errorBuffer = new List<string>();

        private static int id;

        private static IStatCounter Stats { get; set; }

        #endregion

        #region --- Preprocess Internal ---

        private static void OnPreprocessBuildInternal()
        {
            ResetQueuesAndCaches();
            unityAssemblies = CompilationPipeline.GetAssemblies();

            var textFile = MonitoringSystems.Resolve<IMonitoringSettings>().ScriptFileIL2CPP;
            var filePath = AssetDatabase.GetAssetPath(textFile);

            Debug.Log($"Starting IL2CPP AOT type definition generation.\nFilePath: {filePath}");

            ProfileAssembliesAndCreateDefinitions();

            var stringBuilder = new StringBuilder(short.MaxValue);

            AppendHeaderText(stringBuilder);
            AppendIfDefBegin(stringBuilder);
            AppendOpenClass(stringBuilder);

            AppendProfileDefinitions(stringBuilder, fieldProfileDefinitions, "Field Profiles");
            AppendProfileDefinitions(stringBuilder, propertyProfileDefinitions, "Property Profiles");
            AppendProfileDefinitions(stringBuilder, eventProfileDefinitions, "Event Profiles");
            AppendProfileDefinitions(stringBuilder, methodProfileDefinitions, "Method Profiles");

            AppendMethodDefinitions(stringBuilder);

            AppendCloseClass(stringBuilder);
            AppendIfDefEnd(stringBuilder);

            AppendStats(stringBuilder);

            Debug.Log($"Writing type definitions to file.\nFilePath: {filePath}");

            WriteContentToFile(filePath, stringBuilder);

            if (errorBuffer.Any())
            {
                foreach (var errorMessage in errorBuffer)
                {
                    Debug.LogWarning(errorMessage);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Successfully completed IL2CPP AOT type definition generation");

            if (MonitoringSystems.Resolve<IMonitoringSettings>().LogTypeGenerationStats)
            {
                Debug.Log(Stats.ToString(false));
            }

            ResetQueuesAndCaches();
        }

        private static void WriteContentToFile(string filePath, StringBuilder stringBuilder)
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

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Append Definitions ---

        private static void AppendProfileDefinitions(StringBuilder stringBuilder, IReadOnlyList<string> definitions, string title)
        {
            AppendComment(stringBuilder, title);
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                stringBuilder.Append("\n    ");
                stringBuilder.Append(definition);
            }
            AppendLineBreak(stringBuilder);
            AppendLineBreak(stringBuilder);
            AppendLine(stringBuilder);
        }

        private static void AppendMethodDefinitions(StringBuilder stringBuilder)
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

            for (var i = 0; i < signatureDefinitions.Count; i++)
            {
                var definition = signatureDefinitions[i];
                stringBuilder.Append(definition);
            }

            stringBuilder.Append("\n    ");
            stringBuilder.Append("}");
        }

        #endregion

        #region --- Append Meta ---

        private static void AppendHeaderText(StringBuilder stringBuilder)
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

        private static void AppendCopyrightNote(StringBuilder stringBuilder)
        {
            stringBuilder.Append("// Copyright (c) 2022 Jonathan Lang\n");
        }

        private static void AppendAutogeneratedMessage(StringBuilder stringBuilder)
        {
            stringBuilder.Append("//---------- ----------------------------- ----------\n");
            stringBuilder.Append("//---------- !!! AUTOGENERATED CONTENT !!! ----------\n");
            stringBuilder.Append("//---------- ----------------------------- ----------\n");
        }

        private static void AppendIfDefBegin(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append("#if ENABLE_IL2CPP && !DISABLE_MONITORING");
            stringBuilder.Append('\n');
        }

        private static void AppendIfDefEnd(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append("#endif //ENABLE_IL2CPP && !DISABLE_MONITORING");
            stringBuilder.Append('\n');
        }

        private static void AppendOpenClass(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append("internal class IL2CPP_AOT");
            stringBuilder.Append('\n');
            stringBuilder.Append('{');
        }

        private static void AppendCloseClass(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append('}');
            stringBuilder.Append('\n');
        }

        #endregion

        #region --- Append Stats ---

        private static void AppendStats(StringBuilder stringBuilder)
        {
            AppendComment(stringBuilder, new string('-', 118), 0);
            AppendLineBreak(stringBuilder, 2);
            stringBuilder.Append(Stats.ToString(true));
            AppendComment(stringBuilder, new string('-', 118), 0);
            AppendLineBreak(stringBuilder);
            AppendComment(stringBuilder, "If this file contains any errors please contact me and/or create an issue in the linked repository." , 0);
            AppendComment(stringBuilder, "https://github.com/JohnBaracuda/Runtime-Monitoring" , 0);
        }

        #endregion

        #region --- Append Misc ---

        private static void AppendLineBreak(StringBuilder stringBuilder, int breaks = 1)
        {
            for (var i = 0; i < breaks; i++)
            {
                stringBuilder.Append('\n');
            }
        }

        private static void AppendComment(StringBuilder stringBuilder, string comment, int indent = 4)
        {
            stringBuilder.Append('\n');
            stringBuilder.Append(new string(' ', indent));
            stringBuilder.Append("//");
            stringBuilder.Append(comment);
        }

        private static void AppendLine(StringBuilder stringBuilder)
        {
            stringBuilder.Append("    //");
            stringBuilder.Append(new string('-', 114));
            stringBuilder.Append('\n');
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Profiling ---

        private static void ProfileAssembliesAndCreateDefinitions()
        {
            foreach (var filteredAssembly in AssemblyProfiler.GetFilteredAssemblies())
            {
                if (IsEditorAssembly(filteredAssembly))
                {
                    continue;
                }

                foreach (var type in filteredAssembly.GetTypes())
                {
                    // Static Fields
                    foreach (var fieldInfo in type.GetFields(StaticFlags))
                    {
                        if (fieldInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileFieldInfo(fieldInfo);
                        }
                    }
                    // Instance Fields
                    foreach (var fieldInfo in type.GetFields(InstanceFlags))
                    {
                        if (fieldInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileFieldInfo(fieldInfo);
                        }
                    }

                    // Static Properties
                    foreach (var propertyInfo in type.GetProperties(StaticFlags))
                    {
                        if (propertyInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfilePropertyInfo(propertyInfo);
                        }
                    }
                    // Instance Properties
                    foreach (var propertyInfo in type.GetProperties(InstanceFlags))
                    {
                        if (propertyInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfilePropertyInfo(propertyInfo);
                        }
                    }

                    // Static Events
                    foreach (var eventInfo in type.GetEvents(StaticFlags))
                    {
                        if (eventInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileEventInfo(eventInfo);
                        }
                    }
                    // Instance Events
                    foreach (var eventInfo in type.GetEvents(InstanceFlags))
                    {
                        if (eventInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileEventInfo(eventInfo);
                        }
                    }

                    // Static Methods
                    foreach (var methodInfo in type.GetMethods(StaticFlags))
                    {
                        if (methodInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileMethodInfo(methodInfo);
                        }
                    }
                    // Instance Methods
                    foreach (var methodInfo in type.GetMethods(InstanceFlags))
                    {
                        if (methodInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileMethodInfo(methodInfo);
                        }
                    }
                }
            }
        }

        #endregion

        #region --- Profile MemberInfor ---

        private static void ProfileFieldInfo(FieldInfo fieldInfo)
        {
            try
            {
                var template = typeof(FieldProfile<,>);
                var declaring = fieldInfo.DeclaringType;
                var monitored = fieldInfo.FieldType;
                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(fieldInfo.IsStatic? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Fields", "MemberInfo");
                Stats.IncrementStat($"Monitored Fields {(fieldInfo.IsStatic? "Static" : "Instance")}", "MemberInfo");
                CreateProfileTypeDefFor(template, declaring, monitored, fieldProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSystems.Resolve<IMonitoringSettings>().ThrowOnTypeGenerationError)
                {
                    throw;
                }
                errorBuffer.Add(exception.Message);
            }
        }

        private static void ProfilePropertyInfo(PropertyInfo propertyInfo)
        {
            try
            {
                var template = typeof(PropertyProfile<,>);
                var declaring = propertyInfo.DeclaringType;
                var monitored = propertyInfo.PropertyType;
                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(propertyInfo.IsStatic()? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Properties", "MemberInfo");
                Stats.IncrementStat($"Monitored Properties {(propertyInfo.IsStatic()? "Static" : "Instance")}", "MemberInfo");
                CreateProfileTypeDefFor(template, declaring, monitored, propertyProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSystems.Resolve<IMonitoringSettings>().ThrowOnTypeGenerationError)
                {
                    throw;
                }
                errorBuffer.Add(exception.Message);
            }
        }

        private static void ProfileEventInfo(EventInfo eventInfo)
        {
            try
            {
                var template = typeof(EventProfile<,>);
                var declaring = eventInfo.DeclaringType;
                var monitored = eventInfo.EventHandlerType;
                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(eventInfo.IsStatic()? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Events", "MemberInfo");
                Stats.IncrementStat($"Monitored Events {(eventInfo.IsStatic()? "Static" : "Instance")}", "MemberInfo");
                CreateProfileTypeDefFor(template, declaring, monitored, eventProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSystems.Resolve<IMonitoringSettings>().ThrowOnTypeGenerationError)
                {
                    throw;
                }
                errorBuffer.Add(exception.Message);
            }
        }

        private static void ProfileMethodInfo(MethodInfo methodInfo)
        {
            try
            {
                if (!methodInfo.HasReturnValueOrOutParameter())
                {
                    Debug.LogWarning($"Monitored Method {methodInfo.DeclaringType?.Name}.{methodInfo.Name} needs a return value or out parameter!");
                    return;
                }

                Stats.IncrementStat("Monitored Member");
                Stats.IncrementStat($"Monitored Member {(methodInfo.IsStatic? "Static" : "Instance")}");
                Stats.IncrementStat("Monitored Methods", "MemberInfo");
                Stats.IncrementStat($"Monitored Methods {(methodInfo.IsStatic? "Static" : "Instance")}", "MemberInfo");

                var template = typeof(MethodProfile<,>);
                var declaring = methodInfo.DeclaringType;
                var monitored = methodInfo.ReturnType;
                CreateProfileTypeDefFor(template, declaring, monitored, methodProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSystems.Resolve<IMonitoringSettings>().ThrowOnTypeGenerationError)
                {
                    throw;
                }
                errorBuffer.Add(exception.Message);
            }

            foreach (var parameterInfo in methodInfo.GetParameters().Where(info => info.IsOut))
            {
                try
                {
                    Stats.IncrementStat("Monitored Out Parameter", "Out Parameter");
                    Stats.IncrementStat($"Monitored Out Parameter {parameterInfo.ParameterType.HumanizedName()}", "Out Parameter");
                    TryCreateOutParameterHandleDefinition(parameterInfo.ParameterType);
                }
                catch (Exception exception)
                {
                    if (MonitoringSystems.Resolve<IMonitoringSettings>().ThrowOnTypeGenerationError)
                    {
                        throw;
                    }
                    errorBuffer.Add(exception.Message);
                }
            }
        }

        #endregion

        #region --- Type Definitions ---

        /*
         * Out Param Type Def
         */

        private static void TryCreateOutParameterHandleDefinition(Type type)
        {
            var underlying = MakeViableType(type);
            if (underlying == null)
            {
                return;
            }
            var concreteType = typeof(OutParameterHandleT<>).MakeGenericType(underlying);

            if (TryCreateUniqueTypeDefString(concreteType, out var str))
            {
                methodProfileDefinitions.Add(str);
            }
        }


        /*
         * Profile Type Def
         */

        private static void CreateProfileTypeDefFor(Type template, Type declaringType, Type monitoredType, in ICollection<string> definitionList)
        {
            if (monitoredType.IsGenericParameter)
            {
                return;
            }

            var declaring = MakeViableType(declaringType);
            var monitored = MakeViableType(monitoredType);

            if (declaring == null || monitored == null)
            {
                return;
            }


            CreateMethodSig(monitored);
            Stats.IncrementStat($"Monitored {monitored.HumanizedName()}", "Monitored Types");

            var definition = template.MakeGenericType(declaring, monitored);

            if (TryCreateUniqueTypeDefString(definition, out var str))
            {
                definitionList.Add(str);
            }
        }

        /*
         * Create Definition String
         */

        private static bool TryCreateUniqueTypeDefString(Type type, out string defString)
        {
            if (uniqueTypeDefinitions.Contains(type))
            {
                defString = null;
                return false;
            }

            uniqueTypeDefinitions.Add(type);
            defString = CreateTypeDefinitionString(type);
            return true;
        }

        private static string CreateTypeDefinitionString(Type type)
        {
            var stringBuilder = StringBuilderPool.Get();
            stringBuilder.Append("\n    //");
            stringBuilder.Append(type.HumanizedName());
            stringBuilder.Append("\n    ");
            stringBuilder.Append(preserveAttribute);
            stringBuilder.Append("\n    ");
            stringBuilder.Append(MakeAccessibleSyntaxString(type));
            stringBuilder.Append(' ');
            stringBuilder.Append("AOT_GENERATED_TYPE_");
            stringBuilder.Append(id++);
            stringBuilder.Append(';');
            return StringBuilderPool.Release(stringBuilder);
        }

        #endregion

        #region --- Method Signature ---

        private static void CreateMethodSig(Type type)
        {
            var viableType = MakeViableType(type);
            if (viableType == null)
            {
                return;
            }
            if (uniqueMonitoredTypes.Contains(viableType))
            {
                return;
            }

            uniqueMonitoredTypes.Add(viableType);

            if (viableType.IsValueTypeArray())
            {
                ProcessValueTypeArray(type);
            }
            if (viableType.IsArray)
            {
                ProcessArray(type);
            }
            if (viableType.IsGenericIDictionary())
            {
                ProcessDictionary(type);
            }
            if (viableType.IsGenericIEnumerable(true))
            {
                ProcessEnumerable(type);
            }
            if (viableType.IsGenericIList())
            {
                ProcessList(type);
            }

            void ProcessList(Type valueType)
            {
                var stringBuilder = StringBuilderPool.Get();
                stringBuilder.Append("\n        ");
                stringBuilder.Append(aotBridgeClass);
                stringBuilder.Append('.');
                stringBuilder.Append("AOTList");
                stringBuilder.Append('<');
                stringBuilder.Append(MakeAccessibleSyntaxString(valueType));
                stringBuilder.Append(", ");
                stringBuilder.Append(MakeAccessibleSyntaxString(valueType.GetGenericArguments()[0]));
                stringBuilder.Append(">();");
                signatureDefinitions.Add(StringBuilderPool.Release(stringBuilder));
            }

            void ProcessValueTypeArray(Type valueType)
            {
                var stringBuilder = StringBuilderPool.Get();
                stringBuilder.Append("\n        ");
                stringBuilder.Append(aotBridgeClass);
                stringBuilder.Append('.');
                stringBuilder.Append("AOTValueTypeArray");
                stringBuilder.Append('<');
                stringBuilder.Append(MakeAccessibleSyntaxString(valueType.GetElementType()));
                stringBuilder.Append(">();");
                signatureDefinitions.Add(StringBuilderPool.Release(stringBuilder));
            }

            void ProcessArray(Type arrayType)
            {
                var stringBuilder = StringBuilderPool.Get();
                stringBuilder.Append("\n        ");
                stringBuilder.Append(aotBridgeClass);
                stringBuilder.Append('.');
                stringBuilder.Append("AOTReferenceTypeArray");
                stringBuilder.Append('<');
                stringBuilder.Append(MakeAccessibleSyntaxString(arrayType.GetElementType()));
                stringBuilder.Append(">();");
                signatureDefinitions.Add(StringBuilderPool.Release(stringBuilder));
            }

            void ProcessDictionary(Type dictionaryType)
            {
                var stringBuilder = StringBuilderPool.Get();
                stringBuilder.Append("\n        ");
                stringBuilder.Append(aotBridgeClass);
                stringBuilder.Append('.');
                stringBuilder.Append("AOTDictionary");
                stringBuilder.Append('<');
                stringBuilder.Append(MakeAccessibleSyntaxString(dictionaryType.GetGenericArguments()[0]));
                stringBuilder.Append(',');
                stringBuilder.Append(MakeAccessibleSyntaxString(dictionaryType.GetGenericArguments()[1]));
                stringBuilder.Append(">();");
                signatureDefinitions.Add(StringBuilderPool.Release(stringBuilder));
            }

            void ProcessEnumerable(Type enumerableType)
            {
                var stringBuilder = StringBuilderPool.Get();
                stringBuilder.Append("\n        ");
                stringBuilder.Append(aotBridgeClass);
                stringBuilder.Append('.');
                stringBuilder.Append("AOTEnumerable");
                stringBuilder.Append('<');
                stringBuilder.Append(MakeAccessibleSyntaxString(enumerableType.GetGenericArguments()[0]));
                stringBuilder.Append(">();");
                signatureDefinitions.Add(StringBuilderPool.Release(stringBuilder));
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Misc ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Type MakeViableType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var underlying = (type.IsByRef ? type.GetElementType() : type) ?? type;

            if (underlying == typeof(void))
            {
                return typeof(VoidValue);
            }

            if (underlying.IsReadonlyRefStruct())
            {
                return null;
            }

            if (underlying.IsAccessible())
            {
                return underlying;
            }

            if (underlying.IsClass)
            {
                return typeof(object);
            }

            if (underlying.IsEnum)
            {
                switch (Marshal.SizeOf(Enum.GetUnderlyingType(type)))
                {
                    case 1:
                        return typeof(Enum8);
                    case 2:
                        return typeof(Enum16);
                    case 4:
                        return typeof(Enum32);
                    case 8:
                        return typeof(Enum64);
                }
            }

            var error =
                $"[MONITORING] Error: {type.HumanizedName()} is not accessible! ({type.FullName?.Replace('+', '.')})" +
                $"\nCannot generate AOT code for unmanaged internal/private types! " +
                $"Please make sure that {type.HumanizedName()} and all of its declaring types are either public or use a managed type instead of struct!";

            errorBuffer.Add(error);

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MakeAccessibleSyntaxString(Type type)
        {
            if (type.IsStatic())
            {
                return typeof(object).FullName?.Replace('+', '.');
            }

            if (!type.IsAccessible())
            {
                return typeof(object).FullName?.Replace('+', '.');
            }

            if (type.IsGenericType)
            {
                var builder = ConcurrentStringBuilderPool.Get();
                var argBuilder = ConcurrentStringBuilderPool.Get();

                var arguments = type.GetGenericArguments();

                foreach (var typeArg in arguments)
                {
                    // Let's make sure we get the argument list.
                    var arg = MakeAccessibleSyntaxString(typeArg);

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

                ConcurrentStringBuilderPool.ReleaseStringBuilder(builder);
                ConcurrentStringBuilderPool.ReleaseStringBuilder(argBuilder);
                return retType.Replace('+', '.');
            }

            Debug.Assert(type.FullName != null, $"type.FullName != null | {type.Name}, {type.DeclaringType}");

            var returnValue = type.FullName.Replace('+', '.');
            return returnValue;
        }


        private static void ResetQueuesAndCaches()
        {
            id = 0;

            fieldProfileDefinitions.Clear();
            propertyProfileDefinitions.Clear();
            eventProfileDefinitions.Clear();
            methodProfileDefinitions.Clear();

            signatureDefinitions.Clear();
            uniqueTypeDefinitions.Clear();
            uniqueMonitoredTypes.Clear();

            errorBuffer.Clear();

            Stats = new StatCounter();
        }

        #endregion

        #region --- Editor Misc ---

        private static UnityEditor.Compilation.Assembly[] unityAssemblies;

        public static bool IsEditorAssembly(Assembly assembly)
        {
            var editorAssemblies = unityAssemblies;

            for (var i = 0; i < editorAssemblies.Length; i++)
            {
                var unityAssembly = editorAssemblies[i];

                if (unityAssembly.name != assembly.GetName().Name)
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
        #endregion
    }
}