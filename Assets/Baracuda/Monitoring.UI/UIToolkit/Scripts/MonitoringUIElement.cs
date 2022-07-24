// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Pooling.Concretions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    public class MonitoringUIElement : Label, IMonitoringUIElement
    {
        #region --- Fields: Instance & Static ---

        private static readonly Dictionary<object, VisualElement> objectGroups = new Dictionary<object, VisualElement>();
        private static readonly Dictionary<Type, VisualElement> typeGroups = new Dictionary<Type, VisualElement>();

        private static IMonitoringSettings Settings =>
            settings != null ? settings : settings = MonitoringSystems.Resolve<IMonitoringSettings>();

        private static IMonitoringSettings settings;
        private VisualElement _parent;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        public IMonitorUnit Unit { get; }
        public string[] Tags { get; }
        
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
            pickingMode = PickingMode.Ignore;

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
            
            if (profile.TryGetMetaAttribute<MBackgroundColorAttribute>(out var backgroundColorAttribute))
            {
                style.backgroundColor = new StyleColor(backgroundColorAttribute.ColorValue);
            }
            if (profile.TryGetMetaAttribute<MTextColorAttribute>(out var textColorAttribute))
            {
                style.color = new StyleColor(textColorAttribute.ColorValue);
            }

            var font = profile.TryGetMetaAttribute<MFontAttribute>(out var fontAttribute)
                ? provider.GetFont(fontAttribute.FontHash)
                : provider.DefaultFont;

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
                if (!objectGroups.TryGetValue(monitorUnit.Target, out _parent))
                {
                    // Add styles to parent
                    _parent = new VisualElement
                    {
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
                    var label = new Label(
                        $"{profile.FormatData.Group} | {(monitorUnit.Target is UnityEngine.Object obj ? obj.name : monitorUnit.Target.ToString())}");

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
                
                if (profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute))
                {
                    _parent.style.backgroundColor = new StyleColor(groupColorAttribute.ColorValue);
                }
            }
            else
            {
                rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(this);
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
                if (!typeGroups.TryGetValue(profile.UnitTargetType, out _parent))
                {
                    // Add styles to parent
                    _parent = new VisualElement
                    {
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
                    typeGroups.Add(profile.UnitTargetType, _parent);
                }

                _parent ??= rootVisualElement.Q<VisualElement>(Unit.Profile.FormatData.Position.AsString());
                _parent.Add(this);
                
                if (profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute))
                {
                    _parent.style.backgroundColor = new StyleColor(groupColorAttribute.ColorValue);
                }
            }
            else
            {
                rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(this);
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
            
            // Because the unit could have been the only unit in a group we have to check for that case and remove the group if necessary. 
            if (typeGroups.TryGetValue(Unit.Profile.UnitTargetType, out _parent))
            {
                if (_parent.childCount <= 1)
                {
                    _parent.RemoveFromHierarchy();
                    typeGroups.Remove(Unit.Profile.UnitTargetType);
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