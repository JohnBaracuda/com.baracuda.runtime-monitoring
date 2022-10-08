// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValidatorFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToHumanizedString(MemberInfo member)
        {
            return $"{member.Name} in {member.DeclaringType?.Name}";
        }
        
        private static void CheckComparisonViability<TValue>(Comparison condition, object other, MemberInfo memberInfo)
        {
            Debug.Assert(other.TryConvert<object, TValue>(out _), $"{ToHumanizedString(memberInfo)}'s return type cannot be converted to {other.GetType()}");
            
            var monitoredType = typeof(TValue);
            var comparedType = other.GetType();
            
            switch (condition)
            {
                case Comparison.Equals:
                case Comparison.EqualsNot:
                    break;
                case Comparison.Greater:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: return value is not a numeric type! Cannot use Comparison.Greater!");
                    Debug.Assert(comparedType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: Compared type is not a numeric type! Cannot use Comparison.Greater!");
                    break;
                case Comparison.GreaterOrEqual:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: return value is not a numeric type! Cannot use Comparison.GreaterOrEqual!");
                    Debug.Assert(comparedType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: Compared type is not a numeric type! Cannot use Comparison.GreaterOrEqual!");
                    break;
                case Comparison.Lesser:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: return value is not a numeric type! Cannot use Comparison.Lesser!");
                    Debug.Assert(comparedType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: Compared type is not a numeric type! Cannot use Comparison.Lesser!");
                    break;
                case Comparison.LesserOrEqual:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: return value is not a numeric type! Cannot use Comparison.LesserOrEqual!");
                    Debug.Assert(comparedType.IsNumeric(), $"{ToHumanizedString(memberInfo)}: Compared type is not a numeric type! Cannot use Comparison.LesserOrEqual!");
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
                    Debug.Assert(monitoredType == typeof(bool), $"{ToHumanizedString(memberInfo)} is not a boolean! Cannot use Condition.True!");
                    break;
                case Condition.False:
                    Debug.Assert(monitoredType == typeof(bool), $"{ToHumanizedString(memberInfo)} is not a boolean! Cannot use Condition.False!");
                    break;
                case Condition.Null:
                    Debug.Assert(!monitoredType.IsValueType, $"{ToHumanizedString(memberInfo)} is not a reference type! Cannot use Condition.Null!");
                    break;
                case Condition.NotNull:
                    Debug.Assert(!monitoredType.IsValueType, $"{ToHumanizedString(memberInfo)} is not a reference type! Cannot useCondition.NotNull!");
                    break;
                case Condition.NotZero:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)} is not a numeric type! Cannot use Condition.NotZero!");
                    break;
                case Condition.Zero:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)} is not a numeric type! Cannot use Condition.Zero!");
                    break;
                case Condition.Negative:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)} is not a numeric type! Cannot use Condition.Negative!");
                    break;
                case Condition.Positive:
                    Debug.Assert(monitoredType.IsNumeric(), $"{ToHumanizedString(memberInfo)} is not a numeric type! Cannot use Condition.Positive!");
                    break;
                case Condition.NotNullOrEmptyString:
                    Debug.Assert(monitoredType == typeof(string), $"{ToHumanizedString(memberInfo)} is not a string! Cannot use Condition.NotNullOrEmptyString!");
                    break;
                case Condition.NotNullOrWhiteSpace:
                    Debug.Assert(monitoredType == typeof(string), $"{ToHumanizedString(memberInfo)} is not a string! Cannot use Condition.NotNullOrWhiteSpace!");
                    break;
                case Condition.CollectionNotEmpty:
                    Debug.Assert(monitoredType.HasInterface<IEnumerable>(), $"{ToHumanizedString(memberInfo)} is not a IEnumerable! Cannot use Condition.CollectionNotEmpty!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        
    }
}