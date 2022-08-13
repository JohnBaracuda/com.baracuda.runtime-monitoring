// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using TMPro;
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
        
        [Header("FontName")]
        [SerializeField] private TMP_FontAsset defaultFont;
        [SerializeField] private TMP_FontAsset[] availableFonts;
        
        #endregion

        #region --- Fields ---

        private Stack<MonitoringUIElement> _uiElementPool;
        private Stack<MonitoringUIGroup> _uiGroupPool;
        
        
        private Transform _transform;
        private UIControllerComponents _components;
        private Canvas _canvas;

        private readonly Dictionary<int, TMP_FontAsset> _loadedFonts = new Dictionary<int, TMP_FontAsset>();

        #endregion

        #region --- Properties ---

        public TMP_FontAsset GetDefaultFontAsset() => defaultFont;

        public TMP_FontAsset GetFontAsset(int fontHash)
        {
            return _loadedFonts.TryGetValue(fontHash, out var fontAsset) ? fontAsset : defaultFont;
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
            _components = GetComponent<UIControllerComponents>();
            
            // Pools
            _uiElementPool = new Stack<MonitoringUIElement>(initialElementPoolSize);
            _uiGroupPool = new Stack<MonitoringUIGroup>(initialGroupPoolSize);
            
            _canvas = GetComponent<Canvas>();
            
            InitializeElementPool();
            InitializeGroupPool();
            
            ApplyStyleSettings();

            var utility = MonitoringSystems.Resolve<IMonitoringUtility>();
            
            for (var i = 0; i < availableFonts.Length; i++)
            {
                var fontAsset = availableFonts[i];
                var hash = fontAsset.name.GetHashCode();
                if (utility.IsFontHashUsed(hash))
                {
                    _loadedFonts.Add(hash, fontAsset);
                }
            }
            availableFonts = null;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Pooling ---

        internal MonitoringUIElement GetElementFromPool()
        {
            return _uiElementPool.Count > 0 ? _uiElementPool.Pop() : CreateEmptyElement();
        }

        internal MonitoringUIGroup GetGroupFromPool()
        {
            return _uiGroupPool.Count > 0 ? _uiGroupPool.Pop() : CreateEmptyGroup();
        }
        
        internal void ReleaseElementToPool(MonitoringUIElement element)
        {
            element.SetParent(_transform);
            element.SetGameObjectInactive();
            _uiElementPool.Push(element);
        }

        internal void ReleaseGroupToPool(MonitoringUIGroup uiGroup)
        {
            uiGroup.SetParent(_transform);
            uiGroup.SetGameObjectInactive();
            _uiGroupPool.Push(uiGroup);
        }

        #endregion

        #region --- Pooling Internal ---

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
            return Instantiate(_components.elementPrefab, transform);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MonitoringUIGroup CreateEmptyGroup()
        {
            return Instantiate(_components.groupPrefab, transform);
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
            // This is a fix to force canvas sorting order to update.
            // Calling Canvas.ForceUpdateCanvas does not do this.
            _canvas.sortingOrder = _canvas.sortingOrder;
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
            var section = GetSection(unit.Profile.FormatData.Position);
            section.AddChild(unit);
        }
        
        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            var section = GetSection(unit.Profile.FormatData.Position);
            section.RemoveChild(unit);
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Misc ---
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MonitoringUISection GetSection(UIPosition uiPosition)
        {
            switch (uiPosition)
            {
                case UIPosition.UpperLeft:
                    return _components.upperLeftSection;
                case UIPosition.UpperRight:
                    return _components.upperRightSection;
                case UIPosition.LowerLeft:
                    return _components.lowerLeftSection;
                case UIPosition.LowerRight:
                    return _components.lowerRightSection;
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
                _components.upperLeftSection.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                _components.upperRightSection.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                _components.lowerLeftSection.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
                _components.lowerRightSection.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
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
