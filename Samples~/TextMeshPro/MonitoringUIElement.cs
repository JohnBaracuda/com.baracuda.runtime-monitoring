// Copyright (c) 2022 Jonathan Lang

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Baracuda.Monitoring.TextMeshPro
{
    [DisableMonitoring]
    [RequireComponent(typeof(RectTransform))]
    internal class MonitoringUIElement : MonitoringUIBase
    {
        [SerializeField] private TMP_Text tmpText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Canvas backgroundCanvas;

        private Action<string> _update;
        private Action<bool> _toggle;
        private IMonitorHandle _monitorUnit;

        internal bool Enabled => _monitorUnit.Enabled;
        protected override int Order => _order;

        private int _order;
        private int _sortingOrder;

        private void Awake()
        {
            transform.localScale = Vector3.one;
            _toggle = gameObject.SetActive;
            _update = str => tmpText.text = str;
            _sortingOrder = backgroundCanvas.sortingOrder;
        }

        public void Setup(IMonitorHandle handle)
        {
            var controller = Monitor.UI.GetCurrent<TMPMonitoringUI>();

            Assert.IsNotNull(controller);

            _monitorUnit = handle;
            var format = handle.Profile.FormatData;

            tmpText.font = format.FontHash != 0
                ? controller.GetFontAsset(format.FontHash)
                : controller.GetDefaultFontAsset();

            if (format.BackgroundColor.HasValue)
            {
                backgroundImage.color = format.BackgroundColor.Value;
            }
            if (format.TextColor.HasValue)
            {
                tmpText.color = format.TextColor.Value;
            }
            if (format.FontSize > 0)
            {
                tmpText.fontSize = format.FontSize;
            }

            tmpText.richText = format.RichTextEnabled;
            tmpText.alignment = format.TextAlign.ToTextAlignmentOptions();
            _order = format.Order;

            handle.ValueUpdated += _update;
            handle.ActiveStateChanged += _toggle;
            _update(handle.GetState());
            _toggle(handle.Enabled);
        }

        private void OnEnable()
        {
            backgroundCanvas.sortingOrder = _sortingOrder;
        }
    }
}