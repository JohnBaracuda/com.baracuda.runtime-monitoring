// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Threading.Tasks;

namespace Baracuda.Threading.Utils
{
    /// <summary>
    /// Non generic wrapper for a <see cref="TaskCompletionSource{T}"/> that represents a Task that returns no value.
    /// </summary>
    internal class TaskCompletionSource : TaskCompletionSource<Exception>
    {
        public void SetCompleted() => SetResult(null);
        public bool TrySetCompleted() => TrySetResult(null);
    }
}
