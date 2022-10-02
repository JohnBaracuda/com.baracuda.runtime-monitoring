// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.UIToolkit
{
    public interface IMonitoringUIElement
    {
        IMonitorUnit Unit { get; }
        string[] Tags { get; }
    }
}