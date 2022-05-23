// Copyright (c) 2022 Jonathan Lang
using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global

namespace Baracuda.Threading.Tasks
{
    public static class TaskExtensions
    {
        
        #region --- Timeout ---

        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async void Timeout(this Task mightTimeout, int timeoutMillisecond, CancellationToken ct = default)
        {
            await Task.WhenAny(mightTimeout, TimeoutInternalTaskAsync(timeoutMillisecond, ct));
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async void Timeout(this Task mightTimeout, TimeSpan timeoutTimeSpan, CancellationToken ct = default)
        {
            await TimeoutAsync(mightTimeout, timeoutTimeSpan.Milliseconds, ct);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        private static async Task TimeoutInternalTaskAsync(int millisecondsTimeout, CancellationToken ct = default)
        {
            await Task.Delay(millisecondsTimeout, ct);
            throw new TimeoutException($"Timeout after {millisecondsTimeout}ms while awaiting a task!");
        }
        
        #endregion
        
        #region --- Timeout: Async ---
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static Task TimeoutAsync(this Task mightTimeout, int timeoutMillisecond, CancellationToken ct = default)
        {
            return Task.WhenAny(mightTimeout, TimeoutInternalAsync(timeoutMillisecond, ct));
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static Task TimeoutAsync(this Task mightTimeout, TimeSpan timeoutTimeSpan, CancellationToken  ct = default)
        {
            return TimeoutAsync(mightTimeout, timeoutTimeSpan.Milliseconds, ct);
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async Task<TResult> TimeoutAsync<TResult>(this Task<TResult> mightTimeout, int timeoutMillisecond, CancellationToken ct = default)
        {
            return await await Task.WhenAny(mightTimeout, TimeoutInternalAsync<TResult>(timeoutMillisecond, ct));
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async Task<TResult> TimeoutAsync<TResult>(this Task<TResult> mightTimeout, TimeSpan timeoutTimeSpan, CancellationToken ct = default)
        {
            return await TimeoutAsync(mightTimeout, timeoutTimeSpan.Milliseconds, ct);
        }

        //--------------------------------------------------------------------------------------------------------------
        
        private static async Task<TResult> TimeoutInternalAsync<TResult>(int millisecondsTimeout, CancellationToken ct = default)
        {
            await Task.Delay(millisecondsTimeout, ct);
            throw new TimeoutException($"Timeout after {millisecondsTimeout}ms while awaiting a task!");
        }
        
        private static async Task TimeoutInternalAsync(int millisecondsTimeout, CancellationToken ct = default)
        {
            await Task.Delay(millisecondsTimeout, ct);
            throw new TimeoutException($"Timeout after {millisecondsTimeout}ms while awaiting a task!");
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Generic Exception Handling ---

        /// <summary>
        /// Ignore any <see cref="Exception"/> of type <see cref="TException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async Task IgnoreExceptionAsync<TException>(this Task task) where TException : Exception
        {
            try
            {
                await task;
            }
            catch (TException)
            {
            }
        }
        
        /// <summary>
        /// Ignore every <see cref="Exception"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async Task IgnoreExceptionsAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        /// <summary>
        /// Ignore any <see cref="Exception"/> of type <see cref="TException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async Task<TResult> IgnoreExceptionAsync<TException, TResult>(this Task<TResult> task, TResult defaultValue = default) where TException : Exception
        {
            try
            {
                return await task;
            }
            catch (TException)
            {
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Ignore every <see cref="Exception"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async Task<TResult> IgnoreExceptionsAsync<TResult>(this Task<TResult> task, TResult defaultValue = default)
        {
            try
            {
                return await task;
            }
            catch (Exception)
            {
                // ignored
            }

            return defaultValue;
        }
        
        #endregion
        
        #region --- Concrete Exception Handling ---

        
        /// <summary>
        /// Ignore any <see cref="TimeoutException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async Task IgnoreTimeoutException(this Task task)
            => await IgnoreExceptionAsync<TimeoutException>(task);

        
        /// <summary>
        /// Ignore any <see cref="OperationCanceledException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async Task IgnoreOperationCanceledException(this Task task)
            => await IgnoreExceptionAsync<OperationCanceledException>(task);
        
        
        /// <summary>
        /// Ignore any <see cref="ThreadAbortException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async Task IgnoreThreadAbortException(this Task task)
            => await IgnoreExceptionAsync<ThreadAbortException>(task);
        
        
        #endregion

        #region --- Concrete Exception Handling: Async ---

        
        /// <summary>
        /// Ignore any <see cref="TimeoutException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async Task<TResult> IgnoreTimeoutExceptionAsync<TResult>(this Task<TResult> task, TResult defaultValue = default)
            => await IgnoreExceptionAsync<TimeoutException, TResult>(task, defaultValue);

        
        /// <summary>
        /// Ignore any <see cref="OperationCanceledException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async Task<TResult> IgnoreOperationCanceledExceptionAsync<TResult>(this Task<TResult> task, TResult defaultValue = default)
            => await IgnoreExceptionAsync<OperationCanceledException, TResult>(task, defaultValue);
        
        
        /// <summary>
        /// Ignore any <see cref="ThreadAbortException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async Task<TResult> IgnoreThreadAbortExceptionAsync<TResult>(this Task<TResult> task, TResult defaultValue = default)
            => await IgnoreExceptionAsync<ThreadAbortException, TResult>(task, defaultValue);
        
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Valuetask Timeout ---
#if ENABLE_VALUETASK
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async void Timeout(this ValueTask mightTimeout, int timeoutMillisecond, CancellationToken ct = default)
        {
            await Task.WhenAny(mightTimeout.AsTask(), TimeoutInternalTaskAsync(timeoutMillisecond, ct));
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async void Timeout(this ValueTask mightTimeout, TimeSpan timeoutTimeSpan, CancellationToken ct = default)
        {
            await TimeoutAsync(mightTimeout, timeoutTimeSpan.Milliseconds, ct);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        private static async ValueTask TimeoutInternalValueTaskAsync(int millisecondsTimeout, CancellationToken ct = default)
        {
            await Task.Delay(millisecondsTimeout, ct);
            throw new TimeoutException($"Timeout after {millisecondsTimeout}ms while awaiting a task!");
        }
        
#endif
        #endregion
        
        #region --- Valuetask Timeout: Async ---
#if ENABLE_VALUETASK
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static ValueTask TimeoutAsync(this ValueTask mightTimeout, int timeoutMillisecond, CancellationToken ct = default)
        {
            return new ValueTask(Task.WhenAny(mightTimeout.AsTask(), TimeoutInternalAsync(timeoutMillisecond, ct)));
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static ValueTask TimeoutAsync(this ValueTask mightTimeout, TimeSpan timeoutTimeSpan, CancellationToken  ct = default)
        {
            return TimeoutAsync(mightTimeout, timeoutTimeSpan.Milliseconds, ct);
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async ValueTask<TResult> TimeoutAsync<TResult>(this ValueTask<TResult> mightTimeout, int timeoutMillisecond, CancellationToken ct = default)
        {
            return await await Task.WhenAny(mightTimeout.AsTask(), TimeoutInternalAsync<TResult>(timeoutMillisecond, ct));
        }
        
        /// <summary>
        /// Set a timout for the completion of the target <see cref="Task"/>. A <exception cref="TimeoutException"></exception>
        /// is thrown if the target task does not complete within the set timeframe.
        /// </summary>
        public static async ValueTask<TResult> TimeoutAsync<TResult>(this ValueTask<TResult> mightTimeout, TimeSpan timeoutTimeSpan, CancellationToken ct = default)
        {
            return await TimeoutAsync(mightTimeout, timeoutTimeSpan.Milliseconds, ct);
        }
#endif
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Valuetask Generic Exception Handling ---
#if ENABLE_VALUETASK
        
        /// <summary>
        /// Ignore any <see cref="Exception"/> of type <see cref="TException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async ValueTask IgnoreExceptionAsync<TException>(this ValueTask task) where TException : Exception
        {
            try
            {
                await task;
            }
            catch (TException)
            {
            }
        }
        
        /// <summary>
        /// Ignore every <see cref="Exception"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async ValueTask IgnoreExceptionsAsync(this ValueTask task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        /// <summary>
        /// Ignore any <see cref="Exception"/> of type <see cref="TException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async ValueTask<TResult> IgnoreExceptionAsync<TException, TResult>(this ValueTask<TResult> task, TResult defaultValue = default) where TException : Exception
        {
            try
            {
                return await task;
            }
            catch (TException)
            {
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Ignore every <see cref="Exception"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async ValueTask<TResult> IgnoreExceptionsAsync<TResult>(this ValueTask<TResult> task, TResult defaultValue = default)
        {
            try
            {
                return await task;
            }
            catch (Exception)
            {
                // ignored
            }

            return defaultValue;
        }
#endif
        #endregion
        
        #region --- Valuetask Concrete Exception Handling ---
#if ENABLE_VALUETASK
        
        /// <summary>
        /// Ignore any <see cref="TimeoutException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async ValueTask IgnoreTimeoutException(this ValueTask task)
            => await IgnoreExceptionAsync<TimeoutException>(task);

        
        /// <summary>
        /// Ignore any <see cref="OperationCanceledException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async ValueTask IgnoreOperationCanceledException(this ValueTask task)
            => await IgnoreExceptionAsync<OperationCanceledException>(task);
        
        
        /// <summary>
        /// Ignore any <see cref="ThreadAbortException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>
        /// </summary>
        public static async ValueTask IgnoreThreadAbortException(this ValueTask task)
            => await IgnoreExceptionAsync<ThreadAbortException>(task);
        
#endif
        #endregion

        #region --- Valuetask Concrete Exception Handling: Async ---
#if ENABLE_VALUETASK
        
        /// <summary>
        /// Ignore any <see cref="TimeoutException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async ValueTask<TResult> IgnoreTimeoutExceptionAsync<TResult>(this ValueTask<TResult> task, TResult defaultValue = default)
            => await IgnoreExceptionAsync<TimeoutException, TResult>(task, defaultValue);

        
        /// <summary>
        /// Ignore any <see cref="OperationCanceledException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async ValueTask<TResult> IgnoreOperationCanceledExceptionAsync<TResult>(this ValueTask<TResult> task, TResult defaultValue = default)
            => await IgnoreExceptionAsync<OperationCanceledException, TResult>(task, defaultValue);
        
        
        /// <summary>
        /// Ignore any <see cref="ThreadAbortException"/> that might occur during the execution of the
        /// target <see cref="Task"/> <paramref name="task"/>. You can set the default value returned by this
        /// <see cref="Task{TResult}"/> if an exception occurs.
        /// </summary>
        public static async ValueTask<TResult> IgnoreThreadAbortExceptionAsync<TResult>(this ValueTask<TResult> task, TResult defaultValue = default)
            => await IgnoreExceptionAsync<ThreadAbortException, TResult>(task, defaultValue);
        
#endif
        #endregion
        
    }
}
