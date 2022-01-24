using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using Baracuda.Pooling.Concretions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.Baracuda.Monitoring.UI.UIElements
{
    public class MonitoringUIElement : Label
    {
        #region --- [FIELDS & PROPERTIES] ---

        private static readonly Dictionary<object, VisualElement> _objectGroups = new Dictionary<object, VisualElement>();
        private static readonly Dictionary<Type, VisualElement> _typeGroups = new Dictionary<Type, VisualElement>();

        public string[] Tags { get; }

        private readonly IMonitorUnit _monitorUnit;

        private static MonitoringSettings Settings =>
            _settings ? _settings : _settings = MonitoringSettings.Instance();

        private static MonitoringSettings _settings;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [UI ELEMENT CREATION] ---

        internal static MonitoringUIElement CreateInstance(VisualElement rootVisualElement, IMonitorUnit monitorUnit)
        {
            var instance = new MonitoringUIElement(rootVisualElement, monitorUnit);
            return instance;
        }

        /// <summary>
        /// Creating a new Monitor Unit UI Element 
        /// </summary>
        private MonitoringUIElement(VisualElement rootVisualElement, IMonitorUnit monitorUnit)
        {
            var tags = ListPool<string>.Get();
            tags.Add(monitorUnit.Name);
            tags.AddRange(monitorUnit.Profile.Tags);
            Tags = tags.ToArray();

            ListPool<string>.Release(tags);

            _monitorUnit = monitorUnit;
            _monitorUnit.ValueUpdated += UpdateGUI;
            _monitorUnit.Disposing += OnDisposing;

            var profile = monitorUnit.Profile;
            pickingMode = PickingMode.Ignore;

            if (_monitorUnit.Profile.FontSize > 0)
                style.fontSize = _monitorUnit.Profile.FontSize;

            // Add custom styles set via attribute
            for (var i = 0; i < profile.UssStyles.Length; i++)
            {
                AddToClassList(profile.UssStyles[i]);
            }


            if (monitorUnit.Profile.IsStatic)
            {
                InitStatic(rootVisualElement, profile);
            }
            else
            {
                InitInstance(rootVisualElement, monitorUnit, profile);
            }

            UpdateGUI(_monitorUnit.GetValueFormatted);
        }

        private void InitInstance(VisualElement rootVisualElement, IMonitorUnit monitorUnit, IMonitorProfile profile)
        {
            for (var i = 0; i < Settings.InstanceUnitStyles.Length; i++)
            {
                AddToClassList(Settings.InstanceUnitStyles[i]);
            }

            switch (profile.Position)
            {
                case UIPosition.TopLeft:
                case UIPosition.BottomLeft:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                    break;
                case UIPosition.TopRight:
                case UIPosition.BottomRight:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleRight);
                    break;
            }

            if (profile.AllowGrouping)
            {
                if (!_objectGroups.TryGetValue(monitorUnit.Target, out var parentElement))
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
                    for (var i = 0; i < Settings.InstanceGroupStyles.Length; i++)
                    {
                        parentElement.AddToClassList(Settings.InstanceGroupStyles[i]);
                    }

                    // Add styles to label
                    var label = new Label(
                        $"{profile.GroupName} | {(monitorUnit.Target is UnityEngine.Object obj ? obj.name : monitorUnit.Target.ToString())}");

                    for (var i = 0; i < Settings.InstanceLabelStyles.Length; i++)
                    {
                        label.AddToClassList(Settings.InstanceLabelStyles[i]);
                    }

                    parentElement.Add(label);
                    rootVisualElement.Q<VisualElement>(profile.Position.AsString()).Add(parentElement);
                    _objectGroups.Add(monitorUnit.Target, parentElement);
                }

                parentElement ??= rootVisualElement.Q<VisualElement>(_monitorUnit.Profile.Position.AsString());
                parentElement.Add(this);
            }
            else
            {
                rootVisualElement.Q<VisualElement>(profile.Position.AsString()).Add(this);
            }
        }

        private void InitStatic(VisualElement rootVisualElement, IMonitorProfile profile)
        {
            for (var i = 0; i < Settings.StaticUnitStyles.Length; i++)
            {
                AddToClassList(Settings.StaticUnitStyles[i]);
            }

            switch (profile.Position)
            {
                case UIPosition.TopLeft:
                case UIPosition.BottomLeft:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                    break;
                case UIPosition.TopRight:
                case UIPosition.BottomRight:
                    style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleRight);
                    break;
            }

            if (profile.AllowGrouping)
            {
                if (!_typeGroups.TryGetValue(profile.UnitDeclaringType, out var parentElement))
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
                    for (var i = 0; i < Settings.StaticGroupStyles.Length; i++)
                    {
                        parentElement.AddToClassList(Settings.StaticGroupStyles[i]);
                    }

                    // Add styles to label
                    var label = new Label(profile.GroupName);
                    for (var i = 0; i < Settings.StaticLabelStyles.Length; i++)
                    {
                        label.AddToClassList(Settings.StaticLabelStyles[i]);
                    }

                    parentElement.Add(label);
                    rootVisualElement.Q<VisualElement>(profile.Position.AsString()).Add(parentElement);
                    _typeGroups.Add(profile.UnitDeclaringType, parentElement);
                }

                parentElement ??= rootVisualElement.Q<VisualElement>(_monitorUnit.Profile.Position.AsString());
                parentElement.Add(this);
            }
            else
            {
                rootVisualElement.Q<VisualElement>(profile.Position.AsString()).Add(this);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        private void OnDisposing()
        {
            _monitorUnit.ValueUpdated -= UpdateGUI;
            _monitorUnit.Disposing -= OnDisposing;

            RemoveFromHierarchy();
            
            // Because the unit could have been the only unit in a group we have to check for that case and remove the group if necessary. 
            if (_typeGroups.TryGetValue(_monitorUnit.Profile.UnitDeclaringType, out var groupParent))
            {
                if (groupParent.childCount <= 1)
                {
                    groupParent.RemoveFromHierarchy();
                    _typeGroups.Remove(_monitorUnit.Profile.UnitDeclaringType);
                }
            }
            
            if  (_objectGroups.TryGetValue(_monitorUnit.Target, out groupParent))
            {
                if (groupParent.childCount <= 1)
                {
                    groupParent.RemoveFromHierarchy();
                    _objectGroups.Remove(_monitorUnit.Target);
                }
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