// Copyright (c) 2022 Jonathan Lang
// If you want to use the Dispatcher with More Efficient Coroutines you have to define the symbol 'EXPERIMENTAL_ENABLE_MEC',
// create an Assembly Definition File for Mec and add it to the Dispatcher ADFs list of Assembly References.
// Please be aware that this is a experimental feature and its implementation might drastically change in future versions.
#if EXPERIMENTAL_ENABLE_MEC
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Utils;
using MEC;
using UnityEngine;

namespace Baracuda.Threading
{
    public partial class Dispatcher
    {
        #region --- Dispatch: Mec Coroutine ---

        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void InvokeMecCoroutine(IEnumerator<float> enumerator)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                Timing.RunCoroutine(enumerator);
            });
        }

        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void InvokeMecCoroutine(IEnumerator<float> enumerator, ExecutionCycle cycle)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                Timing.RunCoroutine(enumerator);
            }, cycle);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Dispatch: Coroutine Async : Await Start ---

        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task<CoroutineHandle> RunMecCoroutineAsyncAwaitStart(IEnumerator<float> enumerator)
        {
            var tcs = new TaskCompletionSource<CoroutineHandle>();

            Invoke(() =>
            {
                try
                {
                    var result = Timing.RunCoroutine(enumerator);
                    tcs.TrySetResult(result);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task<CoroutineHandle> RunMecCoroutineAsyncAwaitStart(IEnumerator<float> enumerator, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<CoroutineHandle>();

            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = Timing.RunCoroutine(enumerator);
                    tcs.TrySetResult(result);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task<CoroutineHandle> RunMecCoroutineAsyncAwaitStart(IEnumerator<float> enumerator, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<CoroutineHandle>();

            Invoke(() =>
            {
                try
                {
                    var result = Timing.RunCoroutine(enumerator);
                    tcs.TrySetResult(result);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }


        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task<CoroutineHandle> RunMecCoroutineAsyncAwaitStart(IEnumerator<float> enumerator, ExecutionCycle cycle,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<CoroutineHandle>();

            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = Timing.RunCoroutine(enumerator);
                    tcs.TrySetResult(result);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }

        #endregion
        
        #region --- Dispatch: Coroutine Async : Await Completion ---

        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task RunMecCoroutineAsyncAwaitCompletion(IEnumerator<float> enumerator)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    Timing.RunCoroutine(RunMecCoroutineInternal(Timing.RunCoroutine(enumerator), tcs.SetCompleted));
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task RunMecCoroutineAsyncAwaitCompletion(IEnumerator<float> enumerator, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    Timing.RunCoroutine(RunMecCoroutineInternal(Timing.RunCoroutine(enumerator), () =>tcs.TrySetCompleted()));
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task RunMecCoroutineAsyncAwaitCompletion(IEnumerator<float> enumerator, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    Timing.RunCoroutine(RunMecCoroutineInternal(Timing.RunCoroutine(enumerator), tcs.SetCompleted));
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }


        /// <summary>
        ///     Experimental feature will change in future versions!<br/>
        ///     Dispatch an <see cref="IEnumerator{Single}" /> that will be started and executed as a MEC Coroutine and
        ///     return a Task, that when awaited on the calling thread, returns the <see cref="CoroutineHandle" />
        ///     of the Coroutine started on the main thread.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task RunMecCoroutineAsyncAwaitCompletion(IEnumerator<float> enumerator, ExecutionCycle cycle, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    Timing.RunCoroutine(RunMecCoroutineInternal(Timing.RunCoroutine(enumerator), () => tcs.TrySetCompleted()));
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }

        private static IEnumerator<float> RunMecCoroutineInternal(CoroutineHandle coroutineHandle, Action callback)
        {
            yield return Timing.WaitUntilDone(coroutineHandle);
            callback();
        }

        #endregion
    }
}

#endif //ENABLE_MEC