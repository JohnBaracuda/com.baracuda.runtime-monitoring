// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.UI.TextMeshPro
{
    [ExecuteAlways]
    internal class UIControllerComponents : MonoBehaviour
    {
        public MonitoringUISection upperLeftSection;
        public MonitoringUISection upperRightSection;
        public MonitoringUISection lowerLeftSection;
        public MonitoringUISection lowerRightSection;
        
        public MonitoringUIElement elementPrefab;
        public MonitoringUIGroup groupPrefab;
    }
}