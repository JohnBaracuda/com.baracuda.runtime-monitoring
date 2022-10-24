// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.UIToolkit
{
    public interface IMonitoringUIElement
    {
        IMonitorHandle Handle { get; }
        string[] Tags { get; }
    }
}