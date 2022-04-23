using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIToolkit
{
    public interface IMonitoringUIElement
    {
        IMonitorUnit Unit { get; }
        string[] Tags { get; }
        void SetVisible(bool value);
    }
    
    public class MonitoringUIElement : Label, IMonitoringUIElement
    {
        #region --- Fields: Instance & Static ---

        private static readonly Dictionary<object, VisualElement> objectGroups = new Dictionary<object, VisualElement>();
        private static readonly Dictionary<Type, VisualElement> typeGroups = new Dictionary<Type, VisualElement>();
        private static MonitoringSettings Settings =>
            settings ? settings : settings = MonitoringSettings.Instance();

        private static MonitoringSettings settings;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        public IMonitorUnit Unit { get; }
        public string[] Tags { get; }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Ui Element Creation ---

        /// <summary>
        /// Creating a new Monitor Unit UI Element 
        /// </summary>
        internal MonitoringUIElement(VisualElement rootVisualElement, IMonitorUnit monitorUnit)
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

            if (Unit.Profile.FontSize > 0)
            {
                style.fontSize = Unit.Profile.FontSize;
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
                SetupStaticUnit(rootVisualElement, profile);
            }
            else
            {
                SetupInstanceUnit(rootVisualElement, monitorUnit, profile);
            }

            UpdateGUI(Unit.GetValueFormatted);
        }

        private void SetupInstanceUnit(VisualElement rootVisualElement, IMonitorUnit monitorUnit, IMonitorProfile profile)
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
                    objectGroups.Add(monitorUnit.Target, parentElement);
                }

                parentElement ??= rootVisualElement.Q<VisualElement>(Unit.Profile.Position.AsString());
                parentElement.Add(this);
            }
            else
            {
                rootVisualElement.Q<VisualElement>(profile.Position.AsString()).Add(this);
            }
        }

        private void SetupStaticUnit(VisualElement rootVisualElement, IMonitorProfile profile)
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
                if (!typeGroups.TryGetValue(profile.UnitDeclaringType, out var parentElement))
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
                    typeGroups.Add(profile.UnitDeclaringType, parentElement);
                }

                parentElement ??= rootVisualElement.Q<VisualElement>(Unit.Profile.Position.AsString());
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
            Unit.ValueUpdated -= UpdateGUI;
            Unit.Disposing -= OnDisposing;

            RemoveFromHierarchy();
            
            // Because the unit could have been the only unit in a group we have to check for that case and remove the group if necessary. 
            if (typeGroups.TryGetValue(Unit.Profile.UnitDeclaringType, out var groupParent))
            {
                if (groupParent.childCount <= 1)
                {
                    groupParent.RemoveFromHierarchy();
                    typeGroups.Remove(Unit.Profile.UnitDeclaringType);
                }
            }
            
            if  (objectGroups.TryGetValue(Unit.Target, out groupParent))
            {
                if (groupParent.childCount <= 1)
                {
                    groupParent.RemoveFromHierarchy();
                    objectGroups.Remove(Unit.Target);
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