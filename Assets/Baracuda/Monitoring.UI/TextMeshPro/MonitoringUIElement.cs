// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    internal class MonitoringUIElement : MonoBehaviour
    {
        #region --- Fields ---
        
        private TMP_Text _tmpText;
        private Image _backgroundImage; 
        private IMonitorUnit _monitorUnit;
        private Action<string> _updateValueAction;
        private Action<bool> _activeStateAction;
        private TMPMonitoringUIController _controller;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Base Setup ---

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            _backgroundImage = GetComponentInChildren<Image>();
            _updateValueAction = UpdateUI;
            _activeStateAction = UpdateActiveState;
        }
        
        #endregion

        #region --- Unit Setup ---

        internal void InjectController(TMPMonitoringUIController controller)
        {
            _controller = controller;
        }

        public void SetupForUnit(IMonitorUnit monitorUnit)
        {
            _monitorUnit = monitorUnit;
            var profile = monitorUnit.Profile;
            var format = profile.FormatData;
            
            if (profile.TryGetMetaAttribute<MBackgroundColorAttribute>(out var backgroundColor))
            {
                _backgroundImage.color = backgroundColor.ColorValue;
            }
            if (profile.TryGetMetaAttribute<MTextColorAttribute>(out var textColor))
            {
                _tmpText.color = textColor.ColorValue;
            }

            _tmpText.font = profile.TryGetMetaAttribute<MFontAttribute>(out var fontAttribute)
                ? _controller.GetFontAsset(fontAttribute.FontHash)
                : _controller.GetDefaultFontAsset();

            if (format.FontSize > 0)
            {
                _tmpText.fontSize = format.FontSize;
            }
            
            monitorUnit.ValueUpdated += _updateValueAction;
            monitorUnit.ActiveStateChanged += _activeStateAction;
            _updateValueAction(monitorUnit.GetState());
        }
        
        #endregion


        //--------------------------------------------------------------------------------------------------------------

        #region --- Update & ResetElement ---

        public void ResetElement()
        {
            _monitorUnit.ValueUpdated -= _updateValueAction;
            _monitorUnit.ActiveStateChanged -= _activeStateAction;
            _monitorUnit = null;
        }

        private void UpdateUI(string text)
        {
            _tmpText.text = text;
        }

        private void UpdateActiveState(bool activeState)
        {
            gameObject.SetActive(activeState);
        }
        
        #endregion
    }
}