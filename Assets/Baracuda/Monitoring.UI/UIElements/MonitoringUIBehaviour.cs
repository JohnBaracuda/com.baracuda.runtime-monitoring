using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIElements
{
    [AddComponentMenu("Monitoring")]
    [RequireComponent(typeof(UIDocument))]

    public class MonitoringUIBehaviour : MonitoredSingleton<MonitoringUIBehaviour>, IMonitoringUI
    {
        #region --- Inspector ---
        
        [SerializeField] private bool activateOnLoad = true;
        [SerializeField] private bool instantiateAsync = false;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields ---

        private readonly Dictionary<IMonitorUnit, IMonitoringUIElement> _monitorUnitDisplays =
            new Dictionary<IMonitorUnit, IMonitoringUIElement>();
        
        
        private UIDocument _uiDocument;
        private VisualElement _frame;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Init ---

        protected override void Awake()
        {
            base.Awake();

            _uiDocument = GetComponent<UIDocument>();
            _frame = _uiDocument.rootVisualElement.Q<VisualElement>("frame");
            
            // Add custom styleSheets.
            foreach (var optionalStyleSheet in MonitoringSettings.Instance().OptionalStyleSheets)
            {
                _uiDocument.rootVisualElement.styleSheets.Add(optionalStyleSheet);
            }
            
            // if profiling has already been completed at this point the listener will be invoked the moment it is subscribed.
            MonitoringEvents.ProfilingCompleted += OnProfilingCompleted;


            if (activateOnLoad)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }


        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Open Close ---

        public bool IsActive { get; private set; } = true;

        public void Show()
        {
            IsActive = true;
            _uiDocument.rootVisualElement.SetEnabled(true);
            _uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        public void Hide()
        {
            IsActive = false;
            _uiDocument.rootVisualElement.SetEnabled(false);
            _uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        #endregion

        #region --- Filter ---
        
        public void ResetFilter()
        {
            foreach (var pair in _monitorUnitDisplays)
            {
                pair.Value.SetVisible(true);
            }
        }
        
        public void Filter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                ResetFilter();
                return;
            }
            
            foreach (var pair in _monitorUnitDisplays)
            {
                pair.Value.SetVisible(pair.Value.Tags.Any(unitTag =>
                    unitTag.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0));
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ui Element Instantiation ---

        private void OnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            MonitoringEvents.UnitCreated += CreateUnit;
            MonitoringEvents.UnitDisposed += DisposeUnit;

            if (instantiateAsync)
            {
                StartCoroutine(CreateUnitsAsync(staticUnits, instanceUnits));
            }
            else
            {
                for (var i = 0; i < staticUnits.Count; i++)
                {
                    CreateUnit(staticUnits[i]);
                }
                for (var i = 0; i < instanceUnits.Count; i++)
                {
                    CreateUnit(instanceUnits[i]);
                }
            }
                
            enabled = true;
        }

        private IEnumerator CreateUnitsAsync(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            for (var i = 0; i < staticUnits.Count; i++)
            {
                yield return null;
                CreateUnit(staticUnits[i]);
            }
                
            for (var i = 0; i < instanceUnits.Count; i++)
            {
                yield return null;
                CreateUnit(instanceUnits[i]);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringEvents.ProfilingCompleted -= OnProfilingCompleted;
        }

        private void CreateUnit(IMonitorUnit monitorUnit)
        {
            _monitorUnitDisplays.Add(monitorUnit, new MonitoringUIElement(_frame, monitorUnit));
        }
        
        private void DisposeUnit(IMonitorUnit monitorUnit)
        {
            _monitorUnitDisplays.Remove(monitorUnit);
        }
        
        #endregion
        
    }
}
