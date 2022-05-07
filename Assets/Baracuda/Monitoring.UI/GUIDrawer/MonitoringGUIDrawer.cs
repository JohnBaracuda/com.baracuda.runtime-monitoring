using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.UI.GUIDrawer
{
    internal static class RichTextExt
    {
        public static string WithFontSize(this string str, int size)
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

    public class MonitoringGUIDrawer : MonitoringUIController
    {
        private readonly List<IMonitorUnit> _units = new List<IMonitorUnit>(100);

        private void OnGUI()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                GUILayout.Label(unit.GetStateFormatted.WithFontSize(unit.Profile.FormatData.FontSize));
            }
        }

        public override bool IsVisible() => enabled;
        
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

        protected override void ResetFilter()
        {
        }

        protected override void Filter(string filter)
        {
        }
    }
}
