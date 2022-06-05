// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Utilities
{
    internal sealed class ExceptionLogging
    {
        private static ExceptionLogging instance = null;
        
        private readonly LoggingLevel _processorNotFoundLoggingLevel;
        private readonly LoggingLevel _invalidProcessorSignatureLoggingLevel;
        private readonly LoggingLevel _threadAbortedLevel;
        private readonly LoggingLevel _operationCancelledLevel;
        private readonly LoggingLevel _badImageFormatLevel;
        private readonly LoggingLevel _defaultLevel;
        
        internal static void Initialize(MonitoringSettings settings)
        {
            instance = new ExceptionLogging(settings);
        }

        private ExceptionLogging(MonitoringSettings settings)
        {
            _processorNotFoundLoggingLevel = settings.LogProcessorNotFoundException;
            _invalidProcessorSignatureLoggingLevel = settings.LogInvalidProcessorSignatureException;
            _threadAbortedLevel = settings.LogThreadAbortException;
            _defaultLevel = settings.LogUnknownExceptions;
            _operationCancelledLevel = settings.LogOperationCanceledException;
            _badImageFormatLevel = settings.LogBadImageFormatException;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogInternal(string message, LoggingLevel loggingLevel)
        {
            switch (loggingLevel)
            {
                case LoggingLevel.Message:
                    Debug.Log(message);
                    break;
                case LoggingLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LoggingLevel.Error:
                case LoggingLevel.Exception:
                    Debug.LogError(message);
                    break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogInternal(Exception exception, LoggingLevel loggingLevel)
        {
            switch (loggingLevel)
            {
                case LoggingLevel.Message:
                    Debug.Log(exception);
                    break;
                case LoggingLevel.Warning:
                    Debug.LogWarning(exception);
                    break;
                case LoggingLevel.Error:
                    Debug.LogError(exception);
                    break;
                case LoggingLevel.Exception:
                    Debug.LogException(exception);
                    break;
            }
        }

        internal static void LogException(Exception exception)
        {
            LogInternal(exception, instance._defaultLevel);
        }
        
        internal static void LogBadImageFormatException(BadImageFormatException exception)
        {
            LogInternal(exception, instance._badImageFormatLevel);
        }

        internal static void LogThreadAbortedException(ThreadAbortException exception)
        {
            LogInternal(exception, instance._threadAbortedLevel);
        }
        
        internal static void LogOperationCancelledException(OperationCanceledException exception)
        {
            LogInternal(exception, instance._operationCancelledLevel);
        }

        internal static void LogValueProcessNotFound(string processor, Type type)
        {
            var message = $"Processor: {processor} in {type.Name} was not found! Only static methods are valid value processors";
            LogInternal(message, instance._processorNotFoundLoggingLevel);
        }
        
        internal static void LogInvalidProcessorSignature(string processor, Type type)
        {
            var message = $"Processor: {processor} in {type.Name} does not have a valid value processor signature!";
            LogInternal(message, instance._invalidProcessorSignatureLoggingLevel);
        }
    }
}