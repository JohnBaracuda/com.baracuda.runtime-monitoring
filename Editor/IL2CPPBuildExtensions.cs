// Copyright (c) 2022 Jonathan Lang

#if ENABLE_IL2CPP || UNITY_EDITOR

using Baracuda.Monitoring.IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    internal static class IL2CPPBuildExtensions
    {
        public static Type GetUnderlying(this Type type)
        {
            return (type.IsByRef ? type.GetElementType() : type) ?? type;
        }

        public static Type AsObjectPointer(this Type type)
        {
            return type.IsClass || type.IsInterface ? typeof(object) : type;
        }

        public static Type AsDelegatePointer(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type) ? typeof(Delegate) : type;
        }

        public static Type GetReplacement(this Type inaccessible)
        {
            if (!inaccessible.IsEnum)
            {
                return typeof(object);
            }

            switch (Marshal.SizeOf(Enum.GetUnderlyingType(inaccessible)))
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

            return typeof(object);
        }

        public static bool IsEditorAssembly(this Assembly systemAssembly,
            UnityEditor.Compilation.Assembly[] unityssemblies)
        {
            for (var i = 0; i < unityssemblies.Length; i++)
            {
                var unityAssembly = unityssemblies[i];

                if (unityAssembly.name != systemAssembly.GetName().Name)
                {
                    continue;
                }
                var intFlag = (int) unityAssembly.flags;
                if (unchecked((uint) intFlag & (uint) UnityEditor.Compilation.AssemblyFlags.EditorAssembly) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsReadonlyRefStruct(this Type type)
        {
            return type.IsValueType &&
                   type.GetCustomAttributes(true).Any(obj => obj.GetType().Name == "IsByRefLikeAttribute");
        }

        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return propertyInfo?.GetMethod?.IsStatic ??
                   propertyInfo?.SetMethod?.IsStatic ?? throw new InvalidProgramException();
        }

        public static void AddUnique<T>(this IList<T> list, T item)
        {
            if (list.Contains(item))
            {
                return;
            }
            list.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueTypeArray(this Type type)
        {
            return type.IsArray && type.IsElementValueType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsElementValueType(this Type type)
        {
            return type.GetElementType()?.IsValueType ?? false;
        }

        public static bool IsGenericIDictionary(this Type type)
        {
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return true;
            }

            for (var i = 0; i < type.GetInterfaces().Length; i++)
            {
                var interfaceType = type.GetInterfaces()[i];
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsGenericIList(this Type type)
        {
            if (type.IsArray)
            {
                return false;
            }

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return true;
            }

            var interfaces = type.GetInterfaces();

            for (var i = 0; i < interfaces.Length; i++)
            {
                var @interface = interfaces[i];
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsGenericIEnumerable(this Type type)
        {
            if (type.IsArray)
            {
                return false;
            }

            if (type == typeof(string))
            {
                return false;
            }

            if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return true;
            }

            for (var i = 0; i < type.GetInterfaces().Length; i++)
            {
                var interfaceType = type.GetInterfaces()[i];
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsStatic(this EventInfo eventInfo)
        {
            return eventInfo.AddMethod?.IsStatic ?? eventInfo.RemoveMethod.IsStatic;
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

                if (baseType.IsArray && !baseType.GetElementType().IsAccessible())
                {
                    return false;
                }

                if (baseType.IsGenericType)
                {
                    foreach (var genericArgument in baseType.GetGenericArguments())
                    {
                        if (!genericArgument.IsAccessible())
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static string GetDescription(this FieldInfo info)
        {
            return $"{info.DeclaringType?.FullName}::{info.Name} ({info.FieldType.ToTypeDefString()})";
        }

        public static string GetDescription(this PropertyInfo info)
        {
            return $"{info.DeclaringType?.FullName}::{info.Name} ({info.PropertyType.ToTypeDefString()})";
        }

        public static string GetDescription(this EventInfo info)
        {
            return $"{info.DeclaringType?.FullName}::{info.Name} ({info.EventHandlerType.ToTypeDefString()})";
        }

        public static string GetDescription(this MethodInfo info)
        {
            return $"{info.DeclaringType?.FullName}::{info.Name} ({info.ReturnType.ToTypeDefString()})";
        }

        public static string GetDescription(this ParameterInfo info)
        {
            return
                $"{info.Member.DeclaringType}::{info.Member.Name}::{info.Name} ({info.ParameterType.ToTypeDefString()})";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToTypeDefString(this Type type)
        {
            if (type.IsStatic())
            {
                return typeof(object).FullName?.Replace('+', '.');
            }

            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                var builder = new StringBuilder();
                var argBuilder = new StringBuilder();

                var arguments = type.GetGenericArguments();

                foreach (var t in arguments)
                {
                    // Let's make sure we get the argument list.
                    var arg = ToTypeDefString(t);

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

                return retType.Replace('+', '.');
            }

            Debug.Assert(type.FullName != null, $"type.FullName != null | {type.Name}, {type.DeclaringType}");

            var returnValue = type.FullName.Replace('+', '.');
            return returnValue;
        }

        public static Type[] GetAllTypesImplementingOpenGenericType(this Type openGenericType, Assembly[] assemblies)
        {
            var typeBuffer = new List<Type>(32);
            foreach (var assembly in assemblies)
            {
                typeBuffer.AddRange(from assemblyType in assembly.GetTypes()
                    from type in assemblyType.GetInterfaces()
                    let baseType = assemblyType.BaseType
                    where
                        (baseType != null && baseType.IsGenericType &&
                         openGenericType.IsAssignableFrom(baseType.GetGenericTypeDefinition())) ||
                        (type.IsGenericType &&
                         openGenericType.IsAssignableFrom(type.GetGenericTypeDefinition()))
                    select assemblyType);
            }

            return typeBuffer.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string HumanizedName(this Type type)
        {
            if (type.IsGenericType)
            {
                var builder = new StringBuilder();
                var argBuilder = new StringBuilder();

                var arguments = type.GetGenericArguments();

                foreach (var t in arguments)
                {
                    var arg = HumanizedName(t);

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
                    builder.AppendFormat("{0}<{1}>", type.Name.Split('`')[0],
                        argBuilder.ToString().ToTypeKeyWord());
                }

                var retType = builder.ToString().Replace('+', '.');

                return retType;
            }

            return ToTypeKeyWord(type.Name).Replace('+', '.');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToTypeKeyWord(this string typeName)
        {
            switch (typeName)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int";
                case "Single":
                    return "float";
                case "Boolean":
                    return "bool";
                case "Byte":
                    return "byte";
                case "SByte":
                    return "sbyte";
                case "Char":
                    return "char";
                case "Decimal":
                    return "decimal";
                case "Double":
                    return "double";
                case "UInt32":
                    return "uint";
                case "Int64":
                    return "long";
                case "UInt64":
                    return "ulong";
                case "Int16":
                    return "short";
                case "UInt16":
                    return "ushort";
                case "Object":
                    return "object";
                default:
                    return typeName;
            }
        }
    }
}
#endif