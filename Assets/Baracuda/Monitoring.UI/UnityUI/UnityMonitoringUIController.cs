using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.UI.UnityUI
{
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
