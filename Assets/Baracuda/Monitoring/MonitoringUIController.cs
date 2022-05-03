using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Base class for monitoring ui controller.
    /// </summary>
    public abstract class MonitoringUIController : MonitoredBehaviour
    {
        #region --- Abstract ---

        public abstract bool IsVisible { get; }
        protected internal abstract void Show();
        protected internal abstract void Hide();

        /*
         * Unit Handling   
         */

        protected internal abstract void OnUnitDisposed(IMonitorUnit unit);
        protected internal abstract void OnUnitCreated(IMonitorUnit unit);

        /*
         * Filtering   
         */

        protected internal abstract void ResetFilter();
        protected internal abstract void Filter(string filter);

        #endregion
    }
}