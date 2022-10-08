// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        private readonly Dictionary<Type, Delegate> _globalValueProcessors =
            new Dictionary<Type, Delegate>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddGlobalValueProcessorInternal(MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();

            if (!IsMethodValidGlobalValueProcessor(methodInfo, parameterInfos))
            {
                return;
            }

            var valueType = parameterInfos[1].ParameterType;
            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(IFormatData), valueType, typeof(string));
            var processor = methodInfo.CreateDelegate(delegateType);

            if (_globalValueProcessors.ContainsKey(valueType))
            {
                Debug.LogWarning($"[GlobalValueProcessor] processor for {valueType.Name} is already defined!");
                return;
            }

            _globalValueProcessors.Add(valueType, processor);
        }

        private bool IsMethodValidGlobalValueProcessor(MethodInfo methodInfo, ParameterInfo[] parameterInfos)
        {
            Color GetColor() => new Color(0.65f, 0.65f, 1f);
            string GetAttachment() =>
                "Please ensure that a methods marked as a global value processor accept an IFormatData as their first argument, the type you want to process as a second argument and return a string!";

            if (parameterInfos.Length != 2)
            {
                var message =
                    $"[GlobalValueProcessor] parameter length mismatch for method! {methodInfo.DeclaringType?.Name.ColorizeString(GetColor())}.{methodInfo.Name.ColorizeString(GetColor())}\n{GetAttachment()}";
                Debug.LogWarning(message);
                return false;
            }

            if (parameterInfos[0].ParameterType != typeof(IFormatData))
            {
                var message =
                    $"[GlobalValueProcessor] first parameter is not of type IFormatData! {methodInfo.DeclaringType?.Name.ColorizeString(GetColor())}.{methodInfo.Name.ColorizeString(GetColor())}\n{GetAttachment()}";
                Debug.LogWarning(message);
                return false;
            }

            if (methodInfo.ReturnType != typeof(string))
            {
                var message =
                    $"[GlobalValueProcessor] method does not return a string! {methodInfo.DeclaringType?.Name.ColorizeString(GetColor())}.{methodInfo.Name.ColorizeString(GetColor())}\n{GetAttachment()}";
                Debug.LogWarning(message);
                return false;
            }

            return true;
        }
    }
}