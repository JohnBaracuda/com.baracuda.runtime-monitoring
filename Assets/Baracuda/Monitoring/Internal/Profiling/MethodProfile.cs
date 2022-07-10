// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public readonly struct MethodResult
    {
        public readonly bool IsVoid;
        public readonly string[] Results;

        public MethodResult(bool isVoid, string[] results)
        {
            IsVoid = isVoid;
            Results = results;
        }
    }
    
    public sealed class MethodProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Dictionary<int, ParameterInfo> _outParameter = new Dictionary<int, ParameterInfo>();

        private MethodProfile(
            MethodInfo methodInfo,
            MonitorAttribute attribute,
            MonitorProfileCtorArgs args) : base(methodInfo, attribute, typeof(TTarget), typeof(TValue),
            UnitType.Method, args)
        {
            var parameter = CreateParameterArray(methodInfo, attribute);
            _getValueDelegate = CreateGetDelegate(methodInfo, parameter);

            var parameters = methodInfo.GetParameters();
            for (var i = 0; i < parameter.Length; i++)
            {
                if (parameters[i].IsOut)
                {
                    _outParameter.Add(i, parameters[i]);
                }
            }
        }

        internal override MonitorUnit CreateUnit(object target)
        {
            return new MethodUnit<TTarget, TValue>(
                (TTarget)target, 
                _getValueDelegate,
                null,
                ValueProcessor((TTarget)target),
                this);
        }

        //--------------------------------------------------------------------------------------------------------------

        private static Func<TTarget, TValue> CreateGetDelegate(MethodInfo methodInfo, object[] parameter)
        {
            var isVoid = methodInfo.ReturnType == typeof(void);
            
            if (isVoid)
            {
                return target =>
                {
                    methodInfo.Invoke(target, parameter);
                    var data = new VoidRef(parameter);
                    return data.ConvertFast<VoidRef, TValue>();
                };
            }
            return target => (TValue) methodInfo.Invoke(target, parameter);
        }
        
        private static object[] CreateParameterArray(MethodInfo methodInfo, MonitorAttribute attribute)
        {
            var parameterInfos = methodInfo.GetParameters();
            var paramArray = new object[parameterInfos.Length];
            var monitorMethodAttribute = attribute as MonitorMethodAttribute;
            
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var current = parameterInfos[i];
                var currentType = current.ParameterType;
                if (monitorMethodAttribute?.Args?.Length > i && !current.IsOut)
                {
                    paramArray[i] = Convert.ChangeType(monitorMethodAttribute.Args[i] ?? currentType.GetDefault(), currentType);
                }
                else
                {
                    var defaultValue = current.HasDefaultValue? current.DefaultValue : currentType.GetDefault();
                    paramArray[i] = defaultValue;
                }
            }

            return paramArray;
        }
    }
}