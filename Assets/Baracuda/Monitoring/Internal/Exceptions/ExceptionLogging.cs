// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Exceptions
{
    internal static class ExceptionLogging
    {
        private static readonly MonitoringSettings settings = Dispatcher.InvokeAsync(MonitoringSettings.GetInstance).Result;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogException<T>(T exception, LoggingLevel loggingLevel) where T : Exception
        {
            switch (loggingLevel)
            {
                case LoggingLevel.None:
                    break;
                case LoggingLevel.Message:
                    Debug.Log(exception.Message);
                    break;
                case LoggingLevel.Warning:
                    Debug.LogWarning(exception.Message);
                    break;
                case LoggingLevel.Error:
                    Debug.LogError(exception);
                    break;
                case LoggingLevel.Exception:
                    Debug.LogException(exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loggingLevel), loggingLevel, null);
            }
        }
        
        internal static void LogException<T>(T exception) where T : Exception
        {
            switch (exception)
            {
                case ProcessorNotFoundException processorNotFound:
                    LogException(processorNotFound, settings.LogProcessorNotFoundException);
                    break;
                
                case InvalidProcessorSignatureException invalidProcessorSignature:
                    LogException(invalidProcessorSignature, settings.LogInvalidProcessorSignatureException);
                    break;
            }
        }
    }
}