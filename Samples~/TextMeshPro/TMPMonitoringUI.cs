// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.TextMeshPro
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UIControllerComponents))]
    public class TMPMonitoringUI : MonitoringUI
    {
        #region Inspector

        [Header("Pooling")]
        [SerializeField] [Min(1)] private int initialElementPoolSize = 100;
        [SerializeField] [Min(1)] private int initialGroupPoolSize = 100;

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

        #region Fields

        private Stack<MonitoringUIElement> _uiElementPool;
        private Stack<MonitoringUIGroup> _uiGroupPool;


        private Transform _transform;
        private UIControllerComponents _components;
        private Canvas _canvas;

        private readonly Dictionary<int, TMP_FontAsset> _loadedFonts = new Dictionary<int, TMP_FontAsset>();

        #endregion

        #region Properties

        public TMP_FontAsset GetDefaultFontAsset() => defaultFont;

        public TMP_FontAsset GetFontAsset(int fontHash)
        {
            return _loadedFonts.TryGetValue(fontHash, out var fontAsset) ? fontAsset : defaultFont;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Setup

        protected override void Awake()
        {
            _components = GetComponent<UIControllerComponents>();
            _transform = transform;

            // Pools
            _uiElementPool = new Stack<MonitoringUIElement>(initialElementPoolSize);
            _uiGroupPool = new Stack<MonitoringUIGroup>(initialGroupPoolSize);

            _canvas = GetComponent<Canvas>();

            InitializeElementPool();
            InitializeGroupPool();

            ApplyStyleSettings();

            for (var i = 0; i < availableFonts.Length; i++)
            {
                var fontAsset = availableFonts[i];
                if (Monitor.Registry.UsedFonts.Contains(fontAsset.name))
                {
                    var hash = fontAsset.name.GetHashCode();
                    _loadedFonts.Add(hash, fontAsset);
                }
            }
            availableFonts = null;
            base.Awake();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Pooling

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

        #region Pooling Internal

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

        #region Visibility


        /// <summary>
        /// The visible state of the UI.
        /// </summary>
        public override bool Visible
        {
            get => gameObject.activeInHierarchy;
            set
            {
                gameObject.SetActive(value);
                _canvas.sortingOrder = _canvas.sortingOrder;
            }
        }

        #endregion

        #region Unit Creation / Disposing


        /// <summary>
        /// Use to add UI elements for the passed handle.
        /// </summary>
        protected override void OnMonitorHandleCreated(IMonitorHandle handle)
        {
            var section = GetSection(handle.Profile.FormatData.Position);
            section.AddChild(handle);
        }

        /// <summary>
        /// Use to remove UI elements for the passed handle.
        /// </summary>
        protected override void OnMonitorHandleDisposed(IMonitorHandle handle)
        {
            var section = GetSection(handle.Profile.FormatData.Position);
            section.RemoveChild(handle);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Misc

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

        #region Styling

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
