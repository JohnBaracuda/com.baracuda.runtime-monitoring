// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringUISystem : IMonitoringUI
    {
        private static MonitoringUISystem current;
        
        private bool _bufferUICreation = false;
        private string _activeFilter;
        private bool _initialized = false;

        // Dependencies
        private readonly IMonitoringSettings _settings;
        private readonly IMonitoringManager _manager;
        private readonly IMonitoringTicker _ticker;

        /*
         * Visibility API   
         */
        
        public void Show()
        {
            if (MonitoringUIController.Current)
            {
                MonitoringUIController.Current.ShowMonitoringUI();
                VisibleStateChanged?.Invoke(true);
            }
        }

        public void Hide()
        {
            if (MonitoringUIController.Current)
            {
                MonitoringUIController.Current.HideMonitoringUI();
                VisibleStateChanged?.Invoke(false);
            }
        }

        public bool ToggleDisplay()
        {
            if (MonitoringUIController.Current == null)
            {
                return false;
            }

            if (MonitoringUIController.Current.IsVisible())
            {
                MonitoringUIController.Current.HideMonitoringUI();
                VisibleStateChanged?.Invoke(false);
            }
            else
            {
                MonitoringUIController.Current.ShowMonitoringUI();
                VisibleStateChanged?.Invoke(true);
            }

            return IsVisible();
        }

        public event Action<bool> VisibleStateChanged;

        public bool IsVisible()
        {
            return MonitoringUIController.Current != null && MonitoringUIController.Current.IsVisible();
        }

        public MonitoringUIController GetActiveUIController()
        {
            return MonitoringUIController.Current;
        }

        public TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController
        {
            return MonitoringUIController.Current as TUIController;
        }

        /*
         * Ctor
         */

        internal MonitoringUISystem(IMonitoringManager manager, IMonitoringSettings settings, IMonitoringTicker ticker)
        {
            _manager = manager;
            _settings = settings;
            _ticker = ticker;
            current = this;
        }

        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StaticInitialize()
        {
            current?.Initialize();
        }
        
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_settings.EnableMonitoring)
            {
                _initialized = true;
                _manager.ProfilingCompleted += (staticUnits, instanceUnits) =>
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        return;
                    }
#endif
                    if (MonitoringUIController.Current)
                    {
                        SetupController(MonitoringUIController.Current, staticUnits, instanceUnits);
                    }
                    else if (_settings.AutoInstantiateUI || _bufferUICreation)
                    {
                        if (TryCreateMonitoringUI(out var controller))
                        {
                            SetupController(controller, staticUnits, instanceUnits);
                        }
                    }
                };
            }
        }
        

        /*
         * Instantiation   
         */

        public void CreateMonitoringUI()
        {
            _bufferUICreation = true;

            if (!_manager.IsInitialized)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            
            // We return if there is an active UIController.
            if (GetActiveUIController())
            {
                Debug.Log("UIController already instantiated!");
                return;
            }

            var instanceUnits = _manager.GetInstanceUnits();
            var staticUnits = _manager.GetStaticUnits();
            
            if (TryCreateMonitoringUI(out var controller))
            {
                SetupController(controller, staticUnits, instanceUnits);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryCreateMonitoringUI(out MonitoringUIController controller)
        {
            if (_settings.UIController == null)
            {
                Debug.LogWarning("UI Controller is null. Please select an active UI Controller!\n" +
                                 "Window: <b>Tools => Runtime Monitoring => Settings => UI Controller => Monitoring UI Controller</b>");
                controller = null;
                return false;
            }

            controller = Object.Instantiate(_settings.UIController);
            return true;
        }

        private void SetupController(MonitoringUIController controller, IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            _manager.UnitCreated += controller.OnUnitCreated;
            _manager.UnitDisposed += controller.OnUnitDisposed;
            
            Application.quitting += () =>
            {
                _manager.UnitCreated -= controller.OnUnitCreated;
                _manager.UnitDisposed -= controller.OnUnitDisposed;
            };

            _manager.UnitCreated += _ =>
            {
                if (_activeFilter != null)
                {
                    ApplyFilter(_activeFilter);
                }
            };
            
            for (var i = 0; i < staticUnits.Count; i++)
            {
                controller.OnUnitCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                controller.OnUnitCreated(instanceUnits[i]);
            }

            if (_settings.OpenDisplayOnLoad)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        /*
         * Filtering   
         */

        private static readonly Regex onlyLetter = new Regex(@"[^a-zA-Z0-9<>_]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void ApplyFilter(string filterString)
        {
            _activeFilter = filterString;
            _ticker.ValidationTickEnabled = false;
            
            var and = _settings.FilterAppendSymbol;
            var not = _settings.FilterNegateSymbol.ToString();
            var absolute = _settings.FilterAbsoluteSymbol.ToString();
            var tag = _settings.FilterTagsSymbol.ToString();

            var list = _manager.GetAllMonitoringUnits();
            var filters = filterString.Split(and);
            
            for (var i = 0; i < list.Count; i++)
            {
                var unit = list[i];
                var unitEnabled = false;
                    
                for (var filterIndex = 0; filterIndex < filters.Length; filterIndex++)
                {
                    var filter =  filters[filterIndex];
                    var filterOnlyLetters = onlyLetter.Replace(filter, string.Empty);
                    var filterNoSpace = filter.Replace(" ", string.Empty);
                    
                    unitEnabled = filterNoSpace.StartsWith(not);
                    
                    if (filterNoSpace.StartsWith(absolute))
                    {
                        var absoluteFilter = filterNoSpace.Substring(1);
                        if (unit.Name.StartsWith(absoluteFilter))
                        {
                            unitEnabled = true;
                        }
                        goto End;
                    }

                    if (filterNoSpace.StartsWith(tag))
                    {
                        var tagFilter = filterNoSpace.Substring(1);
                        var customTags = unit.Profile.CustomTags;
                        if (string.IsNullOrWhiteSpace(tagFilter))
                        {
                            goto End;
                        }
                        for (var tagIndex = 0; tagIndex < customTags.Length; tagIndex++)
                        {
                            var customTag = customTags[tagIndex];
                            if (customTag.IndexOf(filterOnlyLetters, _settings.FilterComparison) < 0)
                            {
                                continue;
                            }

                            unitEnabled = true;
                            goto End;
                        }
                        goto End;
                    }
                        
                    if (unit.Name.IndexOf(filterOnlyLetters, _settings.FilterComparison) >= 0)
                    {
                        unitEnabled = !filterNoSpace.StartsWith(not);
                        goto End;
                    }
                    
                    if (unit.TargetName.IndexOf(filterOnlyLetters, _settings.FilterComparison) >= 0)
                    {
                        unitEnabled = !filterNoSpace.StartsWith(not);
                        goto End;
                    }
                    
                    // Filter with tags.
                    var tags = unit.Profile.Tags;
                    for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
                    {
                        if (tags[tagIndex].Replace(" ", string.Empty).IndexOf(filterOnlyLetters, _settings.FilterComparison) < 0)
                        {
                            continue;
                        }

                        unitEnabled = !filterNoSpace.StartsWith(not);
                        goto End;
                    }
                }

                End:
                unit.Enabled = unitEnabled;
            }
        }


        public void ResetFilter()
        {
            if (string.IsNullOrWhiteSpace(_activeFilter))
            {
                return;
            }
            
            _activeFilter = null;
            _ticker.ValidationTickEnabled = true;
            var units = _manager.GetAllMonitoringUnits();
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                unit.Enabled = unit.Profile.DefaultEnabled;
            }
        }

        public void Filter(string filter)
        {
            ApplyFilter(filter);
        }
    }
}