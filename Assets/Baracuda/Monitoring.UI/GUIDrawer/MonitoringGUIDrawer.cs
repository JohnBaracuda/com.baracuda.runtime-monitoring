using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.UI.GUIDrawer
{
    public class MonitoringGUIDrawer : MonitoringUIController
    {
        private void OnGUI()
        {
            GUI.TextField(new Rect(0, 0, 300, 100), "text");
        }

        public override bool IsVisible() => false;
        
        protected override void ShowMonitoringUI()
        {
            throw new System.NotImplementedException();
        }

        protected override void HideMonitoringUI()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            throw new System.NotImplementedException();
        }

        protected override void ResetFilter()
        {
            throw new System.NotImplementedException();
        }

        protected override void Filter(string filter)
        {
            throw new System.NotImplementedException();
        }
    }
}
