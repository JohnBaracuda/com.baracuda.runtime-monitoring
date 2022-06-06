// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using TMPro;
using UnityEngine;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    internal class MonitoringUIElement : MonoBehaviour
    {
        #region --- Fields ---
        
        private TMP_Text _tmpText;
        private IMonitorUnit _monitorUnit;
        private Action<string> _updateValueAction;
        private Action<bool> _activeStateAction;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Base Setup ---

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            _updateValueAction = UpdateUI;
            _activeStateAction = UpdateActiveState;
        }
        
        #endregion

        #region --- Unit Setup ---

        public void Setup(IMonitorUnit monitorUnit)
        {
            _monitorUnit = monitorUnit;
            var profile = monitorUnit.Profile;
            var format = profile.FormatData;

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