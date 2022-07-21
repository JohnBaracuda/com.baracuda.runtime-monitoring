// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Interface;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.UI.IMGUI
{
    /// <summary>
    /// Disclaimer!!!
    /// This class is showing the base for a GUI based monitoring UI Controller.
    /// I recommend using either the TextMesh Pro based uGUI solution or the UIToolkit solution instead.
    /// </summary>
    public class MonitoringGUIDrawer : MonitoringUIController
    {
        #region --- Inspector ---
        
        [Header("Element & Window Spacing")]
        [SerializeField] private float elementSpacing = 2f;
        [SerializeField] private MarginOrPadding windowMargin;
        [SerializeField] private MarginOrPadding elementPadding;
        
        [Header("Scaling")]
        [SerializeField] [Range(0,10)] private float scale = 1;

        [Header("Coloring")]
        [SerializeField] private Color backgroundColor = Color.black;

        [Header("Font")]
        [SerializeField] private Font defaultFont;
        [SerializeField] private Font[] availableFonts;
        
        #endregion

        #region --- Fields, Properties & Nested Types ---
        
        private readonly List<GUIElement> _unitsUpperLeft = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsUpperRight = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsLowerLeft = new List<GUIElement>(100);
        private readonly List<GUIElement> _unitsLowerRight = new List<GUIElement>(100);

        private readonly GUIContent _content = new GUIContent();
        private float _calculatedScale;

        private static float lastLowerLeftHeight;
        private static float lastLowerRightHeight;
        private static int topLeftRows;
        private static int topRightRows;
        
        private readonly Dictionary<int, Font> _loadedFonts = new Dictionary<int, Font>();
        
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
            public bool Enabled { get; private set; }
            public bool OverrideFont { get; } 
            public Font Font { get; }
            public int ID { get; }
            public string Content { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
            private IFormatData FormatData { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

            public readonly Texture2D BackgroundTexture;
            
            private readonly int _size;

            private static readonly Dictionary<Color, Texture2D> backgroundTexturePool =
                new Dictionary<Color, Texture2D>();

            public GUIElement(IMonitorUnit unit, MonitoringGUIDrawer ctx)
            {
                unit.ValueUpdated += Update;
                unit.ActiveStateChanged += SetActive;
                Enabled = unit.Enabled;
                FormatData = unit.Profile.FormatData;
                ID = unit.UniqueID;

                var backgroundColor = unit.Profile.TryGetMetaAttribute<MColorAttribute>(out var colorAttribute)
                    ? colorAttribute.ColorValue
                    : ctx.backgroundColor;
                
                if (backgroundTexturePool.TryGetValue(backgroundColor, out var texture))
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

                if (unit.Profile.TryGetMetaAttribute<MFontAttribute>(out var fontAttribute))
                {
                    var fontAsset = ctx.GetFont(fontAttribute.FontHash);
                    if (fontAsset != null)
                    {
                        OverrideFont = true;
                        Font = fontAsset;
                    }
                }
                
                _size = FormatData.FontSize > 0 
                    ? FormatData.FontSize 
                    : OverrideFont 
                        // ReSharper disable once PossibleNullReferenceException
                        ? Font.fontSize 
                        : ctx.defaultFont.fontSize;
                
                Update(unit.GetState());
            }

            private void Update(string text)
            {
                var sb = StringBuilderPool.Get();
                sb.Append("<size=");
                sb.Append(_size);
                sb.Append('>');
                sb.Append(text);
                sb.Append("</size>");
                Content = StringBuilderPool.Release(sb);
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
            public readonly GUIStyle style;

            public Context(GUIStyle style)
            {
                this.style = style;
            }
        }
        
        private Font GetFont(int fontHash)
        {
            return _loadedFonts.TryGetValue(fontHash, out var fontAsset) ? fontAsset : defaultFont;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        protected override void Awake()
        {
            base.Awake();
            
            for (var i = 0; i < availableFonts.Length; i++)
            {
                var fontAsset = availableFonts[i];
                var hash = fontAsset.name.GetHashCode();
                if (MonitoringManager.IsFontHasUsed(hash))
                {
                    _loadedFonts.Add(hash, fontAsset);
                }
            }
            availableFonts = null;
            
            UpdateScale();
        }

        private void OnValidate()
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
#if UNITY_EDITOR
            _calculatedScale = Mathf.Max(scale * UnityEditor.EditorGUIUtility.pixelsPerPoint, .001f);
#else
            _calculatedScale = Mathf.Max(scale, .001f);
#endif
        }

        #endregion

        #region --- GUI ---
        
        private void OnGUI()
        {
            GUI.skin.font = defaultFont;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_calculatedScale, _calculatedScale, 1));
            var ctx = new Context(GUI.skin.label);
            var screenData = new ScreenData(Screen.width / _calculatedScale, Screen.height / _calculatedScale);
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
                if (!guiElement.Enabled)
                {
                    continue;
                }

                GUI.skin.font = guiElement.OverrideFont ? guiElement.Font : defaultFont;
                
                var displayString = guiElement.Content;
                _content.text = displayString;
                
                var textRect = new Rect();
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (elementRect.y + elementRect.height + lastLowerLeftHeight > screenData.Height && elementRect.height < screenData.Height)
                {
                    topLeftRows++;
                    yPos = windowMargin.top;
                    xPos += maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
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
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (topLeftRows > 1 && screenData.Height - yPos > screenData.HalfHeight)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos += maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                else if (topLeftRows == 1 && screenData.Height - yPos > screenData.Height)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos += maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                
                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                
                yPos -= elementRect.height + elementSpacing;
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
        
        private void DrawUpperRight(Context ctx,  ScreenData screenData)
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
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (elementRect.y + elementRect.height + lastLowerRightHeight > screenData.Height && elementRect.height < screenData.Height)
                {
                    topRightRows++;
                    yPos = windowMargin.top;
                    xPos -= (maxWidth + elementSpacing);
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                
                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
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
                var textDimensions = ctx.style.CalcSize(_content);
                
                var elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);

                maxWidth = Mathf.Max(maxWidth, elementRect.width);
                if (topRightRows > 1 && screenData.Height - yPos > screenData.HalfHeight)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos -= maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                else if (topRightRows == 1 && screenData.Height - yPos > screenData.Height)
                {
                    yPos = screenData.Height - windowMargin.bot;
                    xPos -= maxWidth + elementSpacing;
                    maxWidth = 0f;
                    elementRect = ElementRect(ref textRect, textDimensions, xPos, yPos);
                }
                
                GUI.DrawTexture(elementRect, guiElement.BackgroundTexture, ScaleMode.StretchToFill);
                GUI.Label(textRect, displayString);
                
                yPos -= elementRect.height + elementSpacing;
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
                    _unitsUpperLeft.Add(new GUIElement(unit, this));
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Add(new GUIElement(unit, this));
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Add(new GUIElement(unit, this));
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Add(new GUIElement(unit, this));
                    break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUnitDisposedInternal(IMonitorUnit unit)
        {
            switch (unit.Profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                    _unitsUpperLeft.Remove(_unitsUpperLeft.First(element => element.ID == unit.UniqueID));
                    break;
                case UIPosition.UpperRight:
                    _unitsUpperRight.Remove(_unitsUpperRight.First(element => element.ID == unit.UniqueID));
                    break;
                case UIPosition.LowerLeft:
                    _unitsLowerLeft.Remove(_unitsLowerLeft.First(element => element.ID == unit.UniqueID));
                    break;
                case UIPosition.LowerRight:
                    _unitsLowerRight.Remove(_unitsLowerRight.First(element => element.ID == unit.UniqueID));
                    break;
            }
        }
        
        #endregion
    }
}
