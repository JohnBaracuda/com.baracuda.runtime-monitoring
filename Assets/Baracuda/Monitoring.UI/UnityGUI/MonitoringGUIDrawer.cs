// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.UI.UnityGUI
{
    /// <summary>
    /// Disclaimer:
    /// This class is showing the base for a GUI based monitoring UI Controller.
    /// </summary>
    public class MonitoringGUIDrawer : MonitoringUIController
    {
        #region --- Inspector ---
        
        [Header("Element & Window Spacing")]
        [SerializeField] private float elementSpacing = 2f;
        [SerializeField] private MarginOrPadding windowMargin;
        [SerializeField] private MarginOrPadding elementPadding;
        
        [Header("Coloring")]
        [SerializeField] private Color backgroundColor = Color.black;

        [Header("Other")]
        [SerializeField] private bool logStartMessage = true;
        
        #endregion

        #region --- Fields, Properties & Nested Types ---
        
        private readonly List<IMonitorUnit> _unitsUpperLeft = new List<IMonitorUnit>(100);
        private readonly List<IMonitorUnit> _unitsUpperRight = new List<IMonitorUnit>(100);
        private readonly List<IMonitorUnit> _unitsLowerLeft = new List<IMonitorUnit>(100);
        private readonly List<IMonitorUnit> _unitsLowerRight = new List<IMonitorUnit>(100);

        private readonly GUIContent _content = new GUIContent();
        private Texture2D _backgroundTexture;

        private static float lastLowerLeftHeight;
        private static float lastLowerRightHeight;
        private static int topLeftRows;
        private static int topRightRows;
        
        [Serializable]
        public struct MarginOrPadding
        {
            public float top;
            public float bot;
            public float left;
            public float right;
        }
        
        private readonly ref struct ScreenData
        {
            public readonly float width;
            public readonly float height;
            public readonly float halfHeight;

            public ScreenData(float width, float height)
            {
                this.width = width;
                this.height = height;
                halfHeight = height * .5f;
            }
        }
        
        private readonly ref struct Context
        {
            public readonly GUIStyle style;

            public Context(GUIStyle style)
            {
                this.style = style;
            }
        }
        
#if UNITY_EDITOR
        private readonly string _warningMessage =
            "Using the GUI MonitoringUIController may cause serious performance overhead! " +
            "It is recommended to use either the TextMeshPro or UIToolkit based Controller instead! " +
            "\nYou can disable this message from the settings window: <b>Tools > RuntimeMonitoring > Settings: UI Controller > Log Start Message</b>";
#endif 
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        private void Start()
        {
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, backgroundColor);
            _backgroundTexture.Apply();
            
#if UNITY_EDITOR
            if (logStartMessage)
            {
                Debug.LogWarning(_warningMessage);
            }
#endif
        }
        
        #endregion

        #region --- GUI ---
        
        private void OnGUI()
        {
            var ctx = new Context(GUI.skin.label);
            var screenData = new ScreenData(Screen.width, Screen.height);
            DrawUpperLeft(ctx, screenData);
            DrawUpperRight(ctx, screenData);
            DrawLowerLeft(ctx, screenData);
            DrawLowerRight(ctx, screenData);
        }

        /*
         * Draw Left   
         */
        
        private void DrawUpperLeft(Context ctx, ScreenData screenData)
        {
            var xPos = windowMargin.left;
            var yPos = windowMargin.top;
            var maxWidth = 0f;
            topLeftRows = 1;
            
            for (var i = 0; i < _unitsUpperLeft.Count; i++)
            {
                var unit = _unitsUpperLeft[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                _content.text = displayString;
                
                var textRect = new Rect();
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (elementRect.y + elementRect.height + lastLowerLeftHeight > screenData.height && elementRect.height < screenData.height)
                {
                    topLeftRows++;
                    yPos = windowMargin.top;
                    xPos += maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                GUI.DrawTexture(elementRect, _backgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                yPos += elementRect.height + elementSpacing;
            }
            
            // local function
            Rect ElementRect(ref Rect textRect, Vector2 textDimensions, float x, float y)
            {
                textRect.width = textDimensions.x;
                textRect.height = textDimensions.y;
                textRect.x = x;
                textRect.y = y;

                var elementRect = new Rect(textRect);

                elementRect.height += elementPadding.top + elementPadding.bot;
                elementRect.width += elementPadding.right + elementPadding.left;
                textRect.x += elementPadding.left;
                textRect.y += elementPadding.top;
                return elementRect;
            }
        }
        
        private void DrawLowerLeft(Context ctx,  ScreenData screenData)
        {
            var xPos = windowMargin.left;
            var yPos = screenData.height - windowMargin.bot;
            var maxWidth = 0f;
            var maxHeight = 0f;
            
            for (var i = _unitsLowerLeft.Count - 1; i >= 0; i--)
            {
                var unit = _unitsLowerLeft[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                _content.text = displayString;
                
                var textRect = new Rect();
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (topLeftRows > 1 && screenData.height - yPos > screenData.halfHeight)
                {
                    yPos = screenData.height - windowMargin.bot;
                    xPos += maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                else if (topLeftRows == 1 && screenData.height - yPos > screenData.height)
                {
                    yPos = screenData.height - windowMargin.bot;
                    xPos += maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                
                GUI.DrawTexture(elementRect, _backgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                
                yPos -= elementRect.height + elementSpacing;
                maxHeight = Mathf.Max(screenData.height - yPos, maxHeight);
            }
            
            lastLowerLeftHeight = maxHeight;
            
            // local function
            Rect ElementRect(ref Rect textRect, Vector2 textDimensions, float x, float y)
            {
                textRect.width = textDimensions.x;
                textRect.height = textDimensions.y;
                textRect.x = x;
                textRect.y = y;

                var elementRect = new Rect(textRect);
                elementRect.height += elementPadding.top + elementPadding.bot;
                elementRect.width += elementPadding.right + elementPadding.left;

                textRect.x += elementPadding.left;
                textRect.y += elementPadding.top;

                textRect.y -= elementRect.height;
                elementRect.y -= elementRect.height;
                return elementRect;
            }
        }
        
        /*
         * Draw Right      
         */
        
        private void DrawUpperRight(Context ctx,  ScreenData screenData)
        { 
            var xPos = screenData.width;
            var yPos = windowMargin.top;
            var maxWidth = 0f;
            topRightRows = 1;
            
            for (var i = 0; i < _unitsUpperRight.Count; i++)
            {
                var unit = _unitsUpperRight[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                _content.text = displayString;

                var textRect = new Rect();
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (elementRect.y + elementRect.height + lastLowerRightHeight > screenData.height && elementRect.height < screenData.height)
                {
                    topRightRows++;
                    yPos = windowMargin.top;
                    xPos -= (maxWidth + elementSpacing);
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                
                GUI.DrawTexture(elementRect, _backgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                yPos += elementRect.height + elementSpacing;
            }
            
            // local function
            Rect ElementRect(ref Rect textRect, Vector2 textDimensions, float x, float y)
            {
                textRect.width = textDimensions.x;
                textRect.height = textDimensions.y;
                textRect.x = x - (textRect.width + windowMargin.right + elementPadding.right);
                textRect.y = y;

                var elementRect = new Rect(textRect);
                elementRect.height += elementPadding.top + elementPadding.bot;
                elementRect.width += elementPadding.right + elementPadding.left;

                elementRect.x -= elementPadding.left;
                textRect.y += elementPadding.top;
                return elementRect;
            }
        }
        
        private void DrawLowerRight(Context ctx,  ScreenData screenData)
        {
            var xPos = screenData.width;
            var yPos = screenData.height - windowMargin.bot;
            var maxWidth = 0f;
            var maxHeight = 0f;
            
            for (var i = _unitsLowerRight.Count - 1; i >= 0; i--)
            {
                var unit = _unitsLowerRight[i];
                var formatData = unit.Profile.FormatData;
                var displayString = WithFontSize(unit.GetStateFormatted, formatData.FontSize);
                _content.text = displayString;
                
                var textRect = new Rect();
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (topRightRows > 1 && screenData.height - yPos > screenData.halfHeight)
                {
                    yPos = screenData.height - windowMargin.bot;
                    xPos -= maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                else if (topRightRows == 1 && screenData.height - yPos > screenData.height)
                {
                    yPos = screenData.height - windowMargin.bot;
                    xPos -= maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                
                GUI.DrawTexture(elementRect, _backgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                
                yPos -= elementRect.height + elementSpacing;
                maxHeight = Mathf.Max(screenData.height - yPos, maxHeight);
            }
            
            lastLowerRightHeight = maxHeight;
            
            // local function
            Rect ElementRect(ref Rect textRect, Vector2 textDimensions, float x, float y)
            {
                textRect.width = textDimensions.x;
                textRect.height = textDimensions.y;
                textRect.x = x - (textRect.width + windowMargin.right + elementPadding.right);
                textRect.y = y;

                var elementRect = new Rect(textRect);
                elementRect.height += elementPadding.top + elementPadding.bot;
                elementRect.width += elementPadding.right + elementPadding.left;

                elementRect.x -= elementPadding.left;
                textRect.y -= elementRect.height;
                elementRect.y -= elementRect.height;
                return elementRect;
            }
        }
        
        #endregion

        #region --- Overrides ---

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
            OnUnitDisposedInternal(unit);
        }
        

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            OnUnitCreatedInternal(unit);
        }
        
        #endregion

        #region --- Unit Creating / Disposal ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUnitCreatedInternal(IMonitorUnit unit)
        {
            switch (unit.Profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                    _unitsUpperLeft.Add(unit);
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Add(unit);
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Add(unit);
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Add(unit);
                    break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUnitDisposedInternal(IMonitorUnit unit)
        {
            switch (unit.Profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                    _unitsUpperLeft.Remove(unit);
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Remove(unit);
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Remove(unit);
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Remove(unit);
                    break;
            }
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Misc ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string WithFontSize(string str, int size)
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

        #endregion
    }
}
