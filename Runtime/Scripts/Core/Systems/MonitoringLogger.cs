// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal sealed class MonitoringLogger
    {
        private readonly LoggingLevel _processorNotFoundLoggingLevel;
        private readonly LoggingLevel _invalidProcessorSignatureLoggingLevel;
        private readonly LoggingLevel _threadAbortedLevel;
        private readonly LoggingLevel _operationCancelledLevel;
        private readonly LoggingLevel _badImageFormatLevel;
        private readonly LoggingLevel _defaultLevel;

        internal MonitoringLogger()
        {
            var settings = Monitor.Settings;
            _processorNotFoundLoggingLevel = settings.LogProcessorNotFoundException;
            _invalidProcessorSignatureLoggingLevel = settings.LogInvalidProcessorSignatureException;
            _threadAbortedLevel = settings.LogThreadAbortException;
            _defaultLevel = settings.LogUnknownExceptions;
            _operationCancelledLevel = settings.LogOperationCanceledException;
            _badImageFormatLevel = settings.LogBadImageFormatException;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string message, LogType logType, bool stackTrace = true)
        {
            var format = $"{"[Runtime Monitoring]".ColorizeString(new Color(0.5f, 0.53f, 1f))} {message}";
            var option = stackTrace ? LogOption.None : LogOption.NoStacktrace;
            Debug.LogFormat(logType, option, null, format, Array.Empty<object>());
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
            var message =
                $"[Runtime Monitoring] Processor: {processor} in {type.Name} with a valid signature was not found! Note that only static methods are valid value processors";
            LogInternal(message, _processorNotFoundLoggingLevel);
        }

        public void LogInvalidProcessorSignature(string processor, Type type)
        {
            var message =
                $"[Runtime Monitoring] Processor: {processor} in {type.Name} does not have a valid value processor signature!";
            LogInternal(message, _invalidProcessorSignatureLoggingLevel);
        }
    }
}