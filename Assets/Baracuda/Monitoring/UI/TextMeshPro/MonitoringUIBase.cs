// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;

namespace Baracuda.Monitoring.TextMeshPro
{

    [DisableMonitoring]
    [RequireComponent(typeof(RectTransform))]
    internal abstract class MonitoringUIBase : MonoBehaviour
    {
        public static readonly Comparison<MonitoringUIBase> Comparison = (lhs, rhs) => lhs.Order < rhs.Order ? 1 : lhs.Order > rhs.Order ? -1 : 0;

        protected abstract int Order { get; }

        internal void SetSiblingIndex(int index)
        {
            transform.SetSiblingIndex(index);
        }
    }
}