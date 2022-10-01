// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Interfaces;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    public interface IMonitoringUIElement
    {
        IMonitorUnit Unit { get; }
        string[] Tags { get; }
    }
}