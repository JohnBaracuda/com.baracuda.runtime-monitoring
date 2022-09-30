// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Base class for monitoring ui controller.
    /// </summary>
    public abstract class MonitoringUIController : MonitoredSingleton<MonitoringUIController>
    {
        /// <summary>
        /// Return true if the UI is active and false if it is not active.
        /// </summary>
        public abstract bool IsVisible();

        /// <summary>
        /// Activate / show the ui.
        /// </summary>
        public abstract void ShowMonitoringUI();

        /// <summary>
        /// Deactivate / hide the ui.
        /// </summary>
        public abstract void HideMonitoringUI();

        /*
         * Unit Handling
         */

        /// <summary>
        /// Use to add UI elements for the passed unit.
        /// </summary>
        public abstract void OnUnitCreated(IMonitorUnit unit);

        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        public abstract void OnUnitDisposed(IMonitorUnit unit);
    }
}