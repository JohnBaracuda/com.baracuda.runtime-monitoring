namespace Baracuda.Monitoring.UI.UIToolkit
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