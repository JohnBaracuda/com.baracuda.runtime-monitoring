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
        
        #region --- Dispatch: Action ---

        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// Use <see cref="Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions">Documentation</a></footer>
        public static void Invoke(Action action)
        {
            lock (defaultExecutionQueue)
            {
                defaultExecutionQueue.Enqueue(action);
            }

            queuedDefault = true;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions">Documentation</a></footer>
        public static void Invoke(Action action, ExecutionCycle cycle)
        {
            switch (cycle)
            {
#if !DISPATCHER_DISABLE_UPDATE
                case ExecutionCycle.Update:
                    lock (updateExecutionQueue)
                    {
                        updateExecutionQueue.Enqueue(action);
                    }

                    queuedUpdate = true;
                    break;
#endif

#if !DISPATCHER_DISABLE_LATEUPDATE
                case ExecutionCycle.LateUpdate:
                    lock (lateUpdateExecutionQueue)
                    {
                        lateUpdateExecutionQueue.Enqueue(action);
                    }

                    queuedLate = true;
                    break;
#endif

#if !DISPATCHER_DISABLE_FIXEDUPDATE
                case ExecutionCycle.FixedUpdate:
                    lock (fixedUpdateExecutionQueue)
                    {
                        fixedUpdateExecutionQueue.Enqueue(action);
                    }

                    queuedFixed = true;
                    break;
#endif

#if !DISPATCHER_DISABLE_POSTUPDATE
                case ExecutionCycle.PostUpdate:
                    lock (postUpdateExecutionQueue)
                    {
                        postUpdateExecutionQueue.Enqueue(action);
                    }

                    queuedPost = true;
                    break;
#endif

#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
                case ExecutionCycle.TickUpdate:
                    lock (tickExecutionQueue)
                    {
                        tickExecutionQueue.Enqueue(action);
                    }

                    queuedTick = true;
                    break;
#endif

#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
                case ExecutionCycle.EditorUpdate:
                    lock (editorExecutionQueue)
                    {
                        editorExecutionQueue.Enqueue(action);
                    }

                    queuedEditor = true;
                    break;
#endif

                default:
                    lock (defaultExecutionQueue)
                    {
                        defaultExecutionQueue.Enqueue(action);
                    }

                    queuedDefault = true;
                    break;
            }
        }


        

        #endregion

        #region --- Dispatch: Action Async ---

        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async">Documentation</a></footer>
        public static Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    action();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="cycle">The execution cycle during which the <see cref="Action"/> will be invoked.</param>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async">Documentation</a></footer>
        public static Task InvokeAsync(Action action, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    action();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"></param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async-cancel">Documentation</a></footer>
        public static Task InvokeAsync(Action action, CancellationToken ct, bool throwOnCancellation = true)
        {
            if (ct.IsCancellationRequested)
            {
                if (throwOnCancellation)
                {
                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    return Task.CompletedTask;
                }
            }

            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    action();
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
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"> <see cref="Action"/> that will be invoked.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> optional parameter that determines if an <see cref="OperationCanceledException"/>
        /// is thrown if the Task is cancelled prematurely.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async-cancel">Documentation</a></footer>
        public static Task InvokeAsync(Action action, ExecutionCycle cycle, CancellationToken ct,
            bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    
                    action();
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
            }, cycle);
            return tcs.Task;
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Dispatch: Func<tresult> Async ---

        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Func{T}"/> is executed.</param>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-cancel">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Func{T}"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-cancel">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func, ExecutionCycle cycle, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }

        #endregion
        
    }
}