// Copyright (c) 2022 Jonathan Lang
 
using System;
using System.Collections;
using System.Reflection;
using Baracuda.Monitoring.Source.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValidatorFactory
    {
        private static void CheckComparisonViability<TValue>(Comparison condition, object other, MemberInfo memberInfo)
        {
            Debug.Assert(other.TryConvert<object, TValue>(out _), $"{memberInfo.ToHumanizedString()}'s return type cannot be converted to {other.GetType()}");
            
            var monitoredType = typeof(TValue);
            var comparedType = other.GetType();
            
            switch (condition)
            {
                case Comparison.Equals:
                case Comparison.EqualsNot:
                    break;
                case Comparison.Greater:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: return value is not a numeric type! Cannot use Comparison.Greater!");
                    Debug.Assert(comparedType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: Compared type is not a numeric type! Cannot use Comparison.Greater!");
                    break;
                case Comparison.GreaterOrEqual:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: return value is not a numeric type! Cannot use Comparison.GreaterOrEqual!");
                    Debug.Assert(comparedType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: Compared type is not a numeric type! Cannot use Comparison.GreaterOrEqual!");
                    break;
                case Comparison.Lesser:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: return value is not a numeric type! Cannot use Comparison.Lesser!");
                    Debug.Assert(comparedType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: Compared type is not a numeric type! Cannot use Comparison.Lesser!");
                    break;
                case Comparison.LesserOrEqual:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: return value is not a numeric type! Cannot use Comparison.LesserOrEqual!");
                    Debug.Assert(comparedType.IsNumeric(), $"{memberInfo.ToHumanizedString()}: Compared type is not a numeric type! Cannot use Comparison.LesserOrEqual!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        
        private static void CheckConditionViability<TValue>(Condition condition, MemberInfo memberInfo)
        {
            var monitoredType = typeof(TValue);
            switch (condition)
            {
                case Condition.True:
                    Debug.Assert(monitoredType == typeof(bool), $"{memberInfo.ToHumanizedString()} is not a boolean! Cannot use Condition.True!");
                    break;
                case Condition.False:
                    Debug.Assert(monitoredType == typeof(bool), $"{memberInfo.ToHumanizedString()} is not a boolean! Cannot use Condition.False!");
                    break;
                case Condition.Null:
                    Debug.Assert(!monitoredType.IsValueType, $"{memberInfo.ToHumanizedString()} is not a reference type! Cannot use Condition.Null!");
                    break;
                case Condition.NotNull:
                    Debug.Assert(!monitoredType.IsValueType, $"{memberInfo.ToHumanizedString()} is not a reference type! Cannot useCondition.NotNull!");
                    break;
                case Condition.NotZero:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()} is not a numeric type! Cannot use Condition.NotZero!");
                    break;
                case Condition.Negative:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()} is not a numeric type! Cannot use Condition.Negative!");
                    break;
                case Condition.Positive:
                    Debug.Assert(monitoredType.IsNumeric(), $"{memberInfo.ToHumanizedString()} is not a numeric type! Cannot use Condition.Positive!");
                    break;
                case Condition.NotNullOrEmptyString:
                    Debug.Assert(monitoredType == typeof(string), $"{memberInfo.ToHumanizedString()} is not a string! Cannot use Condition.NotNullOrEmptyString!");
                    break;
                case Condition.NotNullOrWhiteSpace:
                    Debug.Assert(monitoredType == typeof(string), $"{memberInfo.ToHumanizedString()} is not a string! Cannot use Condition.NotNullOrWhiteSpace!");
                    break;
                case Condition.CollectionNotEmpty:
                    Debug.Assert(monitoredType.HasInterface<IEnumerable>(), $"{memberInfo.ToHumanizedString()} is not a IEnumerable! Cannot use Condition.CollectionNotEmpty!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        
    }
}