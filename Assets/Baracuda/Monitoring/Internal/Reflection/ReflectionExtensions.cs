using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Internal.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Reflection
{
    public static class ReflectionExtensions
    {
        #region --- Attribute Utilities ---

        public static bool TryGetCustomAttribute<T>(this MemberInfo memberInfo, out T attribute, bool inherited = false) where T : Attribute
        {
            if (memberInfo.GetCustomAttribute<T>(inherited) is { } found)
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

#if !ENABLE_IL2CPP
        public static Func<TTarget, TResult> CreateGetter<TTarget, TResult>(this FieldInfo field)
        {
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
            return target => (TResult)field.GetValue(target);
        }

        public static Action<TTarget, TValue> CreateSetter<TTarget, TValue>(this FieldInfo field)
        {
            return (target, value) => field.SetValue(target, value);
        }
#endif
        

        #endregion
        
        #region --- MemberInfo Casting ---

        private const BindingFlags EVENT_FLAGS = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.Public | BindingFlags.FlattenHierarchy;
        
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

        private static bool StrictCheckIsAutoProperty(PropertyInfo pi)
        {
            return null != pi.GetCustomAttribute<CompilerGeneratedAttribute>();
        }

        private static bool StrictCheckIsAutoPropertyBackingField(PropertyInfo pi, FieldInfo fi)
        {
            return fi.Name == "<" + pi.Name + ">k__BackingField";
        }

        private static int GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfStatic(byte[] msilBytes)
        {
            return 6 == msilBytes.Length && 0x7E == msilBytes[0] && 0x2A == msilBytes[5]
                ? BitConverter.ToInt32(msilBytes, 1)
                : -1;
        }

        private static int GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfStatic(byte[] msilBytes)
        {
            return 7 == msilBytes.Length && 0x02 == msilBytes[0] && 0x80 == msilBytes[1] && 0x2A == msilBytes[6]
                ? BitConverter.ToInt32(msilBytes, 2)
                : -1;
        }

        private static int GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfInstance(byte[] msilBytes)
        {
            return 7 == msilBytes.Length && 0x02 == msilBytes[0] && 0x7B == msilBytes[1] && 0x2A == msilBytes[6]
                ? BitConverter.ToInt32(msilBytes, 2)
                : -1;
        }

        private static int GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfInstance(byte[] msilBytes)
        {
            return 8 == msilBytes.Length && 0x02 == msilBytes[0] && 0x03 == msilBytes[1] && 0x7D == msilBytes[2] &&
                   0x2A == msilBytes[7]
                ? BitConverter.ToInt32(msilBytes, 3)
                : -1;
        }
#endif
        
        

        #endregion
        
        #region --- Type Data ---

        /*
         *  Type Cache   
         */
        
        private static readonly Dictionary<Type, string> typeCache = new Dictionary<Type, string>();
        
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

        /*
         * Access   
         */

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

        /*
         *  Numeric   
         */
        
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

        /*
         *  Elementary & Meta
         */

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
            return type.IsStruct() && type.IsRefBy() && type.IsReadOnly();
        }

        public static bool IsRefBy(this Type type)
        {
            return type.HasAttribute<IsByRefLikeAttribute>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReadOnly(this Type type)
        {
            return type.HasAttribute<IsReadOnlyAttribute>();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        /*
         *  Collections   
         */

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
        public static bool IsGenericIEnumerable(this Type type, bool excludeStrings = false)
        {
            return excludeStrings
                ? type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null && !type.IsString()
                : type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIEnumerableT(this Type type, bool excludeStrings = false)
        {
            return excludeStrings
                ? type.IsGenericType && type.IsAssignableFrom(typeof(IEnumerable<>)) && !type.IsString()
                : type.IsGenericType && type.IsAssignableFrom(typeof(IEnumerable<>));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericIDictionary(this Type type)
        {
            return type.GetInterfaces()
                .Any(interfaceType => interfaceType.IsGenericType
                                      && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericIList(this Type type)
        {
            return type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType &&
                                                             (interfaceType.GetElementType() ??
                                                              interfaceType.GetGenericTypeDefinition()) ==
                                                             typeof(IList<>));
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
        public static bool HasInterface<T>(this Type type)
        {
            return type.GetInterfaces().Any(x => x == typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        #endregion

        #region --- Event ---

        public static int GetSubscriberCount<TDelegate>(this TDelegate eventDelegate) where TDelegate : Delegate
        {
            return eventDelegate?.GetInvocationList().Length ?? 0;
        }
        
        public static string GetSubscriberCountString<TDelegate>(this TDelegate eventDelegate) where TDelegate : Delegate
        {
            return eventDelegate.GetSubscriberCount().ToString();
        }

        #endregion

        #region --- Display String Formatting ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSignatureString<TDelegate>(this TDelegate target) where TDelegate : Delegate
        {
            var method = target?.Method ?? typeof(TDelegate).GetInvokeMethod();
            return method.GetSignatureString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetEventSignatureString(this EventInfo eventInfo)
        {
            var methodInfo = eventInfo.EventHandlerType.GetMethod("Invoke");
            var parameters = methodInfo?.GetParameters();

            var formatted = $"{eventInfo.Name}(";

            if (parameters == null || parameters.Length == 0)
            {
                return $"{formatted}void)";
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                formatted = $"{formatted}{parameters[i].ParameterType.Name.ToTypeKeyWord()} {parameters[i].Name}, ";
            }

            return $"{formatted.Remove(formatted.Length - 2, 2)})";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSignatureString(this MethodInfo methodInfo)
        {
            var parameters = methodInfo!.GetParameters();

            var stringBuilder = ConcurrentStringBuilderPool.GetDisposable();

            stringBuilder.Append(methodInfo!.ReturnType.Name);

            if (parameters.Any())
            {
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
            }
            

            return stringBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToSyntaxString(this Type type)
        {
            if (typeCache.TryGetValue(type, out var value))
            {
                return value;
            }

            if (type.IsGenericType)
            {
                using var builder = ConcurrentStringBuilderPool.GetDisposable();
                using var argBuilder = ConcurrentStringBuilderPool.GetDisposable();

                var arguments = type.GetGenericArguments();

                foreach (var t in arguments)
                {
                    var arg = ToSyntaxString(t);

                    if (argBuilder.Value.Length > 0)
                    {
                        argBuilder.Value.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        argBuilder.Append(arg);
                    }
                }

                if (argBuilder.Value.Length > 0)
                {
                    builder.Value.AppendFormat("{0}<{1}>", type.Name.Split('`')[0],
                        argBuilder.ToString().ToTypeKeyWord());
                }

                var retType = builder.ToString().Replace('+', '.');

                typeCache.Add(type, retType);
                return retType;
            }

            var str = ToTypeKeyWord(type.Name).Replace('+', '.');
            typeCache.Add(type, str);
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFullTypeName(this string typeKeyword) =>
            typeKeyword switch
            {
                "string" => "System.String",
                "sbyte" => "System.SByte",
                "byte" => "System.Byte",
                "short" => "System.Int16",
                "ushort" => "System.UInt16",
                "int" => "System.Int32",
                "uint" => "System.UInt32",
                "long" => "System.Int64",
                "ulong" => "System.UInt64",
                "char" => "System.Char",
                "float" => "System.Single",
                "double" => "System.Double",
                "bool" => "System.Boolean",
                "decimal" => "System.Decimal",
                "void" => "System.Void",
                "object" => "System.Object",
                _ => typeKeyword
            };

        #endregion

        #region --- Base Type Reflection ---
                
        public static Type[] GetBaseTypes(this Type type, bool includeThis)
        {
            var temp = ConcurrentListPool<Type>.Get();

            if (includeThis)
            {
                temp.Add(type);
            }
            
            while (type.BaseType != null)
            {
                temp.Add(type.BaseType);
                type = type.BaseType;
                if(type == typeof(MonoBehaviour) || type == typeof(ScriptableObject))
                {
                    break;
                }
            }

            var array = temp.ToArray();
            ConcurrentListPool<Type>.Release(temp);
            return array;
        }
        
        public static Type[] GetDeclaringTypes(this Type type, bool includeThis)
        {
            var temp = ConcurrentListPool<Type>.Get();

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
            ConcurrentListPool<Type>.Release(temp);
            return array;
        }
        
        public static Type[] GetBaseTypesExcludeUnityTypes(this Type type, bool includeThis)
        {
            var temp = ConcurrentListPool<Type>.Get();

            if (includeThis)
            {
                temp.Add(type);
            }
            
            while (type.BaseType != null)
            {
                if(type.BaseType == typeof(MonoBehaviour) || type.BaseType == typeof(ScriptableObject))
                {
                    break;
                }
                temp.Add(type.BaseType);
                type = type.BaseType;
            }

            var array = temp.ToArray();
            ConcurrentListPool<Type>.Release(temp);
            return array;
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

        #endregion
    }
}
