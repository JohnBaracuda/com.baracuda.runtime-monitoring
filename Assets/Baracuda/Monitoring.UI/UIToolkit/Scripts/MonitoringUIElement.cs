// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Collections.Generic;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Interface;
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
        private static MonitoringSettings Settings =>
            settings ? settings : settings = MonitoringSettings.GetInstance();

        private static MonitoringSettings settings;

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
            
            if (monitorUnit.Profile.IsStatic)
            {
                SetupStaticUnit(rootVisualElement, profile, provider);
            }
            else
            {
                SetupInstanceUnit(rootVisualElement, monitorUnit, profile, provider);
            }

            UpdateGUI(Unit.GetStateFormatted);
        }

        private void SetupInstanceUnit(VisualElement rootVisualElement, IMonitorUnit monitorUnit, IMonitorProfile profile, IStyleProvider provider)
        {
            for (var i = 0; i < provider.InstanceUnitStyles.Length; i++)
            {
                AddToClassList(provider.InstanceUnitStyles[i]);
            }

            switch (profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                case UIPosition.LowerLeft:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                    break;
                case UIPosition.UpperRight:
                case UIPosition.LowerRight:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleRight);
                    break;
            }

            if (profile.FormatData.AllowGrouping)
            {
                if (!objectGroups.TryGetValue(monitorUnit.Target, out var parentElement))
                {
                    // Add styles to parent
                    parentElement = new VisualElement
                    {
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            unityTextAlign = style.unityTextAlign
                        }
                    };
                    for (var i = 0; i < provider.InstanceGroupStyles.Length; i++)
                    {
                        parentElement.AddToClassList(provider.InstanceGroupStyles[i]);
                    }

                    // Add styles to label
                    var label = new Label(
                        $"{profile.FormatData.Group} | {(monitorUnit.Target is UnityEngine.Object obj ? obj.name : monitorUnit.Target.ToString())}");

                    for (var i = 0; i < provider.InstanceLabelStyles.Length; i++)
                    {
                        label.AddToClassList(provider.InstanceLabelStyles[i]);
                    }

                    parentElement.Add(label);
                    rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(parentElement);
                    objectGroups.Add(monitorUnit.Target, parentElement);
                }

                parentElement ??= rootVisualElement.Q<VisualElement>(Unit.Profile.FormatData.Position.AsString());
                parentElement.Add(this);
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

            switch (profile.FormatData.Position)
            {
                case UIPosition.UpperLeft:
                case UIPosition.LowerLeft:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                    break;
                case UIPosition.UpperRight:
                case UIPosition.LowerRight:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleRight);
                    break;
            }

            if (profile.FormatData.AllowGrouping)
            {
                if (!typeGroups.TryGetValue(profile.UnitTargetType, out var parentElement))
                {
                    // Add styles to parent
                    parentElement = new VisualElement
                    {
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            unityTextAlign = style.unityTextAlign
                        }
                    };
                    for (var i = 0; i < provider.StaticGroupStyles.Length; i++)
                    {
                        parentElement.AddToClassList(provider.StaticGroupStyles[i]);
                    }

                    // Add styles to label
                    var label = new Label(profile.FormatData.Group);
                    for (var i = 0; i < provider.StaticLabelStyles.Length; i++)
                    {
                        label.AddToClassList(provider.StaticLabelStyles[i]);
                    }

                    parentElement.Add(label);
                    rootVisualElement.Q<VisualElement>(profile.FormatData.Position.AsString()).Add(parentElement);
                    typeGroups.Add(profile.UnitTargetType, parentElement);
                }

                parentElement ??= rootVisualElement.Q<VisualElement>(Unit.Profile.FormatData.Position.AsString());
                parentElement.Add(this);
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

            RemoveFromHierarchy();
            
            // Because the unit could have been the only unit in a group we have to check for that case and remove the group if necessary. 
            if (typeGroups.TryGetValue(Unit.Profile.UnitTargetType, out var groupParent))
            {
                if (groupParent.childCount <= 1)
                {
                    groupParent.RemoveFromHierarchy();
                    typeGroups.Remove(Unit.Profile.UnitTargetType);
                }
            }
            
            if  (objectGroups.TryGetValue(Unit.Target, out groupParent) && groupParent.childCount <= 1)
            {
                groupParent.RemoveFromHierarchy();
                objectGroups.Remove(Unit.Target);
            }
        }


        //--------------------------------------------------------------------------------------------------------------

        private void UpdateGUI(string content)
        {
            text = content;
        }

        public void SetVisible(bool value)
        {
            style.display = new StyleEnum<DisplayStyle>(value ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}