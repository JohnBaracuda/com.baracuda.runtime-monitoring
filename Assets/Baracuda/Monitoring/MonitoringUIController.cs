// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;

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
        
        #region --- Obsolete ---

        [Obsolete("use IMonitoringUI.ResetFilter() instead!")]
        protected virtual void ResetFilter()
        {
            throw new InvalidOperationException();
        }

        [Obsolete("use IMonitoringUI.ApplyFilter() instead!")]
        protected virtual void Filter(string filter)
        {
            throw new InvalidOperationException();
        }
        
        #endregion
    }
}