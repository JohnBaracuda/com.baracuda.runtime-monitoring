// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Base class for monitoring ui controller.
    /// </summary>
    public abstract class MonitoringUIController : MonitoredBehaviour
    {
        #region --- Abstract ---

        /// <summary>
        /// Return true if the UI is active and false if it is not active.
        /// </summary>
        public abstract bool IsVisible();
        
        /// <summary>
        /// Activate / show the ui.
        /// </summary>
        protected internal abstract void ShowMonitoringUI();
        
        /// <summary>
        /// Deactivate / hide the ui.
        /// </summary>
        protected internal abstract void HideMonitoringUI();

        /*
         * Unit Handling   
         */

        /// <summary>
        /// Use to add UI elements for the passed unit.
        /// </summary>
        protected internal abstract void OnUnitCreated(IMonitorUnit unit);
        
        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        protected internal abstract void OnUnitDisposed(IMonitorUnit unit);

        /*
         * Filtering   
         */

        /// <summary>
        /// Use to reset active filter if any are applied.
        /// </summary>
        protected internal virtual void ResetFilter()
        {
        }
        
        /// <summary>
        /// Use to filter displayed units by their name, tags etc. 
        /// </summary>
        protected internal virtual void Filter(string filter)
        {
        }

        #endregion
    }
}