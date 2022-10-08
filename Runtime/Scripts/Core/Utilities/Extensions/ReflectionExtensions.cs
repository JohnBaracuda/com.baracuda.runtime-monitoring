// Copyright (c) 2022 Jonathan Lang
#define UNITY_ASSERTIONS

using Baracuda.Monitoring.Utilities.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Utilities.Extensions
{
    internal static class ReflectionExtensions
    {
        #region --- Attribute Utilities ---

        public static bool TryGetCustomAttribute<T>(this MemberInfo memberInfo, out T attribute, bool inherited = false) where T : Attribute
        {
            var found = memberInfo.GetCustomAttribute<T>(inherited);
            if (found != null)
            {
                attribute = found;
                return true;
            }

            attribute = null;
            return false;
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherited = true) where T : Attribute
        {
            try
            {
                return provider.IsDefined(typeof(T), inherited);
            }
            catch (MissingMethodException)
            {
                return false;
            }
        }

        public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttribute<T>() != null;
        }

        public static bool LacksAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttribute<T>() == null;
        }

        public static T FindAttributeInMono<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            foreach (var component in target.GetComponents<MonoBehaviour>())
            {
                var found = component.GetType().GetUnderlying().GetCustomAttribute<T>(inherit);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static T[] FindAttributesInMono<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            var attributes = new List<T>(3);
            foreach (var component in target.GetComponents<MonoBehaviour>())
            {
                attributes.AddRange(component.GetType().GetCustomAttributes<T>(inherit));
            }

            return attributes.ToArray();
        }

        public static T FindAttributeInComponent<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            foreach (var component in target.GetComponents<Component>())
            {
                var found = component.GetType().GetUnderlying().GetCustomAttribute<T>(inherit);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static T[] FindAttributesInComponent<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            var attributes = new List<T>(3);
            foreach (var component in target.GetComponents<Component>())
            {
                attributes.AddRange(component.GetType().GetUnderlying().GetCustomAttributes<T>(inherit));
            }

            return attributes.ToArray();
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType
                .GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        #endregion

        #region --- Invoke Method ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetInvokeMethod(this Type type, BindingFlags flags =
            BindingFlags.Static |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.FlattenHierarchy)
        {
            return type.GetMethod("Invoke", flags);
        }

        #endregion

        #region --- FieldInfo Getter & Setter ---

#if !ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER
        public static Func<TTarget, TResult> CreateGetter<TTarget, TResult>(this FieldInfo field)
        {
            if (field.IsLiteral)
            {
                return (target) => (TResult)field.GetValue(target);;
            }

            var methodName = $"{field!.ReflectedType!.FullName}.get_{field.Name}";
            var setterMethod = new DynamicMethod(methodName, typeof(TResult), new[] {typeof(TTarget)}, true);
            var gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }

            gen.Emit(OpCodes.Ret);
            return (Func<TTarget, TResult>) setterMethod.CreateDelegate(typeof(Func<TTarget, TResult>));
        }

        public static Action<TTarget, TValue> CreateSetter<TTarget, TValue>(this FieldInfo field)
        {
            var methodName = $"{field!.ReflectedType!.FullName}.set_{field.Name}";
            var setterMethod = new DynamicMethod(methodName, null, new[] {typeof(TTarget), typeof(TValue)}, true);
            var gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }

            gen.Emit(OpCodes.Ret);
            return (Action<TTarget, TValue>) setterMethod.CreateDelegate(typeof(Action<TTarget, TValue>));
        }
#else
        public static Func<TTarget, TResult> CreateGetter<TTarget, TResult>(this FieldInfo field)
        {
            return target => (TResult) field.GetValue(target);
        }

        public static Action<TTarget, TValue> CreateSetter<TTarget, TValue>(this FieldInfo field)
        {
            return (target, value) => field.SetValue(target, value);
        }
#endif

        public static Func<TResult> CreateStaticGetter<TResult>(this FieldInfo field)
        {
            return () => (TResult) field.GetValue(null);
        }


        #endregion

        #region --- MemberInfo Casting ---

        private const BindingFlags EVENT_FLAGS = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.Public | BindingFlags.FlattenHierarchy;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo AsFieldInfo(this EventInfo eventInfo)
        {
            return eventInfo.DeclaringType?.GetField(eventInfo.Name, EVENT_FLAGS);
        }

        #endregion

        #region --- Delegate Creation ---

        public static Delegate CreateMatchingDelegate(this MethodInfo methodInfo, object target)
        {
            Func<Type[], Type> getType;

            var isVoid = methodInfo.ReturnType == typeof(void);
            var isStatic = methodInfo.IsStatic;
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isVoid)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] {methodInfo.ReturnType});
            }

            return isStatic
                ? Delegate.CreateDelegate(getType(types.ToArray()), methodInfo)
                : Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        public static Delegate CreateMatchingDelegate(this MethodInfo methodInfo)
        {
            Func<Type[], Type> getType;

            var isVoid = methodInfo.ReturnType == typeof(void);
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isVoid)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] {methodInfo.ReturnType});
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
        }

        #endregion

        #region --- Backing Field Access ---

#if !ENABLE_IL2CPP

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
          public static FieldInfo GetBackingField(this PropertyInfo propertyInfo,
            bool strictCheckIsAutoProperty = false)
        {
            if (strictCheckIsAutoProperty && !StrictCheckIsAutoProperty(propertyInfo))
            {
                return null;
            }

            var gts = propertyInfo.DeclaringType?.GetGenericArguments();
            var accessor = propertyInfo.GetGetMethod(true);
            var msilBytes = accessor?.GetMethodBody()?.GetILAsByteArray();
            var rtk = null != msilBytes
                ? accessor.IsStatic
                    ? GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfStatic(msilBytes)
                    : GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfInstance(msilBytes)
                : -1;

            accessor = propertyInfo.GetSetMethod(true);
            msilBytes = accessor?.GetMethodBody()?.GetILAsByteArray();
            if (null != msilBytes)
            {
                var wtk = accessor.IsStatic
                    ? GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfStatic(msilBytes)
                    : GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfInstance(msilBytes);

                if (-1 != wtk)
                {
                    if (wtk == rtk)
                    {
                        var wfi = propertyInfo.Module.ResolveField(wtk, gts, null);
                        if (!strictCheckIsAutoProperty || null == wfi || StrictCheckIsAutoPropertyBackingField(propertyInfo, wfi))
                        {
                            return wfi;
                        }
                    }

                    return null;
                }
            }

            if (-1 == rtk)
            {
                return null;
            }

            var rfi = propertyInfo.Module.ResolveField(rtk, gts, null);
            return !strictCheckIsAutoProperty || null == rfi || StrictCheckIsAutoPropertyBackingField(propertyInfo, rfi)
                ? rfi
                : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StrictCheckIsAutoProperty(PropertyInfo pi)
        {
            return null != pi.GetCustomAttribute<CompilerGeneratedAttribute>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StrictCheckIsAutoPropertyBackingField(PropertyInfo pi, FieldInfo fi)
        {
            return fi.Name == "<" + pi.Name + ">k__BackingField";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfStatic(byte[] msilBytes)
        {
            return 6 == msilBytes.Length && 0x7E == msilBytes[0] && 0x2A == msilBytes[5]
                ? BitConverter.ToInt32(msilBytes, 1)
                : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfStatic(byte[] msilBytes)
        {
            return 7 == msilBytes.Length && 0x02 == msilBytes[0] && 0x80 == msilBytes[1] && 0x2A == msilBytes[6]
                ? BitConverter.ToInt32(msilBytes, 2)
                : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfInstance(byte[] msilBytes)
        {
            return 7 == msilBytes.Length && 0x02 == msilBytes[0] && 0x7B == msilBytes[1] && 0x2A == msilBytes[6]
                ? BitConverter.ToInt32(msilBytes, 2)
                : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfInstance(byte[] msilBytes)
        {
            return 8 == msilBytes.Length && 0x02 == msilBytes[0] && 0x03 == msilBytes[1] && 0x7D == msilBytes[2] &&
                   0x2A == msilBytes[7]
                ? BitConverter.ToInt32(msilBytes, 3)
                : -1;
        }
#endif

        #endregion

        #region --- Underlying & Collection Types ---

        public static Type GetUnderlying(this Type nullableType)
            => Nullable.GetUnderlyingType(nullableType) ?? nullableType;

        public static Type GetEnumerableType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type.GetGenericArguments()[0];
            }

            var face = (from i in type.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select i).FirstOrDefault();

            if (face == null)
            {
                throw new ArgumentException("Does not represent an enumerable type.", "type");
            }

            return GetEnumerableType(face);
        }

        public static Type GetMemberUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo) member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo",
                        nameof(member));
            }
        }

        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return propertyInfo?.GetMethod?.IsStatic ?? propertyInfo?.SetMethod?.IsStatic ?? throw new InvalidProgramException();
        }

        public static bool IsStatic(this EventInfo eventInfo)
        {
            return eventInfo.AddMethod?.IsStatic ?? eventInfo.RemoveMethod.IsStatic;
        }

        #endregion

        #region --- Event ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSubscriberCount<TDelegate>(this TDelegate eventDelegate) where TDelegate : Delegate
        {
            return eventDelegate?.GetInvocationList().Length ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSubscriberCountString<TDelegate>(this TDelegate eventDelegate) where TDelegate : Delegate
        {
            return eventDelegate.GetSubscriberCount().ToString();
        }

        #endregion

        #region --- Display String Formatting ---

        private static readonly Dictionary<Type, string> typeCache = new Dictionary<Type, string>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSignatureString<TDelegate>(this TDelegate target) where TDelegate : Delegate
        {
            var method = target?.Method ?? typeof(TDelegate).GetInvokeMethod();
            return method.GetSignatureString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetEventSignatureString(this EventInfo eventInfo)
        {
            var eventType = eventInfo.EventHandlerType;
            var methodInfo = eventType.GetInvokeMethod();
            var parameters = methodInfo.GetParameters();
            var isGeneric = eventType.IsGenericType;

            var sb = ConcurrentStringBuilderPool.Get();
            sb.Append(eventType.GetNameWithoutGenericArity());

            if (eventType.IsGenericType){}

            if (parameters.Length > 0)
            {
                sb.Append(isGeneric ? '<' : '(');
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterInfo = parameters[i];
                sb.Append(parameterInfo.ParameterType.HumanizedName());
                if (!isGeneric)
                {
                    sb.Append(' ');
                    sb.Append(parameterInfo.Name);
                }
                if (i < parameters.Length - 1)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }
            }

            if (parameters.Length > 0)
            {
                sb.Append(isGeneric ? '>' : ')');
            }

            return ConcurrentStringBuilderPool.Release(sb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSignatureString(this MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            var stringBuilder = ConcurrentStringBuilderPool.Get();

            stringBuilder.Append(methodInfo.ReturnType.Name);

            if (!parameters.Any())
            {
                return stringBuilder.ToString();
            }

            stringBuilder.Append(' ');
            stringBuilder.Append('(');
            for (var i = 0; i < parameters.Length; i++)
            {
                stringBuilder.Append(parameters[i].ParameterType.Name);
                stringBuilder.Append(' ');
                stringBuilder.Append(parameters[i].Name);
                if (i != parameters.Length - 1)
                {
                    stringBuilder.Append(',');
                    stringBuilder.Append(' ');
                }
            }
            stringBuilder.Append(')');


            return ConcurrentStringBuilderPool.Release(stringBuilder);
        }

        private static readonly Dictionary<Type, string> typeCacheFullName = new Dictionary<Type, string>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToReadableTypeStringFullName(this Type type)
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
                    var arg = ToReadableTypeStringFullName(t);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetNameWithoutGenericArity(this Type type)
        {
            var name = type.Name;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string HumanizedName(this Type type)
        {
            if (typeCache.TryGetValue(type, out var value))
            {
                return value;
            }

            if (type.IsGenericType)
            {
                var builder = ConcurrentStringBuilderPool.Get();
                var argBuilder = ConcurrentStringBuilderPool.Get();

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

                typeCache.Add(type, retType);

                ConcurrentStringBuilderPool.ReleaseStringBuilder(builder);
                ConcurrentStringBuilderPool.ReleaseStringBuilder(argBuilder);

                return retType;
            }

            var str = ToTypeKeyWord(type.Name).Replace('+', '.');
            typeCache.Add(type, str);
            return str;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFullTypeName(this string typeKeyword)
        {
            switch (typeKeyword)
            {
                case "string":
                    return "System.String";
                case "sbyte":
                    return "System.SByte";
                case "byte":
                    return "System.Byte";
                case "short":
                    return "System.Int16";
                case "ushort":
                    return "System.UInt16";
                case "int":
                    return "System.Int32";
                case "uint":
                    return "System.UInt32";
                case "long":
                    return "System.Int64";
                case "ulong":
                    return "System.UInt64";
                case "char":
                    return "System.Char";
                case "float":
                    return "System.Single";
                case "double":
                    return "System.Double";
                case "bool":
                    return "System.Boolean";
                case "decimal":
                    return "System.Decimal";
                case "void":
                    return "System.Void";
                case "object":
                    return "System.Object";
                default:
                    return typeKeyword;
            }
        }

        #endregion

        #region --- Base Type Reflection ---

        public static Type[] GetBaseTypes(this Type type, bool includeThis, bool includeInterfaces = false)
        {
            var temp = ListPool<Type>.Get();

            if (includeThis)
            {
                temp.Add(type);
            }

            if (includeInterfaces)
            {
                temp.AddRange(type.GetInterfaces());
            }

            while (type.BaseType != null)
            {
                temp.Add(type.BaseType);
                type = type.BaseType;
                if (type == typeof(MonoBehaviour) || type == typeof(ScriptableObject))
                {
                    break;
                }
            }

            var array = temp.ToArray();
            ListPool<Type>.Release(temp);
            return array;
        }

        public static Type[] GetDeclaringTypes(this Type type, bool includeThis)
        {
            var temp = ListPool<Type>.Get();

            if (includeThis)
            {
                temp.Add(type);
            }

            while (type.DeclaringType != null)
            {
                temp.Add(type.DeclaringType);
                type = type.DeclaringType;
            }

            var array = temp.ToArray();
            ListPool<Type>.Release(temp);
            return array;
        }

        public static Type[] GetBaseTypesExcludeUnityTypes(this Type type, bool includeThis)
        {
            var temp = ListPool<Type>.Get();

            if (includeThis)
            {
                temp.Add(type);
            }

            while (type.BaseType != null)
            {
                if (type.BaseType == typeof(MonoBehaviour) || type.BaseType == typeof(ScriptableObject))
                {
                    break;
                }
                temp.Add(type.BaseType);
                type = type.BaseType;
            }

            var array = temp.ToArray();
            ListPool<Type>.Release(temp);
            return array;
        }

        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> memberCache =
            new Dictionary<Type, Dictionary<string, MemberInfo>>();


        public static void SetMemberValue<TValue>(this Type type, string memberName, object target, BindingFlags flags, TValue value)
        {
            GetMemberValue(memberName, type, target, flags);
        }

        public static void SetMemberValue<TValue>(string memberName, Type type, object target, BindingFlags flags, TValue value)
        {
            if (memberCache.TryGetValue(type, out var dictionary) &&
                dictionary.TryGetValue(memberName, out var memberInfo))
            {
                switch (memberInfo)
                {
                    case FieldInfo fi:  fi.SetValue(target, value);
                        break;
                    case PropertyInfo pi: pi.SetValue(target, value);
                        break;
                    case MethodInfo mi: mi.Invoke(target, new object[] {value});
                        break;
                }
            }

            var fieldInfo = type.GetFieldIncludeBaseTypes(memberName, flags);
            if (fieldInfo != null)
            {
                Cache(fieldInfo);
                fieldInfo.SetValue(target, value);
                return;
            }

            var methodInfo = type.GetMethodIncludeBaseTypes(memberName, flags);
            if (methodInfo != null)
            {
                Cache(methodInfo);
                methodInfo.Invoke(target, new object[] {value});
                return;
            }

            var propertyInfo = type.GetPropertyIncludeBaseTypes(memberName, flags);
            if (propertyInfo != null)
            {
                Cache(propertyInfo);
                propertyInfo.SetValue(target, value);
            }

            void Cache(MemberInfo member)
            {
                if (memberCache.TryGetValue(type, out dictionary))
                {
                    dictionary.Add(memberName, member);
                }
                else
                {
                    memberCache.Add(type, new Dictionary<string, MemberInfo>
                    {
                        {memberName, member}
                    });
                }
            }
        }


        public static object GetMemberValue(this Type type, string memberName, object target, BindingFlags flags)
        {
            return GetMemberValue(memberName, type, target, flags);
        }

        public static object GetMemberValue(string memberName, Type type, object target, BindingFlags flags)
        {
            if (memberCache.TryGetValue(type, out var dictionary) &&
                dictionary.TryGetValue(memberName, out var memberInfo))
            {
                switch (memberInfo)
                {
                    case FieldInfo fi: return fi.GetValue(target);
                    case PropertyInfo pi: return pi.GetValue(target);
                    case MethodInfo mi: return mi.Invoke(target, Array.Empty<object>());
                }
            }

            var fieldInfo = type.GetFieldIncludeBaseTypes(memberName, flags);
            if (fieldInfo != null)
            {
                Cache(fieldInfo);
                return fieldInfo.GetValue(target);
            }

            var methodInfo = type.GetMethodIncludeBaseTypes(memberName, flags);
            if (methodInfo != null)
            {
                Cache(methodInfo);
                return methodInfo.Invoke(target, Array.Empty<object>());
            }

            var propertyInfo = type.GetPropertyIncludeBaseTypes(memberName, flags);
            if (propertyInfo != null)
            {
                Cache(propertyInfo);
                return propertyInfo.GetValue(target);
            }

            void Cache(MemberInfo member)
            {
                if (memberCache.TryGetValue(type, out dictionary))
                {
                    dictionary.Add(memberName, member);
                }
                else
                {
                    memberCache.Add(type, new Dictionary<string, MemberInfo>
                    {
                        {memberName, member}
                    });
                }
            }

            return null;
        }

        public static FieldInfo GetFieldIncludeBaseTypes(this Type type, string fieldName, BindingFlags flags =
            BindingFlags.Static |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.FlattenHierarchy)
        {
            FieldInfo fieldInfo = null;
            var targetType = type;

            while (fieldInfo == null)
            {
                fieldInfo = targetType.GetField(fieldName, flags);
                targetType = targetType.BaseType;

                if (targetType == null)
                {
                    return null;
                }
            }

            return fieldInfo;
        }

        public static PropertyInfo GetPropertyIncludeBaseTypes(this Type type, string propertyName, BindingFlags flags =
            BindingFlags.Static |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.FlattenHierarchy)
        {
            PropertyInfo propertyInfo = null;
            var targetType = type;

            while (propertyInfo == null)
            {
                propertyInfo = targetType.GetProperty(propertyName, flags);
                targetType = targetType.BaseType;

                if (targetType == null)
                {
                    return null;
                }
            }

            return propertyInfo;
        }


        public static MethodInfo GetMethodIncludeBaseTypes(this Type type, string methodName, BindingFlags flags)
        {
            MethodInfo methodInfo = null;
            var targetType = type;

            var value = 0;
            while (methodInfo == null)
            {
                methodInfo = targetType.GetMethod(methodName, flags);
                targetType = targetType.BaseType;

                if (targetType == null || value++ > 10)
                {
                    return null;
                }
            }

            return methodInfo;
        }

        public static EventInfo GetEventIncludeBaseTypes(this Type type, string eventName, BindingFlags flags)
        {
            EventInfo eventInfo = null;
            var targetType = type;

            while (eventInfo == null)
            {
                eventInfo = targetType.GetEvent(eventName, flags);
                targetType = targetType.BaseType;

                if (targetType == null)
                {
                    return null;
                }
            }

            return eventInfo;
        }

        /*
         * Multiple
         */

        public static FieldInfo[] GetFieldsIncludeBaseTypes(this Type type, BindingFlags flags)
        {
            var fieldInfos = ListPool<FieldInfo>.Get();
            var typesToCheck = ListPool<Type>.Get();
            var targetType = type;

            while (targetType.EqualsNone(typeof(MonoBehaviour), typeof(ScriptableObject), typeof(object)))
            {
                typesToCheck.Add(targetType);
                targetType = targetType?.BaseType;
            }

            for (var i = typesToCheck.Count - 1; i >= 0; i--)
            {
                fieldInfos.AddRange(typesToCheck[i].GetFields(flags));
            }

            var array = fieldInfos.ToArray();
            ListPool<Type>.Release(typesToCheck);
            ListPool<FieldInfo>.Release(fieldInfos);
            return array;
        }

        public static PropertyInfo[] GetPropertiesIncludeBaseTypes(this Type type, BindingFlags flags)
        {
            var propertyInfos = ListPool<PropertyInfo>.Get();
            var typesToCheck = ListPool<Type>.Get();
            var targetType = type;

            while (targetType.EqualsNone(typeof(MonoBehaviour), typeof(ScriptableObject), typeof(object)))
            {
                typesToCheck.Add(targetType);
                targetType = targetType?.BaseType;
            }

            for (var i = typesToCheck.Count - 1; i >= 0; i--)
            {
                propertyInfos.AddRange(typesToCheck[i].GetProperties(flags));
            }

            var array = propertyInfos.ToArray();
            ListPool<Type>.Release(typesToCheck);
            ListPool<PropertyInfo>.Release(propertyInfos);
            return array;
        }


        public static MethodInfo[] GetMethodsIncludeBaseTypes(this Type type, BindingFlags flags)
        {
            var methodInfos = ListPool<MethodInfo>.Get();
            var typesToCheck = ListPool<Type>.Get();
            var targetType = type;

            while (targetType.EqualsNone(typeof(MonoBehaviour), typeof(ScriptableObject), typeof(object)))
            {
                typesToCheck.Add(targetType);
                targetType = targetType?.BaseType;
            }

            for (var i = typesToCheck.Count - 1; i >= 0; i--)
            {
                methodInfos.AddRange(typesToCheck[i].GetMethods(flags));
            }

            var array = methodInfos.ToArray();
            ListPool<Type>.Release(typesToCheck);
            ListPool<MethodInfo>.Release(methodInfos);
            return array;
        }

        public static MemberInfo[] GetMembersIncludeBaseTypes(this Type type, BindingFlags flags)
        {
            var memberInfos = ListPool<MemberInfo>.Get();
            var typesToCheck = ListPool<Type>.Get();
            var targetType = type;

            while (targetType.EqualsNone(typeof(MonoBehaviour), typeof(ScriptableObject), typeof(object)))
            {
                typesToCheck.Add(targetType);
                targetType = targetType?.BaseType;
            }

            for (var i = typesToCheck.Count - 1; i >= 0; i--)
            {
                memberInfos.AddRange(typesToCheck[i].GetMembers(flags));
            }

            var array = memberInfos.ToArray();
            ListPool<Type>.Release(typesToCheck);
            ListPool<MemberInfo>.Release(memberInfos);
            return array;
        }


        #endregion

        #region --- Helper ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EqualsNone<T>(this T target, T otherA, T otherB, T otherC) where T : class
        {
            return !(target.Equals(otherA) || target.Equals(otherB) || target.Equals(otherC));
        }

        #endregion
    }
}
