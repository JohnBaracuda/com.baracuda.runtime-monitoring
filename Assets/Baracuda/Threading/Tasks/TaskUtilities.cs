// Copyright (c) 2022 Jonathan Lang
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Threading.Tasks
{
    public static class TaskUtilities
    {
#if ENABLE_VALUETASK
        /// <summary>
        /// Return a <see cref="ValueTask"/> that will yield until the passed condition evaluates to true.
        /// You can set the interval in which the condition is evaluated.
        /// </summary>
        public static async ValueTask WaitUntilAsync(Func<bool> condition, int interval, CancellationToken ct = default)
        {
            var clampedInterval = Mathf.Clamp(interval, 1, 1000);

            while (!condition())
            {
                await Task.Delay(clampedInterval, ct);
            }
        }
#else
        /// <summary>
        /// Return a <see cref="Task"/> that will yield until the passed condition evaluates to true.
        /// You can set the interval in which the condition is evaluated.
        /// </summary>
        public static async Task WaitUntilAsync(Func<bool> condition, int interval, CancellationToken ct = default)
        {
            var clampedInterval = Mathf.Clamp(interval, 1, 1000);
            
            while (!condition())
            {
                await Task.Delay(clampedInterval, ct);
            }
        }
#endif
    }
}