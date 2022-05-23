// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Baracuda.Threading.Internal
{
    public static class ExceptionSensitiveCoroutineHandler
    {

        #region --- Public: Methods ---

        public static void StartCoroutineExceptionSensitive(
            IEnumerator enumerator,
            Action<Exception> error,
            Action completed,
            IDisableCallback callback,
            CancellationToken ct,
            MonoBehaviour target
        )
        {
            target.StartCoroutine(StartCoroutineExceptionSensitiveInternal(enumerator, error, completed, callback, ct));
        }
        
        public static void StartCoroutineExceptionSensitive(
            IEnumerator enumerator,
            Action<Exception> error,
            Action completed,
            CancellationToken ct,
            MonoBehaviour target
        )
        {
            // Because coroutines do not return if their target behaviour was disabled, we have to manually add a 
            // component that will give us a callback if the target behaviour is disabled.
            if (!target.TryGetComponent<IDisableCallback>(out var callback))
            {
                callback = target.gameObject.AddComponent<DisableCallback>();
            }
            
            target.StartCoroutine(StartCoroutineExceptionSensitiveInternal(enumerator, error, completed, callback, ct));
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Private: Methods ---
        
        private static IEnumerator StartCoroutineExceptionSensitiveInternal(
            IEnumerator enumerator,
            Action<Exception> error,
            Action completed,
            IDisableCallback callback,
            CancellationToken ct
        )
        {
            // allocating local method so we can unsubscribe it later to prevent memory leaks.
            void OnDisable() => error(new BehaviourDisabledException("Target Behaviour for iterator was disabled!"));

            callback.Disabled += OnDisable;
            while (true)
            {
                object current;
                try
                {
                    ct.ThrowIfCancellationRequested();
                    if (enumerator.MoveNext() == false)
                    {
                        completed();
                        callback.Disabled -= OnDisable;
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception exception)
                {
                    error(exception);
                    callback.Disabled -= OnDisable;
                    yield break;
                }
                yield return current;
            }
        }
        
        private static IEnumerator StartCoroutineExceptionSensitiveInternal(
            IEnumerator enumerator,
            Func<Exception, bool> error,
            Func<bool> completed,
            IDisableCallback callback,
            CancellationToken ct
        )
        {
            // allocating local method so we can unsubscribe it later to prevent memory leaks.
            void OnDisable() => error(new BehaviourDisabledException("Target Behaviour for iterator was disabled!"));

            callback.Disabled += OnDisable;
            while (true)
            {
                object current;
                try
                {
                    ct.ThrowIfCancellationRequested();
                    if (enumerator.MoveNext() == false)
                    {
                        completed();
                        callback.Disabled -= OnDisable;
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception exception)
                {
                    error(exception);
                    callback.Disabled -= OnDisable;
                    yield break;
                }
                yield return current;
            }
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Extension Methods ---

        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="error">Callback invoked when the iterator has thrown an exception.</param>
        /// <param name="completed">Callback invoked when the iterator has finished.</param>
        /// <param name="ct"></param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        public static void StartCoroutineExceptionSensitive(
            this MonoBehaviour target,
            IEnumerator enumerator,
            Func<Exception, bool> error,
            Func<bool> completed,
            CancellationToken ct
        )
        {
            // Because coroutines do not return if their target behaviour was disabled, we have to manually add a 
            // component that will give us a callback if the target behaviour is disabled.
            if (!target.TryGetComponent<IDisableCallback>(out var callbackComponent))
            {
                callbackComponent = target.gameObject.AddComponent<DisableCallback>();
            }

            target.StartCoroutine(StartCoroutineExceptionSensitiveInternal(enumerator, error, completed, callbackComponent, ct));
        }

        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="error">Callback invoked when the iterator has thrown an exception.</param>
        /// <param name="completed">Callback invoked when the iterator has finished.</param>
        /// <param name="ct"></param>
        /// <param name="callback"></param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        public static void StartCoroutineExceptionSensitive(
            this MonoBehaviour target,
            IEnumerator enumerator,
            Func<Exception, bool> error,
            Func<bool> completed,
            CancellationToken ct,
            IDisableCallback callback
        )
        {
            target.StartCoroutine(StartCoroutineExceptionSensitiveInternal(enumerator, error, completed, callback, ct));
        }
        
        
        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="error">Callback invoked when the iterator has thrown an exception.</param>
        /// <param name="completed">Callback invoked when the iterator has finished.</param>
        /// <param name="ct"></param>
        /// <param name="callback"></param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        public static void StartCoroutineExceptionSensitive(
            this MonoBehaviour target,
            IEnumerator enumerator,
            Action<Exception> error,
            Action completed,
            CancellationToken ct,
            IDisableCallback callback
        )
        {
            target.StartCoroutine(StartCoroutineExceptionSensitiveInternal(enumerator, error, completed, callback, ct));
        }

        #endregion
    }
}