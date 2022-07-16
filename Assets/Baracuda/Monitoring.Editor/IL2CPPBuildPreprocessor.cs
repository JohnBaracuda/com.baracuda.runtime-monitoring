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
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Scripting;

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

        public int callbackOrder => MonitoringSettings.GetInstance().PreprocessBuildCallbackOrder;

        public void OnPreprocessBuild(BuildReport report)
        {
#if !DISABLE_MONITORING
            if (!MonitoringSettings.GetInstance().UseIPreprocessBuildWithReport)
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

        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public |
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
        
        #endregion
        
        #region --- Preprocess Internal ---

        private static void OnPreprocessBuildInternal()
        {
            ClearCaches();
            
            var textFile = MonitoringSettings.GetInstance().ScriptFileIL2CPP;
            var filePath = AssetDatabase.GetAssetPath(textFile);
            
            Debug.Log($"Starting IL2CPP AOT Type Definition Generation.\nFilePath: {filePath}");

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
            
            //---
            
            WriteContentToFile(filePath, stringBuilder);

            if (errorBuffer.Any())
            {
                foreach (var errorMessage in errorBuffer)
                {
                    Debug.LogWarning(errorMessage);
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Successfully Completed IL2CPP AOT Type Definition Generation");
            
            ClearCaches();
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
            stringBuilder.Append("//---------- ----------------------------- ----------\n");
            stringBuilder.Append("//---------- !!! AUTOGENERATED CONTENT !!! ----------\n");
            stringBuilder.Append("//---------- ----------------------------- ----------\n");

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
            stringBuilder.Append("//https://github.com/JohnBaracuda/Runtime-Monitoring");
            stringBuilder.Append('\n');
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

        #region --- Append Misc ---
        
        private static void AppendLineBreak(StringBuilder stringBuilder)
        {
            stringBuilder.Append('\n');
        }
        
        private static void AppendComment(StringBuilder stringBuilder, string comment)
        {
            stringBuilder.Append("\n    //");
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
                if (filteredAssembly.IsEditorAssembly())
                {
                    continue;
                }
                
                foreach (var type in filteredAssembly.GetTypes())
                {
                    // Static Fields
                    foreach (var fieldInfo in type.GetFields(STATIC_FLAGS))
                    {
                        if (fieldInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileFieldInfo(fieldInfo);
                        }
                    }
                    // Instance Fields
                    foreach (var fieldInfo in type.GetFields(INSTANCE_FLAGS))
                    {
                        if (fieldInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileFieldInfo(fieldInfo);
                        }
                    }
                    
                    // Static Properties
                    foreach (var propertyInfo in type.GetProperties(STATIC_FLAGS))
                    {
                        if (propertyInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfilePropertyInfo(propertyInfo);
                        }
                    }
                    // Instance Properties
                    foreach (var propertyInfo in type.GetProperties(INSTANCE_FLAGS))
                    {
                        if (propertyInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfilePropertyInfo(propertyInfo);
                        }
                    }
                    
                    // Static Events
                    foreach (var eventInfo in type.GetEvents(STATIC_FLAGS))
                    {
                        if (eventInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileEventInfo(eventInfo);
                        }
                    }
                    // Instance Events
                    foreach (var eventInfo in type.GetEvents(INSTANCE_FLAGS))
                    {
                        if (eventInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileEventInfo(eventInfo);
                        }
                    }
                    
                    // Static Methods
                    foreach (var methodInfo in type.GetMethods(STATIC_FLAGS))
                    {
                        if (methodInfo.HasAttribute<MonitorAttribute>(true))
                        {
                            ProfileMethodInfo(methodInfo);
                        }
                    }
                    // Instance Methods
                    foreach (var methodInfo in type.GetMethods(INSTANCE_FLAGS))
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
                CreateProfileTypeDefFor(template, declaring, monitored, fieldProfileDefinitions);
                throw new Exception("Test");
            }
            catch (Exception exception)
            {
                if (MonitoringSettings.GetInstance().ThrowOnTypeGenerationError)
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
                CreateProfileTypeDefFor(template, declaring, monitored, propertyProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSettings.GetInstance().ThrowOnTypeGenerationError)
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
                CreateProfileTypeDefFor(template, declaring, monitored, eventProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSettings.GetInstance().ThrowOnTypeGenerationError)
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
                var template = typeof(MethodProfile<,>);
                var declaring = methodInfo.DeclaringType;
                var monitored = methodInfo.ReturnType;
                CreateProfileTypeDefFor(template, declaring, monitored, methodProfileDefinitions);
            }
            catch (Exception exception)
            {
                if (MonitoringSettings.GetInstance().ThrowOnTypeGenerationError)
                {
                    throw;
                }
                errorBuffer.Add(exception.Message);
            }
            
            foreach (var parameterInfo in methodInfo.GetParameters().Where(info => info.IsOut))
            {
                try
                {
                    TryCreateOutParameterHandleDefinition(parameterInfo.ParameterType);
                }
                catch (Exception exception)
                {
                    if (MonitoringSettings.GetInstance().ThrowOnTypeGenerationError)
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
            var concreteType = typeof(OutParameterHandle<>).MakeGenericType(underlying);
            
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
            stringBuilder.Append(type.ToGenericTypeString());
            stringBuilder.Append("\n    ");
            stringBuilder.Append(preserveAttribute);
            stringBuilder.Append("\n    ");
            stringBuilder.Append(type.ToGenericTypeStringFullName());
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
            else if (viableType.IsArray)
            {
                ProcessArray(type);
            }
            else if (viableType.IsGenericIDictionary())
            {
                ProcessDictionary(type);
            }
            else if (viableType.IsGenericIEnumerable(true))
            {
                ProcessEnumerable(type);
            }
            
            void ProcessValueTypeArray(Type valueType)
            {
                var stringBuilder = StringBuilderPool.Get();
                stringBuilder.Append("\n        ");
                stringBuilder.Append(aotBridgeClass);
                stringBuilder.Append('.');
                stringBuilder.Append("AOTValueTypeArray");
                stringBuilder.Append('<');
                stringBuilder.Append(valueType.GetElementType().ToGenericTypeStringFullName());
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
                stringBuilder.Append(arrayType.GetElementType().ToGenericTypeStringFullName());
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
                stringBuilder.Append(dictionaryType.GetGenericArguments()[0].ToGenericTypeStringFullName());
                stringBuilder.Append(',');
                stringBuilder.Append(dictionaryType.GetGenericArguments()[1].ToGenericTypeStringFullName());
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
                stringBuilder.Append(enumerableType.GetGenericArguments()[0].ToGenericTypeStringFullName());
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
                $"[MONITORING] Error: {type.ToGenericTypeString()} is not accessible! ({type.FullName?.Replace('+', '.')})" +
                $"\nCannot generate AOT code for unmanaged internal/private types! " +
                $"Please make sure that {type.ToGenericTypeString()} and all of its declaring types are either public or use a managed type instead of struct!";
            
            errorBuffer.Add(error);
            
            return null;
        }
        
        
        private static void ClearCaches()
        {
            fieldProfileDefinitions.Clear();
            propertyProfileDefinitions.Clear();
            eventProfileDefinitions.Clear();
            methodProfileDefinitions.Clear();
            
            signatureDefinitions.Clear();
            uniqueTypeDefinitions.Clear();
            uniqueMonitoredTypes.Clear();
            
            errorBuffer.Clear();
        }
        
        #endregion
    }
}