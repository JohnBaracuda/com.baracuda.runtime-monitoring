// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal class MonitoringUIFallback : MonitoringUI
    {
        #region Type Definitions

        [Serializable]
        public struct MarginOrPadding
        {
            public float top;
            public float bot;
            public float left;
            public float right;

            public MarginOrPadding(float top, float bot, float left, float right)
            {
                this.top = top;
                this.bot = bot;
                this.left = left;
                this.right = right;
            }
        }

        private class GUIElement
        {
            public static readonly Comparison<GUIElement> Comparison =
                (lhs, rhs) => lhs.Order > rhs.Order ? -1 : lhs.Order < rhs.Order ? 1 : 0;

            public int Order { get; }
            public bool Enabled { get; private set; }
            public bool OverrideFont { get; }
            public Font Font { get; }
            public int ID { get; }
            public string Content { get; private set; }
            private IFormatData Format { get; }

            public Texture2D BackgroundTexture { get; }

            private readonly string _textColor;
            private readonly int _size;

            private static readonly Dictionary<Color, Texture2D> backgroundTexturePool =
                new Dictionary<Color, Texture2D>();

            private readonly StringBuilder _stringBuilder = new StringBuilder(100);

            public GUIElement(IMonitorHandle handle, MonitoringUIFallback ui)
            {
                handle.ActiveStateChanged += SetActive;
                Enabled = handle.Enabled;
                Format = handle.Profile.FormatData;
                ID = handle.UniqueID;

                Order = Format.Order;

                var backgroundColor = Format.BackgroundColor.GetValueOrDefault(ui.backgroundColor);

                if (Format.TextColor.HasValue)
                {
                    _textColor = ColorUtility.ToHtmlStringRGB(Format.TextColor.Value);
                    handle.ValueUpdated += UpdateColorized;
                }
                else
                {
                    handle.ValueUpdated += Update;
                }

                if (backgroundTexturePool.TryGetValueValidated(backgroundColor, out var texture))
                {
                    BackgroundTexture = texture;
                }
                else
                {
                    texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, backgroundColor);
                    texture.Apply();
                    backgroundTexturePool.Add(backgroundColor, texture);
                    BackgroundTexture = texture;
                }

                if (Format.FontHash != 0)
                {
                    var fontAsset = ui.GetFont(Format.FontHash);
                    if (fontAsset != null)
                    {
                        OverrideFont = true;
                        Font = fontAsset;
                    }
                }

                _size = Format.FontSize > 0
                    ? Format.FontSize
                    : OverrideFont && Font
                        ? Font.fontSize
                        : ui.defaultFont ? ui.defaultFont.fontSize : 16;

                if (Format.TextColor.HasValue)
                {
                    UpdateColorized(handle.GetState());
                }
                else
                {
                    Update(handle.GetState());
                }
            }

            private void Update(string text)
            {
                _stringBuilder.Clear();
                _stringBuilder.Append("<size=");
                _stringBuilder.Append(_size);
                _stringBuilder.Append('>');
                _stringBuilder.Append(text);
                _stringBuilder.Append("</size>");
                Content = _stringBuilder.ToString();
            }

            private void UpdateColorized(string text)
            {
                _stringBuilder.Clear();
                _stringBuilder.Append("<size=");
                _stringBuilder.Append(_size);
                _stringBuilder.Append('>');
                _stringBuilder.Append("<color=#");
                _stringBuilder.Append(_textColor);
                _stringBuilder.Append('>');
                _stringBuilder.Append(text);
                _stringBuilder.Append("</color>");
                _stringBuilder.Append("</size>");
                Content = _stringBuilder.ToString();
            }

            private void SetActive(bool activeState)
            {
                Enabled = activeState;
            }
        }

        private readonly ref struct ScreenData
        {
            public readonly float Width;
            public readonly float Height;
            public readonly float HalfHeight;

            public ScreenData(float width, float height)
            {
                Width = width;
                Height = height;
                HalfHeight = height * .5f;
            }
        }

        private readonly ref struct Context
        {
            public readonly GUIStyle Style;

            public Context(GUIStyle style)
            {
                Style = style;
            }
        }

        #endregion

        #region Data

        private Font defaultFont;

        private readonly List<GUIElement> _unitsUpperLeft = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsUpperRight = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsLowerLeft = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsLowerRight = new List<GUIElement>(100);

        private readonly GUIContent _content = new GUIContent();

        private static float lastLowerLeftHeight;
        private static float lastLowerRightHeight;
        private static int topLeftRows;
        private static int topRightRows;

        private readonly Dictionary<int, Font> _loadedFonts = new Dictionary<int, Font>();

        private const float ELEMENT_SPACING = 1f;
        private readonly MarginOrPadding windowMargin = new MarginOrPadding(2, 2, 2, 2);
        private readonly MarginOrPadding elementPadding = new MarginOrPadding(0, 0, 5, 5);

        private readonly Color backgroundColor = Color.black;

        private Font GetFont(int fontHash)
        {
            return _loadedFonts.TryGetValue(fontHash, out var fontAsset) ? fontAsset : defaultFont;
        }

        #endregion

        #region Setup

        protected override void Awake()
        {
            base.Awake();

            var availableFonts = Resources.LoadAll<Font>("Monitoring");

            for (var i = 0; i < availableFonts.Length; i++)
            {
                var fontAsset = availableFonts[i];
                var hash = fontAsset.name.GetHashCode();
                if (Monitor.Registry.UsedFonts.Contains(name))
                {
                    _loadedFonts.Add(hash, fontAsset);
                }
            }

            defaultFont = GetFont("JetBrainsMono-Regular".GetHashCode());
        }

        #endregion

        #region Overrides

        /// <summary>
        /// The visible state of the UI.
        /// </summary>
        public override bool Visible { get; set; } = true;

        /// <summary>
        /// Use to add UI elements for the passed unit.
        /// </summary>
        protected override void OnMonitorHandleCreated(IMonitorHandle handle)
        {
            OnUnitCreatedInternal(handle);
        }

        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        protected override void OnMonitorHandleDisposed(IMonitorHandle handle)
        {
            OnUnitDisposedInternal(handle);
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (Visible)
            {
                GUI.skin.font = defaultFont;
                var ctx = new Context(GUI.skin.label);
                var screenData = new ScreenData(Screen.width, Screen.height);
                DrawUpperLeft(ctx, screenData);
                DrawUpperRight(ctx, screenData);
                DrawLowerLeft(ctx, screenData);
                DrawLowerRight(ctx, screenData);
            }
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
                if (!guiElement.Enabled)
                {
                    continue;
                }

                GUI.skin.font = guiElement.OverrideFont ? guiElement.Font : defaultFont;

                var displayString = guiElement.Content;
                _content.text = displayString;

                var textRect = new Rect();
                var textDimensions = ctx.Style.CalcSize(_content);

                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (elementRect.y + elementRect.height + lastLowerLeftHeight > screenData.Height && elementRect.height < screenData.Height)
                {
                    topLeftRows++;
                    yPos = windowMargin.top;
                    xPos += maxWidth + ELEMENT_SPACING;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                yPos += elementRect.height + ELEMENT_SPACING;
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

        private void DrawLowerLeft(Context ctx, ScreenData screenData)
        {
            var xPos = windowMargin.left;
            var yPos = screenData.Height - windowMargin.bot;
            var maxWidth = 0f;
            var maxHeight = 0f;

            for (var i = _unitsLowerLeft.Count - 1; i >= 0; i--)
            {
                var guiElement = _unitsLowerLeft[i];
                if (!guiElement.Enabled)
                {
                    continue;
                }

                GUI.skin.font = guiElement.OverrideFont ? guiElement.Font : defaultFont;

                var displayString = guiElement.Content;
                _content.text = displayString;

                var textRect = new Rect();
                var textDimensions = ctx.Style.CalcSize(_content);

                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (topLeftRows > 1 && screenData.Height - yPos > screenData.HalfHeight)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos += maxWidth + ELEMENT_SPACING;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                else if (topLeftRows == 1 && screenData.Height - yPos > screenData.Height)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos += maxWidth + ELEMENT_SPACING;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }

                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);

                yPos -= elementRect.height + ELEMENT_SPACING;
                maxHeight = Mathf.Max(screenData.Height - yPos, maxHeight);
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

        private void DrawUpperRight(Context ctx, ScreenData screenData)
        {
            var xPos = screenData.Width;
            var yPos = windowMargin.top;
            var maxWidth = 0f;
            topRightRows = 1;

            for (var i = 0; i < _unitsUpperRight.Count; i++)
            {
                var guiElement = _unitsUpperRight[i];
                if (!guiElement.Enabled)
                {
                    continue;
                }

                GUI.skin.font = guiElement.OverrideFont ? guiElement.Font : defaultFont;

                var displayString = guiElement.Content;
                _content.text = displayString;

                var textRect = new Rect();
                var textDimensions = ctx.Style.CalcSize(_content);

                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (elementRect.y + elementRect.height + lastLowerRightHeight > screenData.Height && elementRect.height < screenData.Height)
                {
                    topRightRows++;
                    yPos = windowMargin.top;
                    xPos -= (maxWidth + ELEMENT_SPACING);
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }

                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                yPos += elementRect.height + ELEMENT_SPACING;
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

        private void DrawLowerRight(Context ctx, ScreenData screenData)
        {
            var xPos = screenData.Width;
            var yPos = screenData.Height - windowMargin.bot;
            var maxWidth = 0f;
            var maxHeight = 0f;

            for (var i = _unitsLowerRight.Count - 1; i >= 0; i--)
            {
                var guiElement = _unitsLowerRight[i];
                if (!guiElement.Enabled)
                {
                    continue;
                }

                GUI.skin.font = guiElement.OverrideFont ? guiElement.Font : defaultFont;

                var displayString = guiElement.Content;
                _content.text = displayString;

                var textRect = new Rect();
                var textDimensions = ctx.Style.CalcSize(_content);

                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (topRightRows > 1 && screenData.Height - yPos > screenData.HalfHeight)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos -= maxWidth + ELEMENT_SPACING;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                else if (topRightRows == 1 && screenData.Height - yPos > screenData.Height)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos -= maxWidth + ELEMENT_SPACING;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }

                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);

                yPos -= elementRect.height + ELEMENT_SPACING;
                maxHeight = Mathf.Max(screenData.Height - yPos, maxHeight);
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

        #region Unit Creating / Disposal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUnitCreatedInternal(IMonitorHandle handle)
        {
            switch (handle.Profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                    _unitsUpperLeft.Add(new GUIElement(handle, this));
                    _unitsUpperLeft.Sort(GUIElement.Comparison);
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Add(new GUIElement(handle, this));
                    _unitsUpperRight.Sort(GUIElement.Comparison);
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Add(new GUIElement(handle, this));
                    _unitsLowerLeft.Sort(GUIElement.Comparison);
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Add(new GUIElement(handle, this));
                    _unitsLowerRight.Sort(GUIElement.Comparison);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUnitDisposedInternal(IMonitorHandle handle)
        {
            switch (handle.Profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                    _unitsUpperLeft.Remove(_unitsUpperLeft.First(element => element.ID == handle.UniqueID));
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Remove(_unitsUpperRight.First(element => element.ID == handle.UniqueID));
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Remove(_unitsLowerLeft.First(element => element.ID == handle.UniqueID));
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Remove(_unitsLowerRight.First(element => element.ID == handle.UniqueID));
                    break;
            }
        }

        #endregion
    }
}
