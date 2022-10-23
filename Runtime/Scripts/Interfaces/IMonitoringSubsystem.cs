// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    [Obsolete("This API will be removed in 4.0.0")]
    public interface IMonitoringSubsystem<T> where T : class, IMonitoringSubsystem<T>
    {
    }
}