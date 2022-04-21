using System;
using UnityEngine;

namespace Baracuda.Threading.Internal
{
    [DisallowMultipleComponent]
    public class DisableCallback : MonoBehaviour, IDisableCallback
    {
        public event Action Disabled;
        private void OnDisable()
        {
            Disabled?.Invoke();
            Disabled = null;
        }
    }
}