// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
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
        private IMonitorUnit _monitorUnit;

        internal bool Enabled => _monitorUnit.Enabled;
        protected override int Order => _order;

        private int _order;
        private int _sortingOrder;

        private void Awake()
        {
            _toggle = gameObject.SetActive;
            _update = str => tmpText.text = str;
            _sortingOrder = backgroundCanvas.sortingOrder;
        }
        
        public void Setup(IMonitorUnit monitorUnit)
        {
            var controller = MonitoringSystems.Resolve<IMonitoringUI>().GetActiveUIController<TMPMonitoringUIController>();
            _monitorUnit = monitorUnit;
            var format = monitorUnit.Profile.FormatData;
            
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
            
            monitorUnit.ValueUpdated += _update;
            monitorUnit.ActiveStateChanged += _toggle;
            _update(monitorUnit.GetState());
            _toggle(monitorUnit.Enabled);
        }

        private void OnEnable()
        {
            backgroundCanvas.sortingOrder = _sortingOrder;
        }
    }
}