// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [RequireComponent(typeof(RectTransform))]
    internal class MonitoringUIGroup : MonitoringUIParentElement
    {
        public int ChildCount { get; private set; }
        
        internal override int Order { get; }
        
        [SerializeField] private TMP_Text groupTitle;
        [SerializeField] private Image backgroundImage;

        private Transform _transform;
        private TMPMonitoringUIController _controller;
        private Action<bool> _checkVisibility;

        private readonly List<IMonitorUnit> _children = new List<IMonitorUnit>(8);

        private void Awake()
        {
            _transform = transform;
            _checkVisibility = childVisible => 
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
            };
        }

        public void SetupGroup(string title, TMPMonitoringUIController controller)
        {
            groupTitle.text = title;
            _controller = controller;
        }

        public void AddChild(IMonitorUnit monitorUnit)
        {
            var element = _controller.GetElementFromPool();
            element.SetParent(_transform);
            element.SetActive(monitorUnit.Enabled);
            element.Setup(monitorUnit);

            monitorUnit.ActiveStateChanged += _checkVisibility;
            _children.Add(monitorUnit);
            
            ChildCount++;
            backgroundImage.color = monitorUnit.Profile.FormatData.GroupColor.GetValueOrDefault(backgroundImage.color);
        }
    }
}
