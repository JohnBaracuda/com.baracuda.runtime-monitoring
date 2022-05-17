// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
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
