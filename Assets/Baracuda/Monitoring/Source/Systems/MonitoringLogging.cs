// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Types;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal sealed class MonitoringLogging : IMonitoringLogger
    {
        private readonly LoggingLevel _processorNotFoundLoggingLevel;
        private readonly LoggingLevel _invalidProcessorSignatureLoggingLevel;
        private readonly LoggingLevel _threadAbortedLevel;
        private readonly LoggingLevel _operationCancelledLevel;
        private readonly LoggingLevel _badImageFormatLevel;
        private readonly LoggingLevel _defaultLevel;
        
        internal MonitoringLogging(IMonitoringSettings settings)
        {
            _processorNotFoundLoggingLevel = settings.LogProcessorNotFoundException;
            _invalidProcessorSignatureLoggingLevel = settings.LogInvalidProcessorSignatureException;
            _threadAbortedLevel = settings.LogThreadAbortException;
            _defaultLevel = settings.LogUnknownExceptions;
            _operationCancelledLevel = settings.LogOperationCanceledException;
            _badImageFormatLevel = settings.LogBadImageFormatException;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LogInternal(string message, LoggingLevel loggingLevel)
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
        private void LogInternal(Exception exception, LoggingLevel loggingLevel)
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

        public void LogException(Exception exception)
        {
            LogInternal(exception, _defaultLevel);
        }
        
        public void LogBadImageFormatException(BadImageFormatException exception)
        {
            LogInternal(exception, _badImageFormatLevel);
        }

        public void LogThreadAbortedException(ThreadAbortException exception)
        {
            LogInternal(exception, _threadAbortedLevel);
        }
        
        public void LogOperationCancelledException(OperationCanceledException exception)
        {
            LogInternal(exception, _operationCancelledLevel);
        }

        public void LogValueProcessNotFound(string processor, Type type)
        {
            var message = $"[ValueProcessor] Processor: {processor} in {type.Name} with a valid signature was not found! Note that only static methods are valid value processors";
            LogInternal(message, _processorNotFoundLoggingLevel);
        }
        
        public void LogInvalidProcessorSignature(string processor, Type type)
        {
            var message = $"[ValueProcessor] Processor: {processor} in {type.Name} does not have a valid value processor signature!";
            LogInternal(message, _invalidProcessorSignatureLoggingLevel);
        }
    }
}