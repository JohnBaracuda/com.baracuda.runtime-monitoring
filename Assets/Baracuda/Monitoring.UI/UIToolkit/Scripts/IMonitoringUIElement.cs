// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    public interface IMonitoringUIElement
    {
        IMonitorUnit Unit { get; }
        string[] Tags { get; }
        void SetVisible(bool value);
    }
}