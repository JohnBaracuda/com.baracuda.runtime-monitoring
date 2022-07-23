using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Core.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Core.Profiling
{
    internal static class ValidatorFactory
    {
        #region --- API ---

        internal static Func<bool> CreateStaticValidator(MConditionalAttribute attribute, Type baseType)
        {
            return CreateStaticValidatorInternal(attribute, baseType);
        }

        internal static Func<TTarget, bool> CreateInstanceValidator<TTarget>(MConditionalAttribute attribute)
        {
            return CreateInstanceValidatorInternal<TTarget>(attribute);
        }
        
        internal static Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MConditionalAttribute attribute, Type baseType)
        {
            return CreateStaticValidatorCondition<TValue>(attribute, baseType);
        }
        
        internal static Func<TTarget, TValue, bool> CreateInstanceConditionalValidator<TTarget, TValue>(MConditionalAttribute attribute, Type baseType)
        {
            return null;
        }

        
        internal static ValidationEvent CreateEventValidator(MConditionalAttribute attribute, Type baseType)
        {
            return CreateEventValidatorInternal(attribute, baseType);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Static Validator ---
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<bool> CreateStaticValidatorInternal(MConditionalAttribute attribute, Type baseType)
        {
            return attribute.ValidationMethod == ValidationMethod.ByMember ? CreateValidatorMethod(attribute.MemberName, baseType) : null;
        }

        private static Func<bool> CreateValidatorMethod(string name, Type baseType)
        {
            var method = baseType.GetMethod(name, STATIC_FLAGS);
            if (method != null)
            {
                return CreateValidatorFromStaticMethod(method);
            }

            var property = baseType.GetProperty(name, STATIC_FLAGS);
            if (property != null)
            {
                return CreateValidatorFromStaticProperty(property);
            }

            var field = baseType.GetField(name, STATIC_FLAGS);
            if (field != null)
            {
                return CreateValidatorFromStaticField(field);
            }
            
            return null;
        }

        /*
         * Method   
         */

        private static Func<bool> CreateValidatorFromStaticMethod(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            if (!methodInfo.IsStatic)
            {
                return null;
            }

            if (methodInfo.ReturnType != typeof(bool))
            {
                return null;
            }

            if (parameters.Length == 0)
            {
                return (Func<bool>) methodInfo.CreateDelegate(typeof(Func<bool>), null);
            }

            return null;
        }

        /*
         * Property   
         */

        private static Func<bool> CreateValidatorFromStaticProperty(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.IsStatic())
            {
                return null;
            }

            if (propertyInfo.PropertyType != typeof(bool))
            {
                return null;
            }

            return (Func<bool>) propertyInfo.GetMethod.CreateDelegate(typeof(Func<bool>), null);
        }

        /*
         * Field   
         */

        private static Func<bool> CreateValidatorFromStaticField(FieldInfo fieldInfo)
        {
            if (!fieldInfo.IsStatic)
            {
                return null;
            }

            if (fieldInfo.FieldType != typeof(bool))
            {
                return null;
            }

            return fieldInfo.CreateStaticGetter<bool>();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Instance Validator ---
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TTarget, bool> CreateInstanceValidatorInternal<TTarget>(MConditionalAttribute attribute)
        {
            return attribute.ValidationMethod == ValidationMethod.ByMember ? CreateInstanceValidatorMethod<TTarget>(attribute.MemberName) : null;
        }

        private static Func<TTarget, bool> CreateInstanceValidatorMethod<TTarget>(string name)
        {
            var targetType = typeof(TTarget);
            
            var method = targetType.GetMethod(name, INSTANCE_FLAGS);
            if (method != null)
            {
                return CreateValidatorFromInstanceMethod<TTarget>(method);
            }

            var property = targetType.GetProperty(name, INSTANCE_FLAGS);
            if (property != null)
            {
                return CreateValidatorFromInstanceProperty<TTarget>(property);
            }

            var field = targetType.GetField(name, INSTANCE_FLAGS);
            if (field != null)
            {
                return CreateValidatorFromInstanceField<TTarget>(field);
            }

            return null;
        }

        /*
         * Method   
         */

        private static Func<TTarget, bool> CreateValidatorFromInstanceMethod<TTarget>(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            if (methodInfo.IsStatic)
            {
                return null;
            }

            if (methodInfo.ReturnType != typeof(bool))
            {
                return null;
            }

            if (parameters.Length == 0)
            {
                return (Func<TTarget, bool>) methodInfo.CreateDelegate(typeof(Func<TTarget, bool>));
            }

            return null;
        }

        /*
         * Property   
         */

        private static Func<TTarget, bool> CreateValidatorFromInstanceProperty<TTarget>(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsStatic())
            {
                return null;
            }

            if (propertyInfo.PropertyType != typeof(bool))
            {
                return null;
            }

            return (Func<TTarget, bool>) propertyInfo.GetMethod.CreateDelegate(typeof(Func<TTarget, bool>));
        }

        /*
         * Field   
         */

        private static Func<TTarget, bool> CreateValidatorFromInstanceField<TTarget>(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                return null;
            }

            if (fieldInfo.FieldType != typeof(bool))
            {
                return null;
            }

            return fieldInfo.CreateGetter<TTarget, bool>();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Static Event Validator ---

        private static ValidationEvent CreateEventValidatorInternal(MConditionalAttribute attribute, Type baseType)
        {
            if (attribute.ValidationMethod != ValidationMethod.ByMember)
            {
                return null;
            }

            var eventInfo = baseType.GetEvent(attribute.MemberName, STATIC_FLAGS);

            if (eventInfo == null)
            {
                return null;
            }

            if (eventInfo.EventHandlerType != typeof(Action<bool>))
            {
                return null;
            }
            
            var addMethod    = (Action<Action<bool>>)eventInfo.GetAddMethod(true).CreateDelegate(typeof(Action<Action<bool>>));
            var removeMethod = (Action<Action<bool>>)eventInfo.GetRemoveMethod(true).CreateDelegate(typeof(Action<Action<bool>>));

            return new ValidationEvent(addMethod, removeMethod);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Static Validator One Argument <TValue> ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TValue, bool> CreateStaticValidatorCondition<TValue>(MConditionalAttribute attribute,
            Type baseType)
        {
            switch (attribute.ValidationMethod)
            {
                case ValidationMethod.ByMember:
                    return CreateValidatorMethodOneArgument<TValue>(attribute.MemberName, baseType);
                case ValidationMethod.Comparison:
                    return CreateValidatorComparison<TValue>(attribute.Comparison, attribute.Other);
                case ValidationMethod.Condition:
                    return CreateValidatorSpecialCondition<TValue>(attribute.Condition);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /*
         * Special Conditions   
         */
        
        private static Func<TValue, bool> CreateValidatorSpecialCondition<TValue>(Condition condition)
        {
            switch (condition)
            {
                case Condition.True:
                    return (Func<TValue, bool>)(Delegate)True();
                
                case Condition.False:
                    return (Func<TValue, bool>)(Delegate)False();
                
                case Condition.Null:
                    return (value) => value == null;
                
                case Condition.NotNull:
                    return (value) => value != null;
                
                case Condition.NotZero:
                    return typeof(TValue).IsNumeric()
                        ? (Func<TValue, bool>) ((value) => Comparer<TValue>.Default.Compare(value, default) != 0)
                        : null;

                case Condition.Negative:
                    return typeof(TValue).IsNumeric()
                        ? (Func<TValue, bool>) ((value) => Comparer<TValue>.Default.Compare(value, default) < 0)
                        : null;
                
                case Condition.Positive:
                    return typeof(TValue).IsNumeric()
                        ? (Func<TValue, bool>) ((value) => Comparer<TValue>.Default.Compare(value, default) > 0)
                        : null;
                
                case Condition.NotNullOrEmpty:
                    return (Func<TValue, bool>)(Delegate)NotNullOrEmpty();
                
                case Condition.NotNullOrWhiteSpace:
                    return (Func<TValue, bool>)(Delegate)NotNullOrWhiteSpace();
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }

            Func<bool, bool> True() => (value) => value;
            Func<bool, bool> False() => (value) => !value;
            Func<string, bool> NotNullOrEmpty() => (value) =>  !string.IsNullOrEmpty(value);
            Func<string, bool> NotNullOrWhiteSpace() => (value) =>  !string.IsNullOrWhiteSpace(value);
        }

        /*
         * Comparison
         */
        

        private static Func<TValue, bool> CreateValidatorComparison<TValue>(Comparison comparison, object other)
        {
            if (!other.TryConvert<object, TValue>(out var convertedOther))
            {
                return null;
            }
                
            switch (comparison)
            {
                case Comparison.Equals:
                    return (value) => EqualityComparer<TValue>.Default.Equals(value, convertedOther);
                case Comparison.EqualsNot:
                    return (value) => !EqualityComparer<TValue>.Default.Equals(value, convertedOther);
                case Comparison.Greater:
                    return (value) => Comparer<TValue>.Default.Compare(value, convertedOther) > 0;
                case Comparison.GreaterOrEqual:
                    return (value) => Comparer<TValue>.Default.Compare(value, convertedOther) >= 0;
                case Comparison.Lesser:
                    return (value) =>Comparer<TValue>.Default.Compare(value, convertedOther) < 0;
                case Comparison.LesserOrEqual:
                    return (value) => Comparer<TValue>.Default.Compare(value, convertedOther) <= 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }
        }


        /*
         * Method   
         */

        private static Func<TValue, bool> CreateValidatorMethodOneArgument<TValue>(string memberName, Type baseType)
        {
            var method = baseType.GetMethod(memberName, STATIC_FLAGS);
            return method != null ? CreateValidatorFromStaticMethodOneArgument<TValue>(method) : null;
        }

        private static Func<TValue, bool> CreateValidatorFromStaticMethodOneArgument<TValue>(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            if (!methodInfo.IsStatic)
            {
                return null;
            }

            if (methodInfo.ReturnType != typeof(bool))
            {
                return null;
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TValue))
            {
                return (Func<TValue, bool>) methodInfo.CreateDelegate(typeof(Func<TValue, bool>), null);
            }

            return null;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Reflection Fields ---

        private const BindingFlags STATIC_FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly;

        private const BindingFlags INSTANCE_FLAGS
            = BindingFlags.Default |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly |
              BindingFlags.Instance;

        #endregion
    }
}