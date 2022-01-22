using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Reflection
{
    public static class ReflectionExtensions
    {
        #region --- [ATTRIBUTE UTILITES] ---

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
        
        /// <summary>
        /// Find a <see cref="Attribute"/> of memberInfo T that is targeted at the objects memberInfo.
        /// </summary>
        public static T FindAttributeInMono<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            foreach (var component in target.GetComponents<MonoBehaviour>())
            {
                if (component.GetType().GetUnderlying().GetCustomAttribute<T>(inherit) is { } found)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Find a <see cref="Attribute"/> of memberInfo T that is targeted at the objects memberInfo.
        /// </summary>
        public static T[] FindAttributesInMono<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            var attributes = new List<T>(3);
            foreach (var component in target.GetComponents<MonoBehaviour>())
            {
                if (component.GetType().GetCustomAttributes<T>(inherit) is { } found)
                {
                    attributes.AddRange(found);
                }
            }

            return attributes.ToArray();
        }

        /// <summary>
        /// Find a <see cref="Attribute"/> of memberInfo T that is targeted at the objects memberInfo.
        /// </summary>
        public static T FindAttributeInComponent<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            foreach (var component in target.GetComponents<Component>())
            {
                if (component.GetType().GetUnderlying().GetCustomAttribute<T>(inherit) is { } found)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Find a <see cref="Attribute"/> of memberInfo T that is targeted at the objects memberInfo.
        /// </summary>
        public static T[] FindAttributesInComponent<T>(this GameObject target, bool inherit = true) where T : Attribute
        {
            var attributes = new List<T>(3);
            foreach (var component in target.GetComponents<Component>())
            {
                if (component.GetType().GetUnderlying().GetCustomAttributes<T>(inherit) is { } found)
                {
                    attributes.AddRange(found);
                }
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
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INVOKE METHOD] ---

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
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FIELDINFO GETTER & SETTER] ---

#if NET_4_6 || NET_STANDARD
        
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
#endif

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CASTING] ---

        private const BindingFlags EVENT_FLAGS = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.Public | BindingFlags.FlattenHierarchy;

        public static FieldInfo AsFieldInfo(this EventInfo eventInfo)
        {
            return eventInfo.DeclaringType?.GetField(eventInfo.Name, EVENT_FLAGS);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DELEGATE CREATION] ---
        
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
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [PROPERTY BACKGING FIELD] ---

          public static FieldInfo GetBackingField(this PropertyInfo propertyInfo,
            bool strictCheckIsAutoProperty = false)
        {
            if (strictCheckIsAutoProperty && !StrictCheckIsAutoProperty(propertyInfo)) return null;

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
                            return wfi;
                    }

                    return null;
                }
            }

            if (-1 == rtk) return null;

            var rfi = propertyInfo.Module.ResolveField(rtk, gts, null);
            return !strictCheckIsAutoProperty || null == rfi || StrictCheckIsAutoPropertyBackingField(propertyInfo, rfi)
                ? rfi
                : null;
        }

        private static bool StrictCheckIsAutoProperty(PropertyInfo pi) =>
            null != pi.GetCustomAttribute<CompilerGeneratedAttribute>();

        private static bool StrictCheckIsAutoPropertyBackingField(PropertyInfo pi, FieldInfo fi) =>
            fi.Name == "<" + pi.Name + ">k__BackingField";

        private static int GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfStatic(byte[] msilBytes) =>
            6 == msilBytes.Length && 0x7E == msilBytes[0] && 0x2A == msilBytes[5]
                ? BitConverter.ToInt32(msilBytes, 1)
                : -1;

        private static int GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfStatic(byte[] msilBytes) =>
            7 == msilBytes.Length && 0x02 == msilBytes[0] && 0x80 == msilBytes[1] && 0x2A == msilBytes[6]
                ? BitConverter.ToInt32(msilBytes, 2)
                : -1;

        private static int GetAutoPropertyBakingFieldMetadataTokenInGetMethodOfInstance(byte[] msilBytes) =>
            7 == msilBytes.Length && 0x02 == msilBytes[0] && 0x7B == msilBytes[1] && 0x2A == msilBytes[6]
                ? BitConverter.ToInt32(msilBytes, 2)
                : -1;

        private static int GetAutoPropertyBakingFieldMetadataTokenInSetMethodOfInstance(byte[] msilBytes) =>
            8 == msilBytes.Length && 0x02 == msilBytes[0] && 0x03 == msilBytes[1] && 0x7D == msilBytes[2] &&
            0x2A == msilBytes[7]
                ? BitConverter.ToInt32(msilBytes, 3)
                : -1;

        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [REFLECT BASETYPE] ---
        
        
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
                if(type == typeof(MonoBehaviour) || type == typeof(ScriptableObject)) break;
            }

            var array = temp.ToArray();
            ConcurrentListPool<Type>.Release(temp);
            return array;
        }
        
        
        public static PropertyInfo GetPropertyInBaseType(this Type type, string name, BindingFlags flags)
        {
            var baseType = type;
            while (true)
            {
                try
                {
                    var propertyInfo = baseType?.GetProperty(name, flags);
                    if (propertyInfo != null)
                    {
                        return propertyInfo;
                    }
                    baseType = baseType?.BaseType;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        public static FieldInfo GetFieldInBaseType(this Type type, string name, BindingFlags flags)
        {
            var baseType = type;
            while (true)
            {
                try
                {
                    var fieldInfo = baseType?.GetField(name, flags);
                    if (fieldInfo != null)
                    {
                        return fieldInfo;
                    }
                    baseType = baseType?.BaseType;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        public static EventInfo GetEventInBaseType(this Type type, string name, BindingFlags flags)
        {
            var baseType = type;
            while (true)
            {
                try
                {
                    var eventInfo = baseType?.GetEvent(name, flags);
                    if (eventInfo != null)
                    {
                        return eventInfo;
                    }
                    baseType = baseType?.BaseType;
                }
                catch
                {
                    return null;
                }
            }
        }        
        
        #endregion
    }
}
