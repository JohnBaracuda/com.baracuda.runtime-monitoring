using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Monitoring.UI.UnityUI
{
    /// <summary>
    /// Disclaimer:
    /// This class is showing the base for a Unity UI based monitoring UI Controller.
    /// It is not complete!
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class UnityMonitoringUIController : MonitoringUIController
    {
        #region --- Inspector ---

        [Min(1)]
        [SerializeField] private int initialPoolSize = 100;
        [SerializeField] private MonitoringUIElement uiElementPrefab;

        [SerializeField] private RectTransform upperLeftTransform;
        [SerializeField] private RectTransform upperRightTransform;
        [SerializeField] private RectTransform lowerLeftTransform;
        [SerializeField] private RectTransform lowerRightTransform;

        [Header("Style")]
        [SerializeField] private float elementSpacing = 0;
        
        
        #endregion

        #region --- Fields ---

        private Stack<MonitoringUIElement> _uiElementPool;
        private Dictionary<IMonitorUnit, MonitoringUIElement> _activeMonitoringUIElement;

        private bool _isDestroyPending = false;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        protected override void Awake()
        {
            _uiElementPool = new Stack<MonitoringUIElement>();
            _activeMonitoringUIElement = new Dictionary<IMonitorUnit, MonitoringUIElement>();
            base.Awake();
            InitializePool();
            ApplyElementSpacing();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _isDestroyPending = true;
            _uiElementPool.Clear();
            _activeMonitoringUIElement.Clear();
        }

        #endregion

        #region --- Pooling ---

        private void InitializePool()
        {
            for (var i = 0; i < initialPoolSize; i++)
            {
                _uiElementPool.Push(CreateElement());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MonitoringUIElement CreateElement()
        {
            var tempMonitoringUIElement = Instantiate(uiElementPrefab, transform);
            tempMonitoringUIElement.Deactivate();
            return tempMonitoringUIElement;
        }

        private MonitoringUIElement GetElementFromPool()
        {
            return _uiElementPool.TryPop(out var element) ? element : CreateElement();
        }

        private void ReleaseElementToPool(MonitoringUIElement element)
        {
            element.Reset();
            element.Deactivate();
            element.SetParent(transform);
            _uiElementPool.Push(element);
        }
        
        #endregion

        #region --- Styling ---

        private void ApplyElementSpacing()
        {
            upperLeftTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
            upperRightTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
            lowerLeftTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
            lowerRightTransform.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = elementSpacing;
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Visibility ---
        
        public override bool IsVisible()
        {
            return gameObject.activeInHierarchy;
        }

        protected override void ShowMonitoringUI()
        {
            gameObject.SetActive(true);
        }

        protected override void HideMonitoringUI()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region --- Unit Creation / Disposing ---
        
        /*
         * Unit creation   
         */

        protected override void OnUnitCreated(IMonitorUnit unit)
        {
            if (_isDestroyPending)
            {
                return;
            }
            var element = GetElementFromPool();
            element.SetParent(GetParentForPosition(unit.Profile.FormatData.Position));
            element.Activate();
            element.Setup(unit);
            _activeMonitoringUIElement.Add(unit, element);
        }

        private Transform GetParentForPosition(UIPosition position)
        {
            return position switch
            {
                UIPosition.TopLeft => upperLeftTransform,
                UIPosition.TopRight => upperRightTransform,
                UIPosition.BottomLeft => lowerLeftTransform,
                UIPosition.BottomRight => lowerRightTransform,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override void OnUnitDisposed(IMonitorUnit unit)
        {
            if (_isDestroyPending)
            {
                return;
            }
            var element = _activeMonitoringUIElement[unit];
            _activeMonitoringUIElement.Remove(unit);
            ReleaseElementToPool(element);
        }
        
        #endregion

        #region --- Filtering ---

        protected override void ResetFilter()
        {
            
        }

        protected override void Filter(string filter)
        {
            
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Helper ---

        #endregion
    }
}
