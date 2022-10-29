// Copyright (c) 2022 Jonathan Lang

using System;
using System.Diagnostics.Contracts;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Access monitoring UI API.
    /// </summary>
    public interface IMonitoringUI
    {
        /// <summary>
        /// Get or set the visibility of the current monitoring UI.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Event is invoked when the monitoring UI became visible/invisible
        /// </summary>
        event Action<bool> VisibleStateChanged;

        /// <summary>
        /// Get the current monitoring UI instance
        /// </summary>
        TMonitoringUI GetCurrent<TMonitoringUI>() where TMonitoringUI : MonitoringUI;

        /// <summary>
        /// ApplyFilter displayed units by their name, tags etc.
        /// </summary>
        void ApplyFilter(string filter);

        /// <summary>
        /// Reset active filter.
        /// </summary>
        void ResetFilter();

        /// <summary>
        /// Set the active MonitoringUI
        /// </summary>
        void SetActiveMonitoringUI(MonitoringUI monitoringUI);

        #region Obsolete

        [Obsolete("Use IMonitoringUI.Visible instead! This API will be removed in 4.0.0")]
        [Pure] bool IsVisible();

        [Obsolete("Use IMonitoringUI.Visible instead! This API will be removed in 4.0.0")]
        void Show();

        [Obsolete("Use IMonitoringUI.Visible instead! This API will be removed in 4.0.0")]
        void Hide();

        [Obsolete("Use IMonitoringUI.Visible instead! This API will be removed in 4.0.0")]
        bool ToggleDisplay();

        [Obsolete("Use IMonitoringUI.GetCurrent<T> instead! This API will be removed in 4.0.0")]
        MonitoringUIController GetActiveUIController();

        [Obsolete("Use IMonitoringUI.GetCurrent<T> instead! This API will be removed in 4.0.0")]
        TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController;

        [Obsolete("This API will be removed in 4.0.0")]
        void CreateMonitoringUI();

        #endregion
    }
}