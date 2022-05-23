// Copyright (c) 2022 Jonathan Lang
using UnityEngine;

namespace Baracuda.Threading.Internal
{
    [RequireComponent(typeof(Dispatcher))]
    public class DispatcherPostUpdate : MonoBehaviour
    {
#if !DISPATCHER_DISABLE_POSTUPDATE
        private void LateUpdate()
        {
            Dispatcher.PostUpdate();
        }
#endif
    }
}
