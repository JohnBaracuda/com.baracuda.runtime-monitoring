// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    internal class UIToolkitMonitoringUIController : MonitoringUI, IStyleProvider
    {
        #region Inspector

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

        #region Properties

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

        #region Fields

        private readonly Dictionary<int, Font> _loadedFonts = new Dictionary<int, Font>();

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<IMonitorHandle, IMonitoringUIElement> _monitorUnitDisplays = new Dictionary<IMonitorHandle, IMonitoringUIElement>();

        private UIDocument _uiDocument;
        private VisualElement _frame;
        private bool _isVisible = true;

        private string[] _instanceUnitStyles = null;
        private string[] _instanceGroupStyles = null;
        private string[] _instanceLabelStyles = null;

        private string[] _staticUnitStyles = null;
        private string[] _staticGroupStyles = null;
        private string[] _staticLabelStyles = null;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Setup

        protected override void Awake()
        {
            base.Awake();

            for (var i = 0; i < availableFonts.Length; i++)
            {
                var fontAsset = availableFonts[i];
                if (Monitor.Registry.UsedFonts.Contains(fontAsset.name))
                {
                    var hash = fontAsset.name.GetHashCode();
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

        #region Visiblility

        /// <summary>
        /// The visible state of the UI.
        /// </summary>
        public override bool Visible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _uiDocument.rootVisualElement.SetEnabled(value);
                _uiDocument.rootVisualElement.style.display = value
                    ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                    : new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        }

        #endregion

        #region Ui Element Instantiation

        /// <summary>
        /// Use to add UI elements for the passed unit.
        /// </summary>
        protected override void OnMonitorHandleCreated(IMonitorHandle handle)
        {
            _monitorUnitDisplays.Add(handle, new MonitoringUIElement(_frame, handle, this));
        }

        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        protected override void OnMonitorHandleDisposed(IMonitorHandle handle)
        {
            _monitorUnitDisplays.Remove(handle);
        }

        #endregion
    }
}
