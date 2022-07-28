// Copyright (c) 2022 Jonathan Lang

using System;
using System.Diagnostics.Contracts;

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringUI : IMonitoringSubsystem<IMonitoringUI>
    {
        /// <summary>
        /// Set the active monitoring display visible.
        /// </summary>
        void Show();

        /// <summary>
        /// Hide the active monitoring display.
        /// </summary>
        void Hide();

        /// <summary>
        /// Toggle the visibility of the active monitoring display.
        /// This method returns a value indicating the new visibility state.
        /// </summary>
        bool ToggleDisplay();

        /// <summary>
        /// Event is invoked when the monitoring UI became visible/invisible
        /// </summary>
        event Action<bool> VisibleStateChanged;

        /// <summary>
        /// Returns true if the there is an active monitoring display that is also visible.
        /// </summary>
        [Pure] bool IsVisible();

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/>
        /// </summary>
        [Pure] MonitoringUIController GetActiveUIController();

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/> as a concrete implementation of T.
        /// </summary>
        [Pure] TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController;
        
        /// <summary>
        /// Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the
        /// Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController
        /// is instantiated.
        /// </summary>
        void CreateMonitoringUI();

        /// <summary>
        /// ApplyFilter displayed units by their name, tags etc. 
        /// </summary>
        void ApplyFilter(string filter);

        /// <summary>
        /// Reset active filter.
        /// </summary>
        void ResetFilter();

        //--------------------------------------------------------------------------------------------------------------

        #region --- Oboslete ---

        [Obsolete("Use ApplyFilter(string filter) instead!")]
        void Filter(string filter);
        
        #endregion
    }
}