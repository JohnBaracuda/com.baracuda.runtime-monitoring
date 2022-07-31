// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringUISystem : IMonitoringUI
    {
        private MonitoringUIController _controllerInstance;
        private bool _bufferUICreation = false;
        
        // Dependencies
        private readonly IMonitoringSettings _settings;
        private readonly IMonitoringManager _manager;
        private readonly IMonitoringTicker _ticker;

        /*
         * Visibility API   
         */
        
        public void Show()
        {
            if (_controllerInstance)
            {
                _controllerInstance.ShowMonitoringUI();
                VisibleStateChanged?.Invoke(true);
            }
        }

        public void Hide()
        {
            if (_controllerInstance)
            {
                _controllerInstance.HideMonitoringUI();
                VisibleStateChanged?.Invoke(false);
            }
        }

        public bool ToggleDisplay()
        {
            if (_controllerInstance == null)
            {
                return false;
            }

            if (_controllerInstance.IsVisible())
            {
                _controllerInstance.HideMonitoringUI();
                VisibleStateChanged?.Invoke(false);
            }
            else
            {
                _controllerInstance.ShowMonitoringUI();
                VisibleStateChanged?.Invoke(true);
            }

            return IsVisible();
        }

        public event Action<bool> VisibleStateChanged;

        public bool IsVisible()
        {
            return _controllerInstance != null && _controllerInstance.IsVisible();
        }

        public MonitoringUIController GetActiveUIController()
        {
            return _controllerInstance;
        }

        public TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController
        {
            return _controllerInstance as TUIController;
        }

        /*
         * Ctor
         */

        internal MonitoringUISystem(IMonitoringManager manager, IMonitoringSettings settings, IMonitoringTicker ticker)
        {
            _manager = manager;
            _settings = settings;
            _ticker = ticker;
            
            if (_settings.EnableMonitoring)
            {
                _manager.ProfilingCompleted  += (staticUnits, instanceUnits) =>
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        return;
                    }
#endif

                    if (_settings.AutoInstantiateUI || _bufferUICreation)
                    {
                        InstantiateMonitoringUI(_manager, _settings, staticUnits, instanceUnits);
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
            InstantiateMonitoringUI(_manager, _settings, instanceUnits, staticUnits);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InstantiateMonitoringUI(IMonitoringManager manager, IMonitoringSettings settings, IReadOnlyList<IMonitorUnit> staticUnits,
            IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            if (settings.UIControllerUIController == null)
            {
                return;
            }
            
            _controllerInstance = Object.Instantiate(settings.UIControllerUIController);
            
            Object.DontDestroyOnLoad(_controllerInstance.gameObject);
            _controllerInstance.gameObject.hideFlags = settings.ShowRuntimeUIController ? HideFlags.None : HideFlags.HideInHierarchy;

            manager.UnitCreated += _controllerInstance.OnUnitCreated;
            manager.UnitDisposed += _controllerInstance.OnUnitDisposed;
            
            Application.quitting += () =>
            {
                manager.UnitCreated -= _controllerInstance.OnUnitCreated;
                manager.UnitDisposed -= _controllerInstance.OnUnitDisposed;
            };
            
            for (var i = 0; i < staticUnits.Count; i++)
            {
                _controllerInstance.OnUnitCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                _controllerInstance.OnUnitCreated(instanceUnits[i]);
            }

            if (settings.OpenDisplayOnLoad)
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

        private static readonly Regex onlyLetter = new Regex(@"[^a-zA-Z0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public void ApplyFilter(string filter)
        {
            ApplyFilterInternal(filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyFilterInternal(string filterString)
        {
            _ticker.ValidationTickEnabled = false;

            const char OR = '|';
            const char NOT = '!';
            const char ABSOLUTE = '@';
            var list = _manager.GetAllMonitoringUnits();
            var filters = filterString.Split(OR);
            
            for (var i = 0; i < list.Count; i++)
            {
                var unit = list[i];
                var tags = unit.Profile.Tags;
                var unitEnabled = false;
                    
                for (var filterIndex = 0; filterIndex < filters.Length; filterIndex++)
                {
                    var filter =  filters[filterIndex];
                    var filterOnlyLetters = onlyLetter.Replace(filter, string.Empty);
                    var filterNoSpace = filter.NoSpace();
                    
                    unitEnabled = filterNoSpace.StartsWith(NOT);
                    
                    if (filterNoSpace.StartsWith(ABSOLUTE))
                    {
                        var absoluteFilter = filterNoSpace.Substring(1);
                        if (unit.Name.StartsWith(absoluteFilter))
                        {
                            unitEnabled = true;
                        }
                        goto End;
                    }
                        
                    if (unit.Name.IndexOf(filterOnlyLetters, _settings.FilterComparison) >= 0)
                    {
                        unitEnabled = !filterNoSpace.StartsWith(NOT);
                        goto End;
                    }
                    
                    if (unit.TargetName.IndexOf(filterOnlyLetters, _settings.FilterComparison) >= 0)
                    {
                        unitEnabled = !filterNoSpace.StartsWith(NOT);
                        goto End;
                    }
                    
                    // Filter with tags.
                    for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
                    {
                        if (tags[tagIndex].NoSpace().IndexOf(filterOnlyLetters, _settings.FilterComparison) < 0)
                        {
                            continue;
                        }

                        unitEnabled = !filterNoSpace.StartsWith(NOT);
                        goto End;
                    }
                }

                End:
                unit.Enabled = unitEnabled;
            }
        }
        
        
        public void ResetFilter()
        {
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