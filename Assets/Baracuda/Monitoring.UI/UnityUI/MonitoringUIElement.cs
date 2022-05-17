// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.UnityUI
{
    internal class MonitoringUIElement : MonoBehaviour
    {
        private TMP_Text _tmpText;
        private Image _backgroundImage;
        private GameObject _gameObject;
        private Transform _transform;
        private IMonitorUnit _monitorUnit;
        private Action<string> _updateValue;

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            _backgroundImage = GetComponent<Image>();
            _gameObject = gameObject;
            _transform = transform;
            _updateValue = UpdateUI;
        }

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
        }

        public void Reset()
        {
            _monitorUnit.ValueUpdated -= _updateValue;
            _monitorUnit = null;
        }

        private void UpdateUI(string text)
        {
            _tmpText.text = text;
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- Misc ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Activate()
        {
            _gameObject.SetActive(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deactivate()
        {
            if(_gameObject == null)
            {
                return;
            }

            _gameObject.SetActive(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetParent(Transform parent)
        {
            if(_transform == null)
            {
                return;
            }
            
            _transform.SetParent(parent);
        }

        #endregion
    }
}