// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Baracuda.Threading
{
    /// <summary>
    /// Class containing multiple extension methods for dispatching the execution of a<br/>
    /// <see cref="Delegate"/>, <see cref="IEnumerator"/> or <see cref="Task"/>
    /// from a background thread to the main thread.
    /// </summary>
    public static class DispatchExtensions
    {
        
        #region --- Action ---

        
        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or Tick cycle.<br/>
        /// Use <see cref="Dispatcher.Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch(this Action action)
        {
            Dispatcher.Invoke(action);
        }
        
        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or Tick cycle.<br/>
        /// Use <see cref="Dispatcher.Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="arg">first argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T>(this Action<T> action, T arg)
        {
            Dispatcher.Invoke(() => action(arg));
        }
        
        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or Tick cycle.<br/>
        /// Use <see cref="Dispatcher.Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="arg1">first argument</param>
        /// <param name="arg2">second argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T, TS>(this Action<T, TS> action, T arg1, TS arg2)
        {
            Dispatcher.Invoke(() => action(arg1, arg2));
        }
        
        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or Tick cycle.<br/>
        /// Use <see cref="Dispatcher.Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="arg1">first argument</param>
        /// <param name="arg2">second argument</param>
        /// <param name="arg3">third argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T, TS, TU>(this Action<T, TS, TU> action, T arg1, TS arg2, TU arg3)
        {
            Dispatcher.Invoke(() => action(arg1, arg2, arg3));
        }

        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or Tick cycle.<br/>
        /// Use <see cref="Dispatcher.Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="arg1">first argument</param>
        /// <param name="arg2">second argument</param>
        /// <param name="arg3">third argument</param>
        /// <param name="arg4">fourth argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T, TS, TU, TV>(this Action<T, TS, TU, TV> action, T arg1, TS arg2, TU arg3, TV arg4)
        {
            Dispatcher.Invoke(() => action(arg1, arg2, arg3, arg4));
        }
        
        
        
        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch(this Action action, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(action, cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <param name="arg">first argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T>(this Action<T> action, T arg, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(() => action(arg), cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <param name="arg1">first argument</param>
        /// <param name="arg2">second argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T, TS>(this Action<T, TS> action, T arg1, TS arg2, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(() => action(arg1, arg2), cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <param name="arg1">first argument</param>
        /// <param name="arg2">second argument</param>
        /// <param name="arg3">third argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T, TS, TU>(this Action<T, TS, TU> action, T arg1, TS arg2, TU arg3, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(() => action(arg1, arg2, arg3), cycle);
        }

        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <param name="arg1">first argument</param>
        /// <param name="arg2">second argument</param>
        /// <param name="arg3">third argument</param>
        /// <param name="arg4">fourth argument</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static void Dispatch<T, TS, TU, TV>(this Action<T, TS, TU, TV> action, T arg1, TS arg2, TU arg3, TV arg4, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(() => action(arg1, arg2, arg3, arg4), cycle);
        }
        
        #endregion

        #region --- Action Async ---

        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="nullCheck">When enabled a null check is performed before dispatching the action and return
        /// a completed task if the action is null.</param>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static Task DispatchAsync(this Action action, bool nullCheck = true)
        {
            return nullCheck && action == null? Task.CompletedTask : Dispatcher.InvokeAsync(action);
        }
        
        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="cycle">The execution cycle during which the <see cref="Action"/> will be invoked.</param>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <param name="nullCheck">When enabled a null check is performed before dispatching the action and return
        /// a completed task if the action is null.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static Task DispatchAsync(this Action action, ExecutionCycle cycle, bool nullCheck = true)
        {
            return nullCheck && action == null? Task.CompletedTask : Dispatcher.InvokeAsync(action, cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"></param>
        /// <param name="nullCheck">When enabled a null check is performed before dispatching the action and return
        /// a completed task if the action is null.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static Task DispatchAsync(this Action action, CancellationToken ct, bool throwOnCancellation = true, bool nullCheck = true)
        {
            return nullCheck && action == null? Task.CompletedTask : Dispatcher.InvokeAsync(action, ct, throwOnCancellation);
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
        /// <param name="nullCheck">When enabled a null check is performed before dispatching the action and return
        /// a completed task if the action is null.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-ext">Documentation</a></footer>
        public static Task DispatchAsync(this Action action, ExecutionCycle cycle, CancellationToken ct, bool throwOnCancellation = true, bool nullCheck = true)
        {
            return nullCheck && action == null? Task.CompletedTask : Dispatcher.InvokeAsync(action, cycle, ct, throwOnCancellation);
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Func<tresult> Async ---

        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-ext">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<TResult> func)
        {
            return Dispatcher.InvokeAsync(func);
        }
        
        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Func{T}"/> is executed.</param>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-ext">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<TResult> func, ExecutionCycle cycle)
        {
            return Dispatcher.InvokeAsync(func, cycle);
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-ext">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<TResult> func, CancellationToken ct)
        {
            return Dispatcher.InvokeAsync(func, ct);
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-ext">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<TResult> func, ExecutionCycle cycle, CancellationToken ct)
        {
            return Dispatcher.InvokeAsync(func, cycle, ct);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Coroutine ---

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Dispatch(this IEnumerator enumerator)
        {
            Dispatcher.Invoke(enumerator);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Dispatch(this IEnumerator enumerator, MonoBehaviour target)
        {
            Dispatcher.Invoke(enumerator, target);
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Dispatch(this IEnumerator enumerator, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(enumerator, cycle);
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Dispatch(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            Dispatcher.Invoke(enumerator, cycle, target);
        }
        
        #endregion
        
        #region --- Coroutine Async Await Start ---

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, MonoBehaviour target)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, target);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, ct);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, MonoBehaviour target, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, target, ct);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, ExecutionCycle cycle)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle, target);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle, ct);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-start">Documentation</a></footer>
        public static Task<Coroutine> DispatchAsyncAwaitStart(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle, target, ct);
        }
        
        #endregion

        #region --- Coroutine Async Await Competion ---

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that is executed as a <see cref="Coroutine"/>
        /// on the main thread and return a <see cref="Task"/>, that can be awaited and returns
        /// after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, CancellationToken ct, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, ct, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, MonoBehaviour target, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, target, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, MonoBehaviour target, CancellationToken ct, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, target, ct, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, ExecutionCycle cycle, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, cycle, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, cycle, ct, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, cycle, target, throwExceptions);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutine-completion">Documentation</a></footer>
        public static Task DispatchAsyncAwaitCompletion(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct, bool throwExceptions = true)
        {
            return Dispatcher.InvokeAsyncAwaitCompletion(enumerator, cycle, target, ct, throwExceptions);
        }
       
        #endregion
        
        #region --- Coroutine: Obsolete ---

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, MonoBehaviour target)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator,target);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, ct);
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, MonoBehaviour target, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator,target, ct);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, ExecutionCycle cycle)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle, target);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle, ct);
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-ext">Documentation</a></footer>
        [Obsolete("Use DispatchAsyncAwaitStart or DispatchAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> DispatchAsync(this IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct)
        {
            return Dispatcher.InvokeAsyncAwaitStart(enumerator, cycle, target, ct);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Task ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Dispatch(this Func<Task> function)
        {
            Dispatcher.Invoke(function);
        }
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> function that returns the dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Dispatch(this Func<Task> function, ExecutionCycle cycle)
        {
            Dispatcher.Invoke(function, cycle);
        }
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> function that returns the dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Dispatch(this Func<CancellationToken, Task> function, CancellationToken ct, bool throwOnCancellation = true)
        {
            Dispatcher.Invoke(function, ct, throwOnCancellation);
        }
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> function that returns the dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Dispatch(this Func<CancellationToken, Task> function, ExecutionCycle cycle, CancellationToken ct, bool throwOnCancellation = true)
        {
            Dispatcher.Invoke(function, cycle, ct, throwOnCancellation);
        }
        
        #endregion
        
        #region --- Task Async ---
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-async">Documentation</a></footer>
        public static Task DispatchAsync(this Func<Task> function)
        {
            return Dispatcher.InvokeAsync(function);
        }
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> function that returns the dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-async">Documentation</a></footer>
        public static Task DispatchAsync(this Func<Task> function, ExecutionCycle cycle)
        {
            return Dispatcher.InvokeAsync(function, cycle);
        }
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> function that returns the dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-async">Documentation</a></footer>
        public static Task DispatchAsync(this Func<CancellationToken, Task> function, CancellationToken ct, bool throwOnCancellation = true)
        {
            return Dispatcher.InvokeAsync(function, ct, throwOnCancellation);
        }
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Task"/> function that returns the dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-async">Documentation</a></footer>
        public static Task DispatchAsync(this Func<CancellationToken, Task> function, ExecutionCycle cycle, CancellationToken ct, bool throwOnCancellation = true)
        {
            return Dispatcher.InvokeAsync(function, cycle, ct, throwOnCancellation);
        }
        
        #endregion
        
        #region --- Task<tresult> Async ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="function"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<Task<TResult>> function)
        {
            return Dispatcher.InvokeAsync(function);
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<Task<TResult>> function, ExecutionCycle cycle)
        {
            return Dispatcher.InvokeAsync(function, cycle);
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<CancellationToken, Task<TResult>> function, CancellationToken ct)
        {
            return Dispatcher.InvokeAsync(function, ct);
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> DispatchAsync<TResult>(this Func<CancellationToken, Task<TResult>> function, ExecutionCycle cycle, CancellationToken ct)
        {
            return Dispatcher.InvokeAsync(function, cycle, ct);
        }
        
        #endregion
    }
}