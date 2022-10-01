// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Interfaces;
using System;
using System.Threading;

namespace Baracuda.Monitoring.Core.Interfaces
{
    internal interface IMonitoringLogger : IMonitoringSubsystem<IMonitoringLogger>
    {
        void LogException(Exception exception);
        
        void LogBadImageFormatException(BadImageFormatException exception);

        void LogThreadAbortedException(ThreadAbortException exception);

        void LogOperationCancelledException(OperationCanceledException exception);

        void LogValueProcessNotFound(string processor, Type type);
        
        void LogInvalidProcessorSignature(string processor, Type type);
    }
}