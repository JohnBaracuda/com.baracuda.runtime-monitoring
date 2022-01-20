using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Reflection
{
    public static class TypeExtensions
    {
        #region --- [TYPE CHECKS] ---

        public static bool IsNumeric(this Type type) 
            => _numericTypes.Contains(type) || _numericTypes.Contains(Nullable.GetUnderlyingType(type));
        
        public static bool IsFloatingPoint(this Type type) 
            => _decimalTypes.Contains(type) || _decimalTypes.Contains(Nullable.GetUnderlyingType(type));
        
        public static bool IsWholeNumber(this Type type) 
            => _integerTypes.Contains(type) || _integerTypes.Contains(Nullable.GetUnderlyingType(type));
        
        public static bool IsString(this Type type)
            => type == typeof(string);
        
        public static bool IsStruct(this Type type)
            => type.IsValueType && !type.IsEnum && !type.IsPrimitive;

        public static bool IsStatic(this Type type)
            => type.IsAbstract && type.IsSealed;
        
        public static bool IsList(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        
        public static bool IsDictionary(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        
        public static bool IsStack(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Stack<>);

        public static bool IsIEnumerable(this Type type, bool excludeStrings = false)
            => excludeStrings
                ? type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.GetInterface(nameof(IEnumerable)) != null;

        public static bool IsGenericIEnumerable(this Type type, bool excludeStrings = false)
            => excludeStrings
                ? type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null;
        
        public static bool IsIEnumerableT(this Type type, bool excludeStrings = false)
            => excludeStrings
                ? type.IsGenericType && type.IsAssignableFrom(typeof(IEnumerable<>)) && !type.IsString()
                : type.IsGenericType && type.IsAssignableFrom(typeof(IEnumerable<>));

        public static bool IsGenericIDictionary(this Type type)
        {
            return type.GetInterfaces()
                .Any(interfaceType => interfaceType.IsGenericType 
                                      && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static bool IsGenericIList(this Type type)
        {
            return type.GetInterfaces()
                .Any(interfaceType => interfaceType.IsGenericType 
                                      && (interfaceType.GetElementType() ?? interfaceType.GetGenericTypeDefinition()) == typeof(IList<>));
        }
        
        
        public static bool IsVector(this Type type) 
            => (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4));
        
        public static bool IsVectorInt(this Type type) 
            => (type == typeof(Vector2Int) || type == typeof(Vector3Int));
        
        public static bool IsColor(this Type type)
            => (type == typeof(Color) || type == typeof(Color32));
        
        public static bool IsDelegate(this Type type)
            => typeof(Delegate).IsAssignableFrom(type.BaseType);

        
        public static bool IsSubclassOrAssignable(this Type type, Type inheritsFrom)
            => type.IsSubclassOf(inheritsFrom) || type.IsAssignableFrom(inheritsFrom);

        public static bool HasInterface<T>(this Type type)
            => type.GetInterfaces().Any(x => x == typeof(T));
        
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic, bool includeSelf)
        {
            if (!includeSelf && toCheck == generic)
            {
                return false;
            }
            while (toCheck != null && toCheck != typeof(object))
            {
                var current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == current)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [GET] ---
        
        public static Type GetUnderlying(this Type nullableType)
            => Nullable.GetUnderlyingType(nullableType) ?? nullableType;
        
        public static Type GetEnumerableType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            var face = (from i in type.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select i).FirstOrDefault();

            if (face == null)
                throw new ArgumentException("Does not represent an enumerable type.", "type");

            return GetEnumerableType(face);
        }
        
        /// <summary>
        /// Gets the member's underlying type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The underlying type of the member.</returns>
        public static Type GetMemberUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [STRING FORMATTING] ---
        
        public static string GetEventSignatureString(this EventInfo eventInfo)
        {
            var methodInfo = eventInfo.EventHandlerType.GetMethod("Invoke");
            var parameters = methodInfo?.GetParameters();

            var formatted = $"{eventInfo.Name}(";

            if (parameters == null || parameters.Length == 0) return $"{formatted}void)";
            for (int i = 0; i < parameters.Length; i++)
            {
                formatted = $"{formatted}{parameters[i].ParameterType.Name.ToTypeKeyWord()} {parameters[i].Name}, ";
            }

            return $"{formatted.Remove(formatted.Length - 2, 2)})";
        }

        public static string GetSignatureString(this MethodInfo methodInfo, bool includeNames = true)
        {
            var signature = $"{methodInfo.ReturnType.Name}(";
            var parameters = methodInfo.GetParameters();

            for (var i = 0; i < parameters.Length; i++)
            {
                signature = 
                    $"{signature}" +
                    $"{parameters[i].ParameterType.Name}" +
                    $"{(includeNames? $" {parameters[i].Name}" : "")}" +
                    $"{(i == parameters.Length - 1? ")" : ", ")}";
            }

            return signature;
        }
        
        public static string ToGenericTypeString(this Type type)
        {
            if (_typeCache.TryGetValue(type, out var value))
                return value;
            
            if (type.IsGenericType)
            {
                using var builder = ConcurrentStringBuilderPool.GetDisposable();
                using var argBuilder = ConcurrentStringBuilderPool.GetDisposable();
                
                var arguments = type.GetGenericArguments();
                
                foreach (var t in arguments)
                {
                    // Let's make sure we get the argument list.
                    var arg = ToGenericTypeString(t);
                    
                    if (argBuilder.Value.Length > 0)
                        argBuilder.Value.AppendFormat(", {0}", arg);
                    else
                        argBuilder.Value.Append(arg);
                }

                if (argBuilder.Value.Length > 0) builder.Value.AppendFormat("{0}<{1}>", type.Name.Split('`')[0], argBuilder.ToString().ToTypeKeyWord());
                var retType = builder.ToString();
                
                _typeCache.Add(type, retType);
                return retType;
            }

            _typeCache.Add(type, ToTypeKeyWord(type.Name));
            return type.Name;
        }
        
        public static string ToTypeKeyWord(this string typeName) =>
            typeName switch
            {
                "String" => "string",
                "Int32" => "int",
                "Single" => "float",
                "Boolean" => "bool",
                "Byte" => "byte",
                "SByte" => "sbyte",
                "Char" => "char",
                "Decimal" => "decimal",
                "Double" => "double",
                "UInt32" => "uint",
                "Int64" => "long",
                "UInt64" => "ulong",
                "Int16" => "short",
                "UInt16" => "ushort",
                "Object" => "object",
#if CSHARP_9_OR_LATER
                "IntPtr"   => "nint",
                "UIntPtr"  => "nuint",
#endif
                _ => typeName
            };
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---
        
        private static readonly Dictionary<Type, string> _typeCache = new Dictionary<Type, string>();

        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>
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
        
        private static readonly HashSet<Type> _integerTypes = new HashSet<Type>
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
        
        private static readonly HashSet<Type> _decimalTypes = new HashSet<Type>
        {
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        #endregion
    }
}