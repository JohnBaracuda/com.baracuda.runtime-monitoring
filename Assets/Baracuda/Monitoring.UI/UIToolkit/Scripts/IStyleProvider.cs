// Copyright (c) 2022 Jonathan Lang
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