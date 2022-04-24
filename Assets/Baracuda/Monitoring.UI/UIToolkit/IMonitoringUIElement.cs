using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring.UI.UIToolkit
{
    public interface IMonitoringUIElement
    {
        IMonitorUnit Unit { get; }
        string[] Tags { get; }
        void SetVisible(bool value);
    }
}