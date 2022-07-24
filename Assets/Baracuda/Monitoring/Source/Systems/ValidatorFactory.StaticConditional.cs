using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Source.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValidatorFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<TValue, bool> CreateStaticValidatorCondition<TValue>(MConditionalAttribute attribute,
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
        
        private Func<TValue, bool> CreateValidatorSpecialCondition<TValue>(Condition condition)
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
                
                case Condition.NotNullOrEmptyString:
                    return (Func<TValue, bool>)(Delegate)NotNullOrEmpty();
                
                case Condition.NotNullOrWhiteSpace:
                    return (Func<TValue, bool>)(Delegate)NotNullOrWhiteSpace();
                
                case Condition.NotEmpty:
                    if (typeof(TValue).HasInterface<ICollection>())
                    {
                        return (Func<TValue, bool>)(Delegate)NotEmptyCount();
                    }
                    return (Func<TValue, bool>)(Delegate)NotEmpty();
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }

            Func<bool, bool> True() => (value) => value;
            Func<bool, bool> False() => (value) => !value;
            Func<string, bool> NotNullOrEmpty() => (value) =>  !string.IsNullOrEmpty(value);
            Func<string, bool> NotNullOrWhiteSpace() => (value) =>  !string.IsNullOrWhiteSpace(value);
            Func<IEnumerable, bool> NotEmpty() => (value) => value?.GetEnumerator().MoveNext() ?? false;
            Func<ICollection, bool> NotEmptyCount() => (value) => value != null && value.Count > 0;
        }

        /*
         * Comparison
         */
        

        private Func<TValue, bool> CreateValidatorComparison<TValue>(Comparison comparison, object other)
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

        private Func<TValue, bool> CreateValidatorMethodOneArgument<TValue>(string memberName, Type baseType)
        {
            var method = baseType.GetMethod(memberName, STATIC_FLAGS);
            return method != null ? CreateValidatorFromStaticMethodOneArgument<TValue>(method) : null;
        }

        private Func<TValue, bool> CreateValidatorFromStaticMethodOneArgument<TValue>(MethodInfo methodInfo)
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
    }
}