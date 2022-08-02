// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [RequireComponent(typeof(RectTransform))]
    internal class MonitoringUIGroup : MonitoringUIBase
    {
        [SerializeField] private TMP_Text groupTitle;
        [SerializeField] private Image backgroundImage;

        internal int ChildCount { get; private set; } = 0;
        protected override int Order => _order;

        private Transform _transform;
        private TMPMonitoringUIController _controller;
        private Action<bool> _checkVisibility;
        private int _order = 0;
        
        

        private readonly List<MonitoringUIElement> _children = new List<MonitoringUIElement>(8);
        private readonly Dictionary<IMonitorUnit, MonitoringUIElement> _unitUIElements = new Dictionary<IMonitorUnit, MonitoringUIElement>(32);


        private void Awake()
        {
            _transform = transform;
            _checkVisibility = CheckVisibility;
        }
        
        public void SetupGroup(string title, TMPMonitoringUIController controller)
        {
            groupTitle.text = title;
            _controller = controller;
            _order = 0;
        }

        public void AddChild(IMonitorUnit monitorUnit)
        {
            var unitUIElement = _controller.GetElementFromPool();
            unitUIElement.SetParent(_transform);
            unitUIElement.SetActive(monitorUnit.Enabled);
            unitUIElement.Setup(monitorUnit);

            var formatData = monitorUnit.Profile.FormatData;
            _order = formatData.GroupOrder != 0 ? formatData.GroupOrder : _order;
            _children.Add(unitUIElement);
            _children.Sort(Comparison);
            _unitUIElements.Add(monitorUnit, unitUIElement);
            ChildCount++;
            
            for (var i = 0; i < _children.Count; i++)
            {
                _children[i].SetSiblingIndex(i + 1);
            }
            monitorUnit.ActiveStateChanged += _checkVisibility;
            backgroundImage.color = formatData.GroupColor.GetValueOrDefault(backgroundImage.color);
            CheckVisibility(monitorUnit.Enabled);
        }

        public void RemoveChild(IMonitorUnit monitorUnit)
        {
            var unitUIElement = _unitUIElements[monitorUnit];
            _unitUIElements.Remove(monitorUnit);
            _children.Remove(unitUIElement);
            _controller.ReleaseElementToPool(unitUIElement);
            ChildCount--;
            CheckVisibility(false);
        }
        
        private void CheckVisibility(bool childVisible)
        {
            gameObject.SetActive(childVisible || IsAnyChildVisible());
            bool IsAnyChildVisible()
            {
                var visible = false;
                for (var i = 0; i < _children.Count; i++)
                {
                    if (_children[i].Enabled)
                    {
                        visible = true;
                        break;
                    }
                }
                return visible;
            }
        }
    }
}
