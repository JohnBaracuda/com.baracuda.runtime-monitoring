// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System.Collections.Generic;
using Baracuda.Monitoring.Interface;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.UI.GUIDrawer
{
    /// <summary>
    /// Disclaimer:
    /// This class is showing the base for a GUI based monitoring UI Controller.
    /// It is not complete!
    /// </summary>
    public class MonitoringGUIDrawer : MonitoringUIController
    {
        private readonly List<IMonitorUnit> _units = new List<IMonitorUnit>(100);

        private void OnGUI()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                GUILayout.Label(displayString);
            }
        }

        /*
         * Overrides   
         */

        public override bool IsVisible()
        {
            return enabled;
        }

        protected override void ShowMonitoringUI()
        {
            enabled = true;
        }

        protected override void HideMonitoringUI()
        {
            enabled = false;
        }

        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            _units.Remove(unit);
        }

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            _units.Add(unit);
        }

        /*
         * Misc   
         */
        
        public static string WithFontSize(string str, int size)
        {
            size = Mathf.Max(size, 14);
            var sb = StringBuilderPool.Get();
            sb.Append("<size=");
            sb.Append(size);
            sb.Append('>');
            sb.Append(str);
            sb.Append("</size>");
            return StringBuilderPool.Release(sb);
        }
    }
}
