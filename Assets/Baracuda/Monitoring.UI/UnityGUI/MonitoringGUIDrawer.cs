// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Utilities;
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
        
        private readonly List<GUIElement> _unitsUpperLeft = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsUpperRight = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsLowerLeft = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsLowerRight = new List<GUIElement>(100);

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
        
        private class GUIElement
        {
            public int ID { get; }
            public string Content { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
            public FormatData FormatData { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
            
            private readonly int _size;
            private readonly string _targetName;
            
            public GUIElement(IMonitorUnit unit)
            {
                unit.ValueUpdated += Update;
                FormatData = unit.Profile.FormatData;
                ID = unit.ID;
                _targetName = unit.Profile.UnitTargetType.Name;
                _size = Mathf.Max(FormatData.FontSize, 14);
                Update(unit.GetStateFormatted);
            }

            private void Update(string text)
            {
                var sb = StringBuilderPool.Get();
                sb.Append("<size=");
                sb.Append(_size);
                sb.Append('>');
                // sb.Append(_targetName);
                // sb.Append(" | ");
                sb.Append(text);
                sb.Append("</size>");
                Content = StringBuilderPool.Release(sb);
            }
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
            "Using the GUI MonitoringUIController may cause performance overhead! " +
            "It is recommended to use the TextMeshPro or UIToolkit based Controller instead! " +
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
                var guiElement = _unitsUpperLeft[i];
                var displayString = guiElement.Content;
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
                var guiElement = _unitsLowerLeft[i];
                var displayString = guiElement.Content;
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
                var guiElement = _unitsUpperRight[i];
                var displayString = guiElement.Content;
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
                var guiElement = _unitsLowerRight[i];
                var displayString = guiElement.Content;
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
                    _unitsUpperLeft.Add(new GUIElement(unit));
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Add(new GUIElement(unit));
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Add(new GUIElement(unit));
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Add(new GUIElement(unit));
                    break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUnitDisposedInternal(IMonitorUnit unit)
        {
            switch (unit.Profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                    _unitsUpperLeft.Remove(_unitsUpperLeft.First(element => element.ID == unit.ID));
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Remove(_unitsUpperRight.First(element => element.ID == unit.ID));
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Remove(_unitsLowerLeft.First(element => element.ID == unit.ID));
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Remove(_unitsLowerRight.First(element => element.ID == unit.ID));
                    break;
            }
        }
        
        #endregion
    }
}
