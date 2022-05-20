// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    internal interface IStyleProvider
    {
        string[] InstanceUnitStyles { get; }
        string[] InstanceGroupStyles { get; }
        string[] InstanceLabelStyles { get; }
        string[] StaticUnitStyles { get; }
        string[] StaticGroupStyles { get; }
        string[] StaticLabelStyles { get; }
    }
}