// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    [RequireComponent(typeof(UIDocument))]
    internal class UIToolkitMonitoringUIController : MonitoringUIController, IStyleProvider
    {
        #region --- Inspector ---
        
        [Header("Styles")]
        [SerializeField] private StyleSheet[] optionalStyleSheets;
        [Space]
        [SerializeField] private string instanceUnitStyles = "";
        [SerializeField] private string instanceGroupStyles = "";
        [SerializeField] private string instanceLabelStyles = "";
        [Space]
        [SerializeField] private string staticUnitStyles = "";
        [SerializeField] private string staticGroupStyles = "";
        [SerializeField] private string staticLabelStyles = "";
        
        #endregion

        #region --- Properties ---
        public string[] InstanceUnitStyles => _instanceUnitStyles ??= instanceUnitStyles.Split(' ');
        public string[] InstanceGroupStyles => _instanceGroupStyles ??= instanceGroupStyles.Split(' ');
        public string[] InstanceLabelStyles => _instanceLabelStyles ??= instanceLabelStyles.Split(' ');
        public string[] StaticUnitStyles => _staticUnitStyles ??= staticUnitStyles.Split(' ');
        public string[] StaticGroupStyles => _staticGroupStyles ??= staticGroupStyles.Split(' ');
        public string[] StaticLabelStyles => _staticLabelStyles ??= staticLabelStyles.Split(' ');

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields ---

        private readonly Dictionary<IMonitorUnit, IMonitoringUIElement> _monitorUnitDisplays = new Dictionary<IMonitorUnit, IMonitoringUIElement>();
        
        private UIDocument _uiDocument;
        private VisualElement _frame;
        private bool _isVisible;
        
        private string[] _instanceUnitStyles = null;
        private string[] _instanceGroupStyles = null;
        private string[] _instanceLabelStyles = null;

        private string[] _staticUnitStyles = null;
        private string[] _staticGroupStyles = null;
        private string[] _staticLabelStyles = null;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        protected override void Awake()
        {
            base.Awake();
            _monitorUnitDisplays.Clear();

            _uiDocument = GetComponent<UIDocument>();
            _frame = _uiDocument.rootVisualElement.Q<VisualElement>("frame");
            
            foreach (var optionalStyleSheet in optionalStyleSheets)
            {
                _uiDocument.rootVisualElement.styleSheets.Add(optionalStyleSheet);
            }
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Open Close ---

        public override bool IsVisible() => _isVisible;

        protected override void ShowMonitoringUI()
        {
            _isVisible = true;
            _uiDocument.rootVisualElement.SetEnabled(true);
            _uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        protected override void HideMonitoringUI()
        {
            _isVisible = false;
            _uiDocument.rootVisualElement.SetEnabled(false);
            _uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }


        #endregion
        
        #region --- Ui Element Instantiation ---

        protected override void OnUnitCreated(IMonitorUnit monitorUnit)
        {
            _monitorUnitDisplays.Add(monitorUnit, new MonitoringUIElement(_frame, monitorUnit, this));
        }
        
        protected override void OnUnitDisposed(IMonitorUnit monitorUnit)
        {
            _monitorUnitDisplays.Remove(monitorUnit);
        }
        
        #endregion

        #region --- Filter ---
        
        protected override void ResetFilter()
        {
            foreach (var pair in _monitorUnitDisplays)
            {
                pair.Value.SetVisible(true);
            }
        }
        
        protected override void Filter(string filter)
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
    }
}
