// Copyright (c) 2022 Jonathan Lang
#if  DISPATCHER_DISABLE_UPDATE && DISPATCHER_DISABLE_LATEUPDATE && DISPATCHER_DISABLE_POSTUPDATE && DISPATCHER_DISABLE_FIXEDUPDATE && DISPATCHER_DISABLE_TICKUPDATE
    #define TICKFALLBACK
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Internal;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Baracuda.Threading
{
    /// <summary>
    /// Class for dispatching the execution of a<br/>
    /// <see cref="Delegate"/>, <see cref="IEnumerator"/> or <see cref="Task"/>
    /// from a background thread to the main thread.
    /// </summary>
    /// <footer><a href="https://johnbaracuda.com/dispatcher.html">Documentation</a></footer>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DispatcherPostUpdate))]
    public sealed partial class Dispatcher : MonoBehaviour
    {
        #region --- Utilities ---

        /// <summary>
        /// Returns true if called from the main thread and false if not.
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == (mainThread?.ManagedThreadId 
            ?? throw new Exception($"{nameof(Dispatcher)}.{nameof(mainThread)} is not initialized"));

        /// <summary>
        /// Throws an InvalidOperationException if not called from the main thread.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GuardAgainstIsNotMainThread(string methodCall)
        {
            if (!IsMainThread())
            {
                throw new InvalidOperationException($"{methodCall} is only allowed to bne called from the main thread!");
            }
        }

        /// <summary>
        /// Ensure that a <see cref="Dispatcher"/> instance exists and return it.
        /// This method is just a wrapper for Dispatcher.<see cref="Dispatcher.Current"/>
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        public static Dispatcher Validate() => Current;
        
#if DISPATCHER_DEBUG
        /// <summary>
        /// Get the <see cref="ExecutionCycle"/> definition of the currently executed update cycle.
        /// This property is only available if DISPATCHER_DEBUG is defined.
        /// </summary>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#cycle">Documentation</a></footer>
        public static ExecutionCycle CurrentCycle { get; private set; } = ExecutionCycle.Default;
#endif

        /// <summary>
        /// Return a <see cref="CancellationToken"/> that is valid for the duration of the applications runtime.
        /// This means until OnApplicationQuit is called in a build
        /// or until the play state is changed in the editor. 
        /// </summary>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#runtimeToken">Documentation</a></footer>
        public static CancellationToken RuntimeToken => runtimeCts.Token;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Fields: Private ---
        
        private static readonly Thread mainThread = Thread.CurrentThread;
        
        private static CancellationTokenSource runtimeCts = new CancellationTokenSource();
        
        private static readonly Queue<Action> defaultExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedDefault = false;
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private static readonly Queue<Action> fixedUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedFixed = false;
#endif
        
        
#if !DISPATCHER_DISABLE_UPDATE
        private static readonly Queue<Action> updateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedUpdate = false;
#endif
        
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private static readonly Queue<Action> lateUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedLate = false;
#endif
        
        
#if !DISPATCHER_DISABLE_POSTUPDATE
        private static readonly Queue<Action> postUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedPost = false;
#endif
        
        
#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
        private static readonly Queue<Action> tickExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedTick = false;
#endif
        
#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
        private static readonly Queue<Action> editorExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedEditor = false;
#endif



        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Initialize ---

        private void OnApplicationQuit()
        {
            CancelAndResetRuntimeToken();
        }

        private static void CancelAndResetRuntimeToken()
        {
            runtimeCts.Cancel();
            runtimeCts.Dispose();
            runtimeCts = new CancellationTokenSource();
        }


#if UNITY_EDITOR
        static Dispatcher()
        {
            UnityEditor.EditorApplication.playModeStateChanged += change =>
            {
                
                switch (change)
                {
                    case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                        CancelAndResetRuntimeToken();
                        break;
                }
            };
            
#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
            InitializeTickUpdateLoop();
#endif
            
#if !DISPATCHER_DISABLE_EDITORUPDATE
            UnityEditor.EditorApplication.update += EditorUpdate;
#endif
        }
#endif // UNITY_EDITOR

#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
        private static void EditorUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.EditorUpdate;      
#endif
            if (queuedEditor)
            {
                lock (editorExecutionQueue)
                {
                    while (editorExecutionQueue.Count > 0)
                    {
                        editorExecutionQueue.Dequeue().Invoke();
                    }

                    queuedEditor = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void DispatchAfterAssembliesLoaded() => ReleaseDefaultQueue();
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DispatchBeforeSceneLoad() => ReleaseDefaultQueue();
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void DispatchAfterSceneLoad() => ReleaseDefaultQueue();
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Update: Tick ---

#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
        
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private const int TICK_DELAY = 50;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitTick() => InitializeTickUpdateLoop();
        
        private static void InitializeTickUpdateLoop()
        {
            StopTick();
            cts = new CancellationTokenSource();
            TickUpdate(cts.Token);
        }

        private static void StopTick()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        
        private static async void TickUpdate(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
#if DISPATCHER_DEBUG
                CurrentCycle = ExecutionCycle.TickUpdate;      
#endif
                if (queuedTick)
                {
                    lock (tickExecutionQueue)
                    {
                        while (tickExecutionQueue.Count > 0)
                        {
                            tickExecutionQueue.Dequeue().Invoke();
                        }
                    }
                }

                ReleaseDefaultQueue();
                
                await Task.Delay(TICK_DELAY, ct);
            }
        }
#endif
        
        #endregion
        
        #region --- Update: Monobehaviour ---
        
#if !DISPATCHER_DISABLE_UPDATE
        private void Update()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.Update;      
#endif
            if (queuedUpdate)
            {
                lock (updateExecutionQueue)
                {
                    while (updateExecutionQueue.Count > 0)
                    {
                        updateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedUpdate = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif
        
#if !DISPATCHER_DISABLE_LATEUPDATE
        private void LateUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.LateUpdate;      
#endif
            if (queuedLate)
            {
                lock (lateUpdateExecutionQueue)
                {
                    while (lateUpdateExecutionQueue.Count > 0)
                    {
                        lateUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedLate = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private void FixedUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.FixedUpdate;      
#endif
            if (queuedFixed)
            {
                lock (fixedUpdateExecutionQueue)
                {
                    while (fixedUpdateExecutionQueue.Count > 0)
                    {
                        fixedUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedFixed = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif

#if !DISPATCHER_DISABLE_POSTUPDATE
        internal static void PostUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.PostUpdate;      
#endif
            if (queuedPost)
            {
                lock (postUpdateExecutionQueue)
                {
                    while (postUpdateExecutionQueue.Count > 0)
                    {
                        postUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedPost = false;
                }
            }

            ReleaseDefaultQueue();
        }
#endif

        #endregion

        #region --- Release ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReleaseDefaultQueue()
        {
            if (!queuedDefault)
            {
                return;
            }

            lock (defaultExecutionQueue)
            {
                while (defaultExecutionQueue.Count > 0)
                {
                    defaultExecutionQueue.Dequeue()?.Invoke();
                }

                queuedDefault = false;
            }
        }

        #endregion
    }
}

