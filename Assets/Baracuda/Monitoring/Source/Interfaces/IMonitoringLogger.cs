// Copyright (c) 2022 Jonathan Lang

using System;
using System.Threading;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Source.Interfaces
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