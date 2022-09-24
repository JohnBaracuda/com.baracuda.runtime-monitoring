using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Utilities.Extensions
{
    public static class TypeExtensions
    {
        #region --- Type Data ---


        /*
         *  Number Sets
         */

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

        private static readonly HashSet<Type> integerTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
        };

        private static readonly HashSet<Type> decimalTypes = new HashSet<Type>
        {
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        #endregion

        #region --- Type Checks ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumeric(this Type type)
        {
            return numericTypes.Contains(type) || numericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFloatingPoint(this Type type)
        {
            return decimalTypes.Contains(type) || decimalTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInteger(this Type type)
        {
            return integerTypes.Contains(type) || integerTypes.Contains(Nullable.GetUnderlyingType(type));
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
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReadonlyRefStruct(this Type type)
        {
            return type.IsValueType &&
                   type.GetCustomAttributes(true).Any(obj => obj.GetType().Name == "IsByRefLikeAttribute");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStack(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Stack<>);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIEnumerable(this Type type, bool excludeStrings = false)
        {
            return excludeStrings
                ? type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.GetInterface(nameof(IEnumerable)) != null;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericIEnumerable(this Type type, bool excludeStrings = false)
        {
            if (excludeStrings && type.IsString())
            {
                return false;
            }

            if (type.IsGenericType && type.IsInterface && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
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

        /// <summary>
        /// Returns ture if the type and all of its declaring types are public.
        /// </summary>
        public static bool IsAccessible(this Type type)
        {
            var baseTypes = type.GetDeclaringTypes(true);

            for (var i = 0; i < baseTypes.Length; i++)
            {
                var baseType = baseTypes[i];
                if (!baseType.IsPublic && !baseType.IsNestedPublic)
                {
                    return false;
                }
            }

            return true;
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

        /*
         *  Unity Types
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVector(this Type type)
        {
            return (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVectorInt(this Type type)
        {
            return (type == typeof(Vector2Int) || type == typeof(Vector3Int));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsColor(this Type type)
        {
            return (type == typeof(Color) || type == typeof(Color32));
        }

        /*
         *  Generics & Delegates
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type.BaseType);
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

        #endregion
    }
}