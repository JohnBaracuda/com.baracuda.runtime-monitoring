// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.TextMeshPro
{
    internal class MonitoringUIElement : MonoBehaviour
    {
        #region --- Fields ---
        
        private TMP_Text _tmpText;
        private IMonitorUnit _monitorUnit;
        private Action<string> _updateValue;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Base Setup ---

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            _updateValue = UpdateUI;
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
            
            monitorUnit.ValueUpdated += _updateValue;
            _updateValue(monitorUnit.GetState());
        }
        
        #endregion


        //--------------------------------------------------------------------------------------------------------------

        #region --- Update & ResetElement ---

        public void ResetElement()
        {
            _monitorUnit.ValueUpdated -= _updateValue;
            _monitorUnit = null;
        }

        private void UpdateUI(string text)
        {
            _tmpText.text = text;
        }
        
        #endregion
    }
}