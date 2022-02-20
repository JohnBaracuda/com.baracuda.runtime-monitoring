namespace Baracuda.Monitoring.Interface
{
    public interface IMonitoringUI
    {
        bool IsActive { get; }
        void Show();
        void Hide();
    }
}
