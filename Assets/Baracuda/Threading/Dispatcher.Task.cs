// Copyright (c) 2022 Jonathan Lang
using System;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Utils;
using UnityEngine;

namespace Baracuda.Threading
{
    public partial class Dispatcher
    {
        #region --- Dispatch: Task ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<Task> function)
        {
            async void Action()
            {
                await function();
            }
            Invoke(Action);
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<Task> function, ExecutionCycle cycle)
        {
            async void Action()
            {
                await function();
            }

            Invoke(Action, cycle);
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<CancellationToken, Task> function, CancellationToken ct, bool throwOnCancellation = true)
        {
            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await function(ct);
                }
                catch (OperationCanceledException oce)
                {
                    if (throwOnCancellation)
                    {
                        Debug.LogException(oce);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Invoke(Action);
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<CancellationToken, Task> function, ExecutionCycle cycle, CancellationToken ct,
            bool throwOnCancellation = true)
        {
            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await function(ct);
                }
                catch (OperationCanceledException oce)
                {
                    if (throwOnCancellation)
                    {
                        Debug.LogException(oce);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Invoke(Action, cycle);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Dispatch: Task Async ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<Task> function)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    await function();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<Task> function, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    await function();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<CancellationToken, Task> function, CancellationToken ct, bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await function(ct);
                    tcs.SetCompleted();
                }
                catch (OperationCanceledException oce)
                {
                    if (throwOnCancellation)
                    {
                        tcs.SetException(oce);
                    }
                    else
                    {
                        tcs.SetCompleted();
                    }
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<CancellationToken, Task> function, ExecutionCycle cycle, CancellationToken ct,
            bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();

                    await function(ct);
                    tcs.SetCompleted();
                }
                catch (OperationCanceledException oce)
                {
                    if (throwOnCancellation)
                    {
                        tcs.SetException(oce);
                    }
                    else
                    {
                        tcs.SetCompleted();
                    }
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }

        #endregion

        #region --- Dispatch: Task<tresult> Async ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    tcs.SetResult(await function());
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task{TResult}"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    tcs.SetResult(await function());
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <param name="ct"></param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<CancellationToken, Task<TResult>> function, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    tcs.SetResult(await function(ct));
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task{TResult}"/> is executed.</param>
        /// <param name="ct"></param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<CancellationToken, Task<TResult>> function, ExecutionCycle cycle,
            CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    tcs.SetResult(await function(ct));
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }

        #endregion
    }
}