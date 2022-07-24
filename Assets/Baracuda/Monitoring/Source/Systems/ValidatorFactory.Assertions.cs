using System;
using System.Collections;
using Baracuda.Monitoring.Source.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValidatorFactory
    {
        //TODO: Check Comparison Viability

        private static void CheckComparisonViability<TValue>(Comparison condition, object other, Type baseType)
        {
            Debug.Assert(other.TryConvert<object, TValue>(out _), "other.GetType() == Monitored Type");
        }
        
        private static void CheckConditionViability<TValue>(Condition condition, Type baseType)
        {
            //TODO: Better asserts
            var monitoredType = typeof(TValue);
            switch (condition)
            {
                case Condition.True:
                    Debug.Assert(monitoredType == typeof(bool), "monitoredType == typeof(bool)");
                    break;
                case Condition.False:
                    Debug.Assert(monitoredType == typeof(bool), "monitoredType == typeof(bool)");
                    break;
                case Condition.Null:
                    Debug.Assert(!monitoredType.IsValueType, "monitoredType.IsValueType == false");
                    break;
                case Condition.NotNull:
                    Debug.Assert(!monitoredType.IsValueType, "monitoredType.IsValueType == false");
                    break;
                case Condition.NotZero:
                    Debug.Assert(monitoredType.IsNumeric(), "monitoredType.IsNumeric()");
                    break;
                case Condition.Negative:
                    Debug.Assert(monitoredType.IsNumeric(), "monitoredType.IsNumeric()");
                    break;
                case Condition.Positive:
                    Debug.Assert(monitoredType.IsNumeric(), $"monitoredType.IsNumeric()" );
                    break;
                case Condition.NotNullOrEmptyString:
                    Debug.Assert(monitoredType == typeof(string), "monitoredType == typeof(string)");
                    break;
                case Condition.NotNullOrWhiteSpace:
                    Debug.Assert(monitoredType == typeof(string), "monitoredType == typeof(string)");
                    break;
                case Condition.CollectionNotEmpty:
                    Debug.Assert(monitoredType.HasInterface<IEnumerable>(), "monitoredType.HasInterface<IEnumerable>()");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        
    }
}