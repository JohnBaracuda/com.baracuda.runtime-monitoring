// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.TextMeshPro
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    internal class MonitoringUISection : MonoBehaviour
    {
        private TMPMonitoringUI _controller;
        private Transform _transform;

        private readonly Dictionary<string, MonitoringUIGroup> _namedGroups =
            new Dictionary<string, MonitoringUIGroup>();

        private readonly Dictionary<object, MonitoringUIGroup> _targetedGroups =
            new Dictionary<object, MonitoringUIGroup>();

        private readonly Dictionary<IMonitorHandle, MonitoringUIElement> _unitUIElements =
            new Dictionary<IMonitorHandle, MonitoringUIElement>(32);

        private readonly List<MonitoringUIBase> _children = new List<MonitoringUIBase>();
        private static readonly StringBuilder stringBuilder = new StringBuilder(64);

        /*
         * Setup
         */

        internal void Awake()
        {
            _controller = GetComponentInParent<TMPMonitoringUI>();
            _transform = transform;
        }

        //--------------------------------------------------------------------------------------------------------------

        internal void AddChild(IMonitorHandle handle)
        {
            if (TryGetGroupForNewUnit(handle, out var uiGroup))
            {
                uiGroup.AddChild(handle);
            }
            else
            {
                var unitUIElement = _controller.GetElementFromPool();
                unitUIElement.SetParent(_transform);
                unitUIElement.SetActive(handle.Enabled);
                unitUIElement.Setup(handle);
                _unitUIElements.Add(handle, unitUIElement);
                _children.Add(unitUIElement);
            }

            _children.Sort(MonitoringUIBase.Comparison);
            for (var i = 0; i < _children.Count; i++)
            {
                _children[i].SetSiblingIndex(i);
            }
        }

        internal void RemoveChild(IMonitorHandle monitorHandle)
        {
            var profile = monitorHandle.Profile;
            var format = profile.FormatData;
            var groupName = format.Group;
            if (profile.FormatData.AllowGrouping)
            {
                MonitoringUIGroup uiGroup;
                if (profile.IsStatic || groupName != null)
                {
                    uiGroup = _namedGroups[groupName];
                    uiGroup.RemoveChild(monitorHandle);
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
                    if (!_targetedGroups.TryGetValue(monitorHandle.Target, out uiGroup))
                    {
                        return;
                    }
                    uiGroup.RemoveChild(monitorHandle);
                    if (uiGroup.ChildCount != 0)
                    {
                        return;
                    }
                    _targetedGroups.Remove(monitorHandle.Target);
                    _children.Remove(uiGroup);
                    _controller.ReleaseGroupToPool(uiGroup);
                }
            }
            else
            {
                var unitUIElement = _unitUIElements[monitorHandle];
                _unitUIElements.Remove(monitorHandle);
                _children.Remove(unitUIElement);
                _controller.ReleaseElementToPool(unitUIElement);
            }
        }

        private bool TryGetGroupForNewUnit(IMonitorHandle monitorHandle, out MonitoringUIGroup uiGroup)
        {
            if (!monitorHandle.Profile.FormatData.AllowGrouping)
            {
                uiGroup = null;
                return false;
            }

            uiGroup = GetGroupForUnit(monitorHandle);

            return true;
        }

        private MonitoringUIGroup GetGroupForUnit(IMonitorHandle monitorUnit)
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
                if (profile.DeclaringType.Name != monitorUnit.DisplayName)
                {
                    stringBuilder.Append(' ');
                    stringBuilder.Append('|');
                    stringBuilder.Append(' ');
                    stringBuilder.Append(monitorUnit.DisplayName);
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