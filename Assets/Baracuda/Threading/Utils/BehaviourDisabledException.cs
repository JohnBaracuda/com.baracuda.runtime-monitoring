// Copyright (c) 2022 Jonathan Lang
using System;

namespace Baracuda.Threading.Internal
{
    /// <summary>
    /// Exception is thrown inside a <see cref="UnityEngine.Coroutine"/> that is handled by the
    /// <see cref="ExceptionSensitiveCoroutineHandler"/> class if the coroutines target behaviour is disabled
    /// while it is still running.
    /// </summary>
    public class BehaviourDisabledException : SystemException
    {
        public BehaviourDisabledException(string message) : base(message)
        {
        }
    }
}