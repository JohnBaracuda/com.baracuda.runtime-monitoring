// Copyright (c) 2022 Jonathan Lang
using System;

namespace Baracuda.Threading.Internal
{
    /// <summary>
    /// You can implement this interface in a MonoBehaviour that is passed as a target when dispatching and awaiting a
    /// coroutine. This interface is then used by the dispatcher to receive a callback if the target behaviour was disabled while
    /// the coroutine is still running, so it is important that the onDisable event is invoked if the target behaviour is disabled (OnDisable).
    /// If you are not passing a target MonoBehaviour when dispatching a Coroutine (which I would advise),
    /// the dispatcher itself will act as the target for the coroutine which already implements this interface and should
    /// not be disabled during runtime anyway.
    /// </summary>
    /// <footer><a href="https://johnbaracuda.com/dispatcher.html#IDisableCallback">Documentation</a></footer>
    public interface IDisableCallback
    {
        event Action Disabled;
    }
}