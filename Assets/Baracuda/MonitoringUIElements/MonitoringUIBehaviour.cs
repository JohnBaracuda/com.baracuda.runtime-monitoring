using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.MonitoringUXML
{
    [AddComponentMenu("Monitoring")]
    [RequireComponent(typeof(UIDocument))]
    public class MonitoringUIBehaviour : MonitoredSingleton<MonitoringUIBehaviour>
    {
        #region --- [INSPECTOR] ---
        
        [SerializeField] private bool activateOnLoad = true;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FIELDS] ---
        
        private readonly List<MonitoringUIElement> _monitorUnitDisplays = new List<MonitoringUIElement>();
        
        private UIDocument _uiDocument;
        private VisualElement _frame;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INIT] ---

        protected override void Awake()
        {
            base.Awake();

            _uiDocument = GetComponent<UIDocument>();
            _frame = _uiDocument.rootVisualElement.Q<VisualElement>("frame");
            
            // Add custom styleSheets.
            foreach (var optionalStyleSheet in MonitoringSettings.Instance().optionalStyleSheets)
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

        #region --- [OPEN CLOSE] ---

        public bool IsActive { get; private set; }
        
        public void Show()
        {
            _uiDocument.rootVisualElement.SetEnabled(true);
            _uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            IsActive = true;
        }

        public void Hide()
        {
            _uiDocument.rootVisualElement.SetEnabled(false);
            _uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            IsActive = false;
        }

        #endregion

        #region --- [FILTER] ---
        
        public void ResetFilter()
        {
            for (var i = 0; i < _monitorUnitDisplays.Count; i++)
            {
                _monitorUnitDisplays[i].SetVisible(true);
            }
        }
        
        public void Filter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                ResetFilter();
                return;
            }
            for (var i = 0; i < _monitorUnitDisplays.Count; i++)
            {
                _monitorUnitDisplays[i].SetVisible(_monitorUnitDisplays[i].Tags.Any(unitTag =>
                    unitTag.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0));
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UI ELEMENT INSTANTIATION] ---

        private void OnProfilingCompleted(IMonitorUnit[] staticUnits, IMonitorUnit[] instanceUnits)
        {
            MonitoringEvents.UnitCreated += CreateUnit;
                
            for (var i = 0; i < staticUnits.Length; i++)
            {
                CreateUnit(staticUnits[i]);
            }
                
            for (var i = 0; i < instanceUnits.Length; i++)
            {
                CreateUnit(instanceUnits[i]);
            }
                
            enabled = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringEvents.ProfilingCompleted -= OnProfilingCompleted;
        }

        private void CreateUnit(IMonitorUnit unit)
        {
            _monitorUnitDisplays.Add(MonitoringUIElement.CreateInstance(_frame, unit));
        }

        
        #endregion
        
    }
}
