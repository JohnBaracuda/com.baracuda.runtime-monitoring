using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baracuda.Pooling.Concretions;

namespace Baracuda.Monitoring.Internal.Reflection
{
    internal static class TypeExtensions
    {
        #region --- [TYPE CHECKS] ---

        internal static bool IsString(this Type type)
            => type == typeof(string);

        internal static bool IsDictionary(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        internal static bool IsIEnumerable(this Type type, bool excludeStrings = false)
            => excludeStrings
                ? type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.GetInterface(nameof(IEnumerable)) != null;

        internal static bool IsGenericIEnumerable(this Type type, bool excludeStrings = false)
            => excludeStrings
                ? type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null;

        internal static bool IsGenericIDictionary(this Type type)
        {
            return type.GetInterfaces()
                .Any(interfaceType => interfaceType.IsGenericType
                                      && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        internal static bool IsGenericIList(this Type type)
            => type.GetInterfaces().Any(interfaceType =>
                interfaceType.IsGenericType &&
                (interfaceType.GetElementType() ?? interfaceType.GetGenericTypeDefinition()) == typeof(IList<>));


        internal static bool IsSubclassOrAssignable(this Type type, Type inheritsFrom)
            => type.IsSubclassOf(inheritsFrom) || type.IsAssignableFrom(inheritsFrom);

        internal static bool HasInterface<T>(this Type type)
            => type.GetInterfaces().Any(x => x == typeof(T));


        internal static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic, bool includeSelf)
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

        #region --- [DISPLAY STRING] ---
        
        internal static string GetEventSignatureString(this EventInfo eventInfo)
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

        private static readonly Dictionary<Type, string> _typeCache = new Dictionary<Type, string>();

        internal static string ToGenericTypeString(this Type type)
        {
            if (_typeCache.TryGetValue(type, out var value))
                return value;

            if (type.IsGenericType)
            {
                var sb = ConcurrentStringBuilderPool.Get();
                var sbArgs = ConcurrentStringBuilderPool.Get();

                var arguments = type.GetGenericArguments();

                foreach (var t in arguments)
                {
                    // Let's make sure we get the argument list.
                    var arg = ToGenericTypeString(t);

                    if (sbArgs.Length > 0)
                        sbArgs.AppendFormat(", {0}", arg);
                    else
                        sbArgs.Append(arg);
                }

                if (sbArgs.Length > 0)
                {
                    sb.AppendFormat("{0}<{1}>", type.Name.Split('`')[0],
                        ConcurrentStringBuilderPool.Release(sbArgs).ToTypeKeyWord());
                }
                else
                {
                    ConcurrentStringBuilderPool.ReleaseStringBuilder(sbArgs);
                }

                var retType = ConcurrentStringBuilderPool.Release(sb);

                _typeCache.Add(type, retType);
                return retType;
            }

            _typeCache.Add(type, ToTypeKeyWord(type.Name));
            return type.Name;
        }

        internal static string ToTypeKeyWord(this string typeName)
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

        #endregion
    }
}