// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using Baracuda.Monitoring.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    [RequireComponent(typeof(UIDocument))]
    internal class UIToolkitMonitoringUIController : MonitoringUIController, IStyleProvider
    {
        #region --- Inspector ---
        
        [Header("Font")]
        [SerializeField] private Font defaultFont;
        [SerializeField] private Font[] availableFonts;
        
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
        public Font DefaultFont => defaultFont;
        public Font GetFont(int fontHash)
        {
            return _loadedFonts.TryGetValue(fontHash, out var fontAsset) ? fontAsset : defaultFont;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields ---
        
        
        private readonly Dictionary<int, Font> _loadedFonts = new Dictionary<int, Font>();

        // ReSharper disable once CollectionNeverQueried.Local
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
            
            var utility = MonitoringSystems.Resolve<IMonitoringUtility>();
            for (var i = 0; i < availableFonts.Length; i++)
            {
                var fontAsset = availableFonts[i];
                var hash = fontAsset.name.GetHashCode();
                if (utility.IsFontHashUsed(hash))
                {
                    _loadedFonts.Add(hash, fontAsset);
                }
            }
            availableFonts = null;
            
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
    }
}
