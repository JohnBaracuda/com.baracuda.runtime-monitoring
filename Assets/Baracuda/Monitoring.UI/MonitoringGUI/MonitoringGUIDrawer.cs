// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Interface;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.UI.MonitoringGUI
{
    /// <summary>
    /// Disclaimer:
    /// This class is showing the base for a GUI based monitoring UI Controller.
    /// </summary>
    public class MonitoringGUIDrawer : MonitoringUIController
    {
        [SerializeField] private MarginOrPadding windowMargin;
        [SerializeField] private MarginOrPadding elementPadding;
        
        [Space]
        [SerializeField] private float spacing = 2f;
        [SerializeField] private Color backgroundColor = Color.black;
        
        private readonly List<IMonitorUnit> _unitsUpperLeft = new List<IMonitorUnit>(100);
        private readonly List<IMonitorUnit> _unitsUpperRight = new List<IMonitorUnit>(100);
        private readonly List<IMonitorUnit> _unitsLowerLeft = new List<IMonitorUnit>(100);
        private readonly List<IMonitorUnit> _unitsLowerRight = new List<IMonitorUnit>(100);

        private readonly GUIContent _content = new GUIContent();
        private Texture2D _backgroundTexture;

        #region --- Nested ---

        [Serializable]
        public struct MarginOrPadding
        {
            public float top;
            public float bot;
            public float left;
            public float right;
        }
        
        #endregion
        
        private void Start()
        {
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, backgroundColor);
            _backgroundTexture.Apply();
        }

        private void OnGUI()
        {
            var style = GUI.skin.label;
            DrawUpperLeft(style);
            DrawUpperRight(style);
        }

        private void DrawUpperLeft(GUIStyle skin)
        {
            var xPos = windowMargin.left;
            var yPos = windowMargin.top;
            for (var i = 0; i < _unitsUpperLeft.Count; i++)
            {
                //TODO: create internal type to cache unit values and listen for updates
                var unit = _unitsUpperLeft[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                _content.text = displayString;
                
                var textRect = new Rect();
                var textDimensions = skin.CalcSize(_content);
                
                textRect.width = textDimensions.x;
                textRect.height = textDimensions.y;
                
                textRect.x = xPos;
                textRect.y = yPos;
                
                var elementRect = new Rect(textRect);
                elementRect.height += elementPadding.top + elementPadding.bot;
                elementRect.width += elementPadding.right + elementPadding.left;

                textRect.x += elementPadding.left;
                textRect.y += elementPadding.top;
                
                GUI.DrawTexture(elementRect, _backgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                yPos += elementRect.height + spacing;
            }
        }
        
        private void DrawUpperRight(GUIStyle skin)
        {
            var screenWidth = Screen.width;
            var xPos = screenWidth;
            var yPos = windowMargin.top;
            for (var i = 0; i < _unitsUpperRight.Count; i++)
            {
                var unit = _unitsUpperRight[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                _content.text = displayString;

                var textRect = new Rect();
                var textDimensions = skin.CalcSize(_content);
                
                textRect.width = textDimensions.x;
                textRect.height = textDimensions.y;
                
                textRect.x = xPos - (textRect.width + windowMargin.right + elementPadding.right);
                textRect.y = yPos;
                
                var elementRect = new Rect(textRect);
                elementRect.height += elementPadding.top + elementPadding.bot;
                elementRect.width += elementPadding.right + elementPadding.left;

                elementRect.x -= elementPadding.left;
                textRect.y += elementPadding.top;
                
                GUI.DrawTexture(elementRect, _backgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                yPos += elementRect.height + spacing;
            }
        }

        /*
         * Overrides   
         */

        public override bool IsVisible()
        {
            return enabled;
        }

        protected override void ShowMonitoringUI()
        {
            enabled = true;
        }

        protected override void HideMonitoringUI()
        {
            enabled = false;
        }

        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            switch (unit.Profile.FormatData.Position)
            {
                case UIPosition.TopLeft:
                    _unitsUpperLeft.Remove(unit);
                    break;
                case UIPosition.TopRight:
                    _unitsUpperRight.Remove(unit);
                    break;
                case UIPosition.BottomLeft:
                    _unitsLowerLeft.Remove(unit);
                    break;
                case UIPosition.BottomRight:
                    _unitsLowerRight.Remove(unit);
                    break;
            }
        }

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            switch (unit.Profile.FormatData.Position)
            {
                case UIPosition.TopLeft:
                    _unitsUpperLeft.Add(unit);
                    break;
                case UIPosition.TopRight:
                    _unitsUpperRight.Add(unit);
                    break;
                case UIPosition.BottomLeft:
                    _unitsLowerLeft.Add(unit);
                    break;
                case UIPosition.BottomRight:
                    _unitsLowerRight.Add(unit);
                    break;
            }
        }

        /*
         * Misc   
         */
        
        public static string WithFontSize(string str, int size)
        {
            size = Mathf.Max(size, 14);
            var sb = StringBuilderPool.Get();
            sb.Append("<size=");
            sb.Append(size);
            sb.Append('>');
            sb.Append(str);
            sb.Append("</size>");
            return StringBuilderPool.Release(sb);
        }
    }
}
