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
        #region --- [UTILITIES] ---

        /// <summary>
        /// Returns true if called from the main thread and false if not.
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == (_mainThread?.ManagedThreadId 
            ?? throw new Exception($"{nameof(Dispatcher)}.{nameof(_mainThread)} is not initialized"));

        
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
        public static CancellationToken RuntimeToken => _runtimeCts.Token;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS: PRIVATE] ---
        
        private static readonly Thread _mainThread = Thread.CurrentThread;
        
        private static CancellationTokenSource _runtimeCts = new CancellationTokenSource();
        
        private static readonly Queue<Action> _defaultExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedDefault = false;
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private static readonly Queue<Action> _fixedUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedFixed = false;
#endif
        
        
#if !DISPATCHER_DISABLE_UPDATE
        private static readonly Queue<Action> _updateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedUpdate = false;
#endif
        
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private static readonly Queue<Action> _lateUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedLate = false;
#endif
        
        
#if !DISPATCHER_DISABLE_POSTUPDATE
        private static readonly Queue<Action> _postUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedPost = false;
#endif
        
        
#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
        private static readonly Queue<Action> _tickExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedTick = false;
#endif
        
#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
        private static readonly Queue<Action> _editorExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedEditor = false;
#endif



        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INITIALIZE] ---

        private void OnApplicationQuit()
        {
            CancelAndResetRuntimeToken();
        }

        private static void CancelAndResetRuntimeToken()
        {
            _runtimeCts.Cancel();
            _runtimeCts.Dispose();
            _runtimeCts = new CancellationTokenSource();
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
            if (_queuedEditor)
            {
                lock (_editorExecutionQueue)
                {
                    while (_editorExecutionQueue.Count > 0)
                    {
                        _editorExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedEditor = false;
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

        #region --- [UPDATE: TICK] ---

#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
        
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private const int TICK_DELAY = 50;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitTick() => InitializeTickUpdateLoop();
        
        private static void InitializeTickUpdateLoop()
        {
            StopTick();
            _cts = new CancellationTokenSource();
            TickUpdate(_cts.Token);
        }

        private static void StopTick()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        
        private static async void TickUpdate(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
#if DISPATCHER_DEBUG
                CurrentCycle = ExecutionCycle.TickUpdate;      
#endif
                if (_queuedTick)
                {
                    lock (_tickExecutionQueue)
                    {
                        while (_tickExecutionQueue.Count > 0)
                        {
                            _tickExecutionQueue.Dequeue().Invoke();
                        }
                    }
                }

                ReleaseDefaultQueue();
                
                await Task.Delay(TICK_DELAY, ct);
            }
        }
#endif
        
        #endregion
        
        #region --- [UPDATE: MONOBEHAVIOUR] ---
        
#if !DISPATCHER_DISABLE_UPDATE
        private void Update()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.Update;      
#endif
            if (_queuedUpdate)
            {
                lock (_updateExecutionQueue)
                {
                    while (_updateExecutionQueue.Count > 0)
                    {
                        _updateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedUpdate = false;
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
            if (_queuedLate)
            {
                lock (_lateUpdateExecutionQueue)
                {
                    while (_lateUpdateExecutionQueue.Count > 0)
                    {
                        _lateUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedLate = false;
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
            if (_queuedFixed)
            {
                lock (_fixedUpdateExecutionQueue)
                {
                    while (_fixedUpdateExecutionQueue.Count > 0)
                    {
                        _fixedUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedFixed = false;
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
            if (_queuedPost)
            {
                lock (_postUpdateExecutionQueue)
                {
                    while (_postUpdateExecutionQueue.Count > 0)
                    {
                        _postUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedPost = false;
                }
            }

            ReleaseDefaultQueue();
        }
#endif

        #endregion

        #region --- [RELEASE] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReleaseDefaultQueue()
        {
            if (!_queuedDefault) return;
            
            lock (_defaultExecutionQueue)
            {
                while (_defaultExecutionQueue.Count > 0)
                {
                    _defaultExecutionQueue.Dequeue()?.Invoke();
                }

                _queuedDefault = false;
            }
        }

        #endregion
    }
}

