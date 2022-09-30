// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Baracuda.Monitoring.API;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    public class MonitoringUIElement : Label, IMonitoringUIElement, IOrder
    {
        #region --- Fields: Instance & Static ---

        private static readonly Dictionary<object, VisualElement> objectGroups = new Dictionary<object, VisualElement>();
        private static readonly Dictionary<string, VisualElement> namedGroups = new Dictionary<string, VisualElement>();

        private static IMonitoringSettings Settings =>
            settings != null ? settings : settings = MonitoringSystems.Resolve<IMonitoringSettings>();
        
        private static IMonitoringSettings settings;
        private VisualElement _parent;
        private static readonly StringBuilder stringBuilder = new StringBuilder(64);
        
        private readonly Comparison<VisualElement> _comparison = (lhs, rhs) =>
            {
                if (lhs is IOrder mLhs && rhs is IOrder mRhs)
                {
                    return mLhs.Order < mRhs.Order ? 1 : mLhs.Order > mRhs.Order ? -1 : 0;
                }
                return 0;
            };

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        public IMonitorUnit Unit { get; }
        public string[] Tags { get; }
        public int Order { get; }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- UI Element Creation ---

        /// <summary>
        /// Creating a new Monitor Unit UI Element 
        /// </summary>
        internal MonitoringUIElement(VisualElement rootVisualElement, IMonitorUnit monitorUnit, IStyleProvider provider)
        {
            var tags = ListPool<string>.Get();
            tags.Add(monitorUnit.Name);
            tags.AddRange(monitorUnit.Profile.Tags);
            Tags = tags.ToArray();

            ListPool<string>.Release(tags);

            Unit = monitorUnit;
            Unit.ValueUpdated += UpdateGUI;
            Unit.Disposing += OnDisposing;
            Unit.ActiveStateChanged += UpdateActiveState;

            var profile = monitorUnit.Profile;
            var formatData = profile.FormatData;
            pickingMode = PickingMode.Ignore;

            Order = formatData.Order;

            if (Unit.Profile.FormatData.FontSize > 0)
            {
                style.fontSize = Unit.Profile.FormatData.FontSize;
            }

            // Add custom styles set via attribute
            if (profile.TryGetMetaAttribute<StyleAttribute>(out var styles))
            {
                for (var i = 0; i < styles.ClassList.Length; i++)
                {
                    AddToClassList(styles.ClassList[i]);
                }
            }

            if (formatData.BackgroundColor.HasValue)
            {
                style.backgroundColor = new StyleColor(formatData.BackgroundColor.Value);
            }
            if (formatData.TextColor.HasValue)
            {
                style.color = new StyleColor(formatData.TextColor.Value);
            }

            var font = formatData.FontHash != 0 ? provider.GetFont(formatData.FontHash) : provider.DefaultFont;
            
            style.unityFontDefinition = new StyleFontDefinition(font);
            
            if (monitorUnit.Profile.IsStatic)
            {
                SetupStaticUnit(rootVisualElement, profile, provider);
            }
            else
            {
                SetupInstanceUnit(rootVisualElement, monitorUnit, profile, provider);
            }

            UpdateGUI(Unit.GetState());
            UpdateActiveState(Unit.Enabled);
        }

        private void SetupInstanceUnit(VisualElement rootVisualElement, IMonitorUnit monitorUnit, IMonitorProfile profile, IStyleProvider provider)
        {
            for (var i = 0; i < provider.InstanceUnitStyles.Length; i++)
            {
                AddToClassList(provider.InstanceUnitStyles[i]);
            }

            switch (profile.FormatData.TextAlign)
            {
                case HorizontalTextAlign.Left:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                    break;
                case HorizontalTextAlign.Center:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    break;
                case HorizontalTextAlign.Right:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleRight);
                    break;
            }

            if (profile.FormatData.AllowGrouping)
            {
                if (!string.IsNullOrWhiteSpace(profile.FormatData.Group))
                {
                    if (!namedGroups.TryGetValue(profile.FormatData.Group, out _parent))
                    {
                        // Add styles to parent
                        _parent = new OrderedVisualElement
                        {
                            Order = profile.FormatData.GroupOrder,
                            pickingMode = PickingMode.Ignore,
                            style =
                            {
                                unityTextAlign = style.unityTextAlign
                            }
                        };
                        for (var i = 0; i < provider.InstanceGroupStyles.Length; i++)
                        {
                            _parent.AddToClassList(provider.InstanceGroupStyles[i]);
                        }

                        // Add styles to label
                        var label = new Label($"{profile.FormatData.Group}");

                        for (var i = 0; i < provider.InstanceLabelStyles.Length; i++)
                        {
                            label.AddToClassList(provider.InstanceLabelStyles[i]);
                        }

                        _parent.Add(label);
                        rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(_parent);
                        namedGroups.Add(profile.FormatData.Group, _parent);
                    }

                    _parent ??= rootVisualElement.Q<VisualElement>(Unit.Profile.FormatData.Position.AsString());
                    _parent.Add(this);
                    
                    if (_parent is OrderedVisualElement orderParent && profile.FormatData.GroupOrder != 0)
                    {
                        orderParent.Order = profile.FormatData.GroupOrder;
                    }
                    
                    if (profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute))
                    {
                        _parent.style.backgroundColor = new StyleColor(groupColorAttribute.ColorValue);
                    }
                    _parent.Sort(_comparison);
                }
                else
                {
                    if (!objectGroups.TryGetValue(monitorUnit.Target, out _parent))
                    {
                        // Add styles to parent
                        _parent = new OrderedVisualElement
                        {
                            Order = profile.FormatData.GroupOrder,
                            pickingMode = PickingMode.Ignore,
                            style =
                            {
                                unityTextAlign = style.unityTextAlign
                            }
                        };
                        for (var i = 0; i < provider.InstanceGroupStyles.Length; i++)
                        {
                            _parent.AddToClassList(provider.InstanceGroupStyles[i]);
                        }

                        // Add styles to label
                        stringBuilder.Clear();
                        stringBuilder.Append(profile.DeclaringType.Name);
                        if (profile.DeclaringType.Name != monitorUnit.TargetName)
                        {
                            stringBuilder.Append(' ');
                            stringBuilder.Append('|');
                            stringBuilder.Append(' ');
                            stringBuilder.Append(monitorUnit.TargetName);
                        }
                        var label = new Label(stringBuilder.ToString());

                        for (var i = 0; i < provider.InstanceLabelStyles.Length; i++)
                        {
                            label.AddToClassList(provider.InstanceLabelStyles[i]);
                        }

                        _parent.Add(label);
                        rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(_parent);
                        objectGroups.Add(monitorUnit.Target, _parent);
                    }

                    _parent ??= rootVisualElement.Q<VisualElement>(Unit.Profile.FormatData.Position.AsString());
                    _parent.Add(this);
                    
                    if (_parent is OrderedVisualElement orderParent && profile.FormatData.GroupOrder != 0)
                    {
                        orderParent.Order = profile.FormatData.GroupOrder;
                    }
                    
                    if (profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute))
                    {
                        _parent.style.backgroundColor = new StyleColor(groupColorAttribute.ColorValue);
                    }
                    _parent.Sort(_comparison);
                }
            }
            else
            {
                var root = rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString());
                root.Add(this);
                root.Sort(_comparison);
            }
        }
        
        private void SetupStaticUnit(VisualElement rootVisualElement, IMonitorProfile profile, IStyleProvider provider)
        {
            for (var i = 0; i < provider.StaticUnitStyles.Length; i++)
            {
                AddToClassList(provider.StaticUnitStyles[i]);
            }

            switch (profile.FormatData.TextAlign)
            {
                case HorizontalTextAlign.Left:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                    break;
                case HorizontalTextAlign.Center:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    break;
                case HorizontalTextAlign.Right:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleRight);
                    break;
            }
            

            if (profile.FormatData.AllowGrouping)
            {
                if (!namedGroups.TryGetValue(profile.FormatData.Group, out _parent))
                {
                    // Add styles to parent
                    _parent = new OrderedVisualElement
                    {
                        Order = profile.FormatData.GroupOrder,
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            unityTextAlign = style.unityTextAlign
                        }
                    };
                    for (var i = 0; i < provider.StaticGroupStyles.Length; i++)
                    {
                        _parent.AddToClassList(provider.StaticGroupStyles[i]);
                    }

                    // Add styles to label
                    var label = new Label(profile.FormatData.Group);
                    for (var i = 0; i < provider.StaticLabelStyles.Length; i++)
                    {
                        label.AddToClassList(provider.StaticLabelStyles[i]);
                    }

                    _parent.Add(label);
                    rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(_parent);
                    namedGroups.Add(profile.FormatData.Group, _parent);
                }
                
                _parent ??= rootVisualElement.Q<VisualElement>(Unit.Profile.FormatData.Position.AsString());
                _parent.Add(this);

                if (_parent is OrderedVisualElement orderParent && profile.FormatData.GroupOrder != 0)
                {
                    orderParent.Order = profile.FormatData.GroupOrder;
                }
                
                if (profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute))
                {
                    _parent.style.backgroundColor = new StyleColor(groupColorAttribute.ColorValue);
                }
                
                _parent.Sort(_comparison);
            }
            else
            {
                var root = rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString());
                root.Add(this);
                root.Sort(_comparison);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        private void OnDisposing()
        {
            Unit.ValueUpdated -= UpdateGUI;
            Unit.Disposing -= OnDisposing;
            Unit.ActiveStateChanged -= UpdateActiveState;
            _parent = null;

            RemoveFromHierarchy();

            if (Unit.Profile.FormatData.Group != null)
            {
                if (namedGroups.TryGetValue(Unit.Profile.FormatData.Group, out _parent))
                {
                    if (_parent.childCount <= 1)
                    {
                        _parent.RemoveFromHierarchy();
                        namedGroups.Remove(Unit.Profile.FormatData.Group);
                    }
                }
            }
            if  (objectGroups.TryGetValue(Unit.Target, out _parent) && _parent.childCount <= 1)
            {
                _parent.RemoveFromHierarchy();
                objectGroups.Remove(Unit.Target);
            }
        }


        //--------------------------------------------------------------------------------------------------------------

        private void UpdateGUI(string content)
        {
            text = content;
        }

        private void UpdateActiveState(bool activeState)
        {
            this.SetVisible(activeState);
            _parent?.SetVisible(_parent.Children().Count(child => child.style.display.value != DisplayStyle.None) > 1);
        }
    }

    internal static class UIToolkitExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisible(this VisualElement element, bool value)
        {
            element.style.display = new StyleEnum<DisplayStyle>(value ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}