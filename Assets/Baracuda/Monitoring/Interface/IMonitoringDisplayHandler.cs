namespace Baracuda.Monitoring.Interface
{
    public interface IMonitoringDisplayHandler
    {
        bool IsActive { get; }
        void Show();
        void Hide();
    }
}
