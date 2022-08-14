// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Text;
using Baracuda.Monitoring.API;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    internal class MonitoringUISection : MonoBehaviour
    {
        private TMPMonitoringUIController _controller;
        private Transform _transform;
        
        private readonly Dictionary<string, MonitoringUIGroup> _namedGroups =
            new Dictionary<string, MonitoringUIGroup>();
        
        private readonly Dictionary<object, MonitoringUIGroup> _targetedGroups =
            new Dictionary<object, MonitoringUIGroup>();

        private readonly Dictionary<IMonitorUnit, MonitoringUIElement> _unitUIElements =
            new Dictionary<IMonitorUnit, MonitoringUIElement>(32);
            
        private readonly List<MonitoringUIBase> _children = new List<MonitoringUIBase>();
        private static readonly StringBuilder stringBuilder = new StringBuilder(64);

        /*
         * Setup   
         */
        
        private void Awake()
        {
            _controller = GetComponentInParent<TMPMonitoringUIController>();
            _transform = transform;
        }

        //--------------------------------------------------------------------------------------------------------------
        
        internal void AddChild(IMonitorUnit monitorUnit)
        {
            if (TryGetGroupForNewUnit(monitorUnit, out var uiGroup))
            {
                uiGroup.AddChild(monitorUnit);
            }
            else
            {
                var unitUIElement = _controller.GetElementFromPool();
                unitUIElement.SetParent(_transform);
                unitUIElement.SetActive(monitorUnit.Enabled);
                unitUIElement.Setup(monitorUnit);
                _unitUIElements.Add(monitorUnit, unitUIElement);
                _children.Add(unitUIElement);
            }
            
            _children.Sort(MonitoringUIBase.Comparison);
            for (var i = 0; i < _children.Count; i++)
            {
                _children[i].SetSiblingIndex(i);
            }
        }

        internal void RemoveChild(IMonitorUnit monitorUnit)
        {
            var profile = monitorUnit.Profile;
            var format = profile.FormatData;
            var groupName = format.Group;
            if (profile.FormatData.AllowGrouping)
            {
                MonitoringUIGroup uiGroup;
                if (profile.IsStatic)
                {
                    uiGroup = _namedGroups[groupName];
                    uiGroup.RemoveChild(monitorUnit);
                    if (uiGroup.ChildCount != 0)
                    {
                        return;
                    }
                    _namedGroups.Remove(groupName);
                    _children.Remove(uiGroup);
                    _controller.ReleaseGroupToPool(uiGroup);
                }
                else
                {
                    uiGroup = _targetedGroups[monitorUnit.Target];
                    uiGroup.RemoveChild(monitorUnit);
                    if (uiGroup.ChildCount != 0)
                    {
                        return;
                    }
                    _targetedGroups.Remove(monitorUnit.Target);
                    _children.Remove(uiGroup);
                    _controller.ReleaseGroupToPool(uiGroup);
                }
            }
            else
            {
                var unitUIElement = _unitUIElements[monitorUnit];
                _unitUIElements.Remove(monitorUnit);
                _children.Remove(unitUIElement);
                _controller.ReleaseElementToPool(unitUIElement);
            }
        }
        
        private bool TryGetGroupForNewUnit(IMonitorUnit monitorUnit, out MonitoringUIGroup uiGroup)
        {
            if (!monitorUnit.Profile.FormatData.AllowGrouping)
            {
                uiGroup = null;
                return false;
            }

            uiGroup = GetGroupForUnit(monitorUnit);

            return true;
        }

        private MonitoringUIGroup GetGroupForUnit(IMonitorUnit monitorUnit)
        {
            var profile = monitorUnit.Profile;
            var format = profile.FormatData;
            var groupName = format.Group;

            if (profile.IsStatic || groupName != null)
            {
                if (_namedGroups.TryGetValue(groupName, out var uiGroup))
                {
                    return uiGroup;
                }

                uiGroup = MakeGroup(groupName);
                _namedGroups.Add(groupName, uiGroup);
                return uiGroup;
            }
            else
            {
                if (_targetedGroups.TryGetValue(monitorUnit.Target, out var uiGroup))
                {
                    return uiGroup;
                }

                stringBuilder.Clear();
                stringBuilder.Append(profile.DeclaringType.Name);
                if (profile.DeclaringType.Name != monitorUnit.TargetName)
                {
                    stringBuilder.Append(' ');
                    stringBuilder.Append('|');
                    stringBuilder.Append(' ');
                    stringBuilder.Append(monitorUnit.TargetName);
                }
                uiGroup = MakeGroup(stringBuilder.ToString());
                _targetedGroups.Add(monitorUnit.Target, uiGroup);
                return uiGroup;
            }

            MonitoringUIGroup MakeGroup(string title)
            {
                var group = _controller.GetGroupFromPool();
                group.SetParent(_transform);
                group.SetGameObjectActive();
                group.SetupGroup(title, _controller);
                _children.Add(group);
                return group;
            }
        }

    }
}