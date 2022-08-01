using System.Collections.Generic;
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
        
        private readonly Dictionary<string, MonitoringUIGroup> _staticGroups =
            new Dictionary<string, MonitoringUIGroup>();
        
        private readonly Dictionary<object, MonitoringUIGroup> _instanceGroups =
            new Dictionary<object, MonitoringUIGroup>();
        
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
                var element = _controller.GetElementFromPool();
                element.SetParent(_transform);
                element.SetActive(monitorUnit.Enabled);
                element.Setup(monitorUnit);
            }
            //Sort
        }

        internal void RemoveChild(IMonitorUnit monitorUnit)
        {
            
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
            
            if (profile.IsStatic)
            {
                if (!_staticGroups.TryGetValue(groupName, out var uiGroup))
                {
                    uiGroup = _controller.GetGroupFromPool();
                    uiGroup.SetParent(_transform);
                    uiGroup.SetGameObjectActive();
                    uiGroup.SetupGroup(groupName, _controller);
                    _staticGroups.Add(groupName, uiGroup);
                }
                return uiGroup;
            }
            else
            {
                if (!_instanceGroups.TryGetValue(monitorUnit.Target, out var uiGroup))
                {
                    uiGroup = _controller.GetGroupFromPool();
                    uiGroup.SetParent(_transform);
                    uiGroup.SetGameObjectActive();
                    uiGroup.SetupGroup(MakeTargetGroupName(monitorUnit.Target, groupName), _controller);
                    _instanceGroups.Add(monitorUnit.Target, uiGroup);
                }
                return uiGroup;
            }
        }

        private static string MakeTargetGroupName(object target, string groupName)
        {
            return $"{groupName} | {(target is UnityEngine.Object unityObject ? unityObject.name : target.ToString())}";
        }
    }
}