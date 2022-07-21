using System;
using System.Collections.Generic;
using System.Reflection;
using Baracuda.Monitoring.Core.Utilities;
using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.Core.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        private static readonly Dictionary<Type, Delegate> globalValueProcessors =
            new Dictionary<Type, Delegate>();
        
        internal static void AddGlobalValueProcessor(MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();

            if (!IsMethodValidGlobalValueProcessor(methodInfo, parameterInfos))
            {
                return;
            }

            var valueType = parameterInfos[1].ParameterType;
            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(IFormatData), valueType, typeof(string));
            var processor = methodInfo.CreateDelegate(delegateType);

            if (globalValueProcessors.ContainsKey(valueType))
            {
                Debug.LogWarning($"[GlobalValueProcessor] processor for {valueType.Name} is already defined!");
                return;
            }
            
            globalValueProcessors.Add(valueType, processor);
        }

        private static bool IsMethodValidGlobalValueProcessor(MethodInfo methodInfo, ParameterInfo[] parameterInfos)
        {
            Color GetColor() => new Color(0.65f, 0.65f, 1f);
            string GetAttachment() =>
                "Please ensure that a methods marked as a global value processor accept an IFormatData as their first argument, the type you want to process as a second argument and return a string!";

            if (parameterInfos.Length != 2)
            {
                var message =
                    $"[GlobalValueProcessor] parameter length mismatch for method! {methodInfo.DeclaringType?.Name.Colorize(GetColor())}.{methodInfo.Name.Colorize(GetColor())}\n{GetAttachment()}";
                Debug.LogWarning(message);
                return false;
            }

            if (parameterInfos[0].ParameterType != typeof(IFormatData))
            {
                var message =
                    $"[GlobalValueProcessor] first parameter is not of type IFormatData! {methodInfo.DeclaringType?.Name.Colorize(GetColor())}.{methodInfo.Name.Colorize(GetColor())}\n{GetAttachment()}";
                Debug.LogWarning(message);
                return false;
            }

            if (methodInfo.ReturnType != typeof(string))
            {
                var message =
                    $"[GlobalValueProcessor] method does not return a string! {methodInfo.DeclaringType?.Name.Colorize(GetColor())}.{methodInfo.Name.Colorize(GetColor())}\n{GetAttachment()}";
                Debug.LogWarning(message);
                return false;
            }

            return true;
        }
    }
}