// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    
    
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UIControllerComponents))]
    public class TMPMonitoringUIController : MonitoringUIController
    {
        #region --- Inspector ---

        [Header("Pooling")]
        [SerializeField][Min(1)] private int initialElementPoolSize = 100;
        [SerializeField][Min(1)] private int initialGroupPoolSize = 100;

        [Header("Style")]
        [SerializeField] private float elementSpacing = 0;
        [SerializeField] private int marginTop;
        [SerializeField] private int marginLeft;
        [SerializeField] private int marginBottom;
        [SerializeField] private int marginRight;
        
        #endregion

        #region --- Fields ---

        private Stack<MonitoringUIElement> _uiElementPool;
        private Stack<MonitoringUIGroup> _uiGroupPool;
        private Dictionary<IMonitorUnit, MonitoringUIElement> _activeMonitoringUIElements;
        private Dictionary<object, MonitoringUIGroup> _activeGroups;
        private Dictionary<string, MonitoringUIGroup> _activeGroupsStr;
        private Transform _transform;
        private UIControllerComponents _components;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
            _components = GetComponent<UIControllerComponents>();
            _uiElementPool = new Stack<MonitoringUIElement>(initialElementPoolSize);
            _uiGroupPool = new Stack<MonitoringUIGroup>(initialGroupPoolSize);
            _activeMonitoringUIElements = new Dictionary<IMonitorUnit, MonitoringUIElement>();
            _activeGroups = new Dictionary<object, MonitoringUIGroup>(initialGroupPoolSize);
            _activeGroupsStr = new Dictionary<string, MonitoringUIGroup>(initialGroupPoolSize); 
            
            InitializeElementPool();
            InitializeGroupPool();
            
            ApplyStyleSettings();
        }

        #endregion

        #region --- Pooling ---

        private void InitializeElementPool()
        {
            for (var i = 0; i < initialElementPoolSize; i++)
            {
                var element = CreateEmptyElement();
                element.SetGameObjectInactive();
                element.SetParent(_transform);
                _uiElementPool.Push(element);
            }
        }

        private void InitializeGroupPool()
        {
            for (var i = 0; i < initialGroupPoolSize; i++)
            {
                var group = CreateEmptyGroup();
                group.SetGameObjectInactive();
                group.SetParent(_transform);
                _uiGroupPool.Push(group);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MonitoringUIElement CreateEmptyElement()
        {
            var emptyElement = Instantiate(_components.elementPrefab, transform);
            emptyElement.SetGameObjectInactive();
            return emptyElement;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MonitoringUIGroup CreateEmptyGroup()
        {
            var emptyGroup = Instantiate(_components.groupPrefab, transform);
            emptyGroup.SetGameObjectInactive();
            return emptyGroup;
        }

        private MonitoringUIElement GetElementFromPool()
        {
            return _uiElementPool.Count > 0 ? _uiElementPool.Pop() : CreateEmptyElement();
        }
        
        private MonitoringUIGroup GetGroupFromPool()
        {
            return _uiGroupPool.Count > 0 ? _uiGroupPool.Pop() : CreateEmptyGroup();
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Visibility ---
        
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

        #endregion

        #region --- Unit Creation / Disposing ---
        
        /*
         * Unit creation   
         */

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            var element = GetElementFromPool();
            var parent = SetupParentForUnit(unit);
            element.SetParent(parent);
            element.Setup(unit);
            element.SetGameObjectActive();
            _activeMonitoringUIElements.Add(unit, element);
        }
        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            var element = _activeMonitoringUIElements[unit];
            _activeMonitoringUIElements.Remove(unit);
            element.ResetElement();
            element.SetGameObjectInactive();
            element.SetParent(_transform);
            _uiElementPool.Push(element);

            if (_activeGroups.TryGetValue(unit.Target, out var uiGroup))
            {
                uiGroup.RemoveChild(unit);
                if (uiGroup.ChildCount > 0)
                {
                    return;
                }

                uiGroup.SetGameObjectInactive();
                uiGroup.SetParent(_transform);
                _uiGroupPool.Push(uiGroup);
            }
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Group Setup ---

        private Transform SetupParentForUnit(IMonitorUnit unit)
        {
            var profile = unit.Profile;
            var data = profile.FormatData;
            var position = data.Position;
            
            if (!data.AllowGrouping)
            {
                return GetPositionTransform(position);
            }

            if (profile.IsStatic)
            {
                var groupName = data.Group;

                if (_activeGroupsStr.TryGetValue(groupName, out var uiGroup))
                {
                    uiGroup.AddChild(unit);
                    return uiGroup.transform;
                }

                uiGroup = GetGroupFromPool();
                uiGroup.SetTitle(groupName);
                uiGroup.SetParent(GetPositionTransform(position));
                uiGroup.SetGameObjectActive();
                uiGroup.AddChild(unit);
                _activeGroupsStr.Add(groupName, uiGroup);
                return uiGroup.transform;
            }
            else
            {
                var groupName = data.Group;

                if (_activeGroups.TryGetValue(unit.Target, out var uiGroup))
                {
                    uiGroup.AddChild(unit);
                    return uiGroup.transform;
                }

                uiGroup = GetGroupFromPool();
                var title = $"{groupName} | {(unit.Target is UnityEngine.Object obj ? obj.name : unit.Target.ToString())}";
                uiGroup.SetTitle(title);
                uiGroup.SetParent(GetPositionTransform(position));
                uiGroup.SetGameObjectActive();
                uiGroup.AddChild(unit);
                _activeGroups.Add(unit.Target, uiGroup);
                return uiGroup.transform;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Transform GetPositionTransform(UIPosition uiPosition)
        {
            switch (uiPosition)
            {
                case UIPosition.UpperLeft:
                    return _components.upperLeftTransform;
                case UIPosition.UpperRight:
                    return _components.upperRightTransform;
                case UIPosition.LowerLeft:
                    return _components.lowerLeftTransform;
                case UIPosition.LowerRight:
                    return _components.lowerRightTransform;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Styling ---

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyStyleSettings();
        }
#endif
        
        private void ApplyStyleSettings()
        {
            try
            {
                _components = _components ? _components : GetComponent<UIControllerComponents>();
                _components.upperLeftTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                _components.upperRightTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                _components.lowerLeftTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                _components.lowerRightTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                GetComponent<HorizontalOrVerticalLayoutGroup>().padding =
                    new RectOffset(marginLeft, marginRight, marginTop, marginBottom);
            }
            catch
            {
                // ignored
            }
        }
        
        #endregion
    }
}
