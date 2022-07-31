// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.API
{
    [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
    public static class MonitoringUI
    {
        #region --- API ---

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static void Show()
        {
#if !DISABLE_MONITORING
            MonitoringSystems.Resolve<IMonitoringUI>().Show();
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static void Hide()
        {
#if !DISABLE_MONITORING
            MonitoringSystems.Resolve<IMonitoringUI>().Hide();
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static bool ToggleDisplay()
        {
#if !DISABLE_MONITORING
            return MonitoringSystems.Resolve<IMonitoringUI>().ToggleDisplay();
#else
            return false;
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static bool IsVisible()
        {
#if !DISABLE_MONITORING
            return MonitoringSystems.Resolve<IMonitoringUI>().IsVisible();
#else
            return false;
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static MonitoringUIController GetActiveUIController()
        {
#if !DISABLE_MONITORING
            return MonitoringSystems.Resolve<IMonitoringUI>().GetActiveUIController();
#else
            return null;
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static T GetActiveUIController<T>() where T : MonitoringUIController
        {
#if !DISABLE_MONITORING
            return MonitoringSystems.Resolve<IMonitoringUI>().GetActiveUIController<T>();
#else
            return null;
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static void CreateMonitoringUI()
        {
#if !DISABLE_MONITORING
            MonitoringSystems.Resolve<IMonitoringUI>().CreateMonitoringUI();
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static void Filter(string filter)
        {
#if !DISABLE_MONITORING
            MonitoringSystems.Resolve<IMonitoringUI>().ApplyFilter(filter);
#endif
        }

        [Obsolete("Use IMonitoringUI instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUI>()")]
        public static void ResetFilter()
        {
#if !DISABLE_MONITORING
            MonitoringSystems.Resolve<IMonitoringUI>().ResetFilter();
#endif
        }

        #endregion
    }
}
