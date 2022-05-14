using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.UI.UnityUI
{
    /// <summary>
    /// Disclaimer:
    /// This class is showing the base for a Unity UI based monitoring UI Controller.
    /// It is not complete!
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class UnityMonitoringUIController : MonitoringUIController
    {
        public override bool IsVisible()
        {
            return gameObject.activeInHierarchy;
        }

        protected override void ShowMonitoringUI()
        {
            gameObject.SetActive(true);
        }

        protected override void HideMonitoringUI()
        {
            gameObject.SetActive(false);
        }

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            
        }

        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            
        }

        protected override void ResetFilter()
        {
            
        }

        protected override void Filter(string filter)
        {
            
        }
    }
}
