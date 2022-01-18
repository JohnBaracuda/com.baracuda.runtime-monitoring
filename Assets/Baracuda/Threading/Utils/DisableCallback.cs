using System;
using UnityEngine;

namespace Baracuda.Threading.Internal
{
    [DisallowMultipleComponent]
    public class DisableCallback : MonoBehaviour, IDisableCallback
    {
        public event Action onDisable;
        private void OnDisable()
        {
            onDisable?.Invoke();
            onDisable = null;
        }
    }
}