using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Utilities.Extensions
{
    internal static class TypeExtensions
    {

        private static readonly HashSet<Type> numericTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumeric(this Type type)
        {
            return numericTypes.Contains(type) || numericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInt32(this Type type)
        {
            return type == typeof(int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInt64(this Type type)
        {
            return type == typeof(long);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSingle(this Type type)
        {
            return type == typeof(float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDouble(this Type type)
        {
            return type == typeof(float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsString(this Type type)
        {
            return type == typeof(string);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIEnumerable(this Type type, bool excludeStrings = false)
        {
            return excludeStrings
                ? type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.GetInterface(nameof(IEnumerable)) != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericIEnumerable(this Type type, out Type elementType)
        {
            if (type.IsString())
            {
                elementType = null;
                return false;
            }

            if (type.IsGenericType && type.IsInterface && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = null;
                return true;
            }

            for (var i = 0; i < type.GetInterfaces().Length; i++)
            {
                var interfaceType = type.GetInterfaces()[i];
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = interfaceType.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetDefault(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericIList(this Type type)
        {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasReturnValue(this MethodInfo methodInfo)
        {
            return methodInfo.ReturnType != typeof(void);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasReturnValueOrOutParameter(this MethodInfo methodInfo)
        {
            var parameter = methodInfo.GetParameters();
            for (var i = 0; i < parameter.Length; i++)
            {
                if (parameter[i].IsOut)
                {
                    return true;
                }
            }
            return methodInfo.HasReturnValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type NotVoid(this Type type, Type replacement)
        {
            return type == typeof(void) ? replacement : type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSubclassOrAssignable(this Type type, Type inheritsFrom)
        {
            return type.IsSubclassOf(inheritsFrom) || type.IsAssignableFrom(inheritsFrom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasInterface<T>(this Type type) where T : class
        {
            if (type == typeof(T))
            {
                return true;
            }
            for (var i = 0; i < type.GetInterfaces().Length; i++)
            {
                var @interface = type.GetInterfaces()[i];
                if (@interface == typeof(T))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic, bool includeSelf, int maxDepth = 7)
        {
            if (!includeSelf && toCheck == generic)
            {
                return false;
            }

            var index = 0;
            while (toCheck != null && toCheck != typeof(object))
            {
                var current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == current)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
                if (index++ > maxDepth)
                {
                    return false;
                }
            }

            return false;
        }
    }
}