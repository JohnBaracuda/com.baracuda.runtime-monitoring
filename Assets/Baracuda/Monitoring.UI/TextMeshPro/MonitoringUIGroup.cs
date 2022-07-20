using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [RequireComponent(typeof(RectTransform))]
    public class MonitoringUIGroup : MonoBehaviour
    {
        [SerializeField] private TMP_Text groupTitle;
        [SerializeField] private Image backgroundImage; 
        
        private readonly List<IMonitorUnit> _children = new List<IMonitorUnit>(10);
        public int ChildCount => _children.Count;


        public void AddChild(IMonitorUnit unit)
        {
            if (unit.Profile.TryGetMetaAttribute<MGroupColorAttribute>(out var colorAttribute))
            {
                backgroundImage.color = colorAttribute.ColorValue;
            }

            _children.Add(unit);
            unit.ActiveStateChanged += EvaluateActiveState;
        }
        
        public void RemoveChild(IMonitorUnit unit)
        {
            _children.Remove(unit);
            unit.ActiveStateChanged -= EvaluateActiveState;
        }
        
        public void SetTitle(string title)
        {
            groupTitle.text = title;
        }

        private void EvaluateActiveState(bool activeState)
        {
            gameObject.SetActive(activeState || _children.Any(unit => unit.Enabled));
        }
    }
}
