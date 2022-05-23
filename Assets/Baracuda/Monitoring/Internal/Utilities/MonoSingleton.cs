// Copyright (c) 2022 Jonathan Lang
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Utilities
{
    /// <summary>
    /// Abstract class for making scene persistent MonoBehaviour singletons.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool hideInHierarchy = false;

        public static T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
        
        /// <summary>
        /// Calling <see cref="Promise"/> will guarantee to return an instance.
        /// </summary>
        public static T Promise()
        {
            return Current != null ? Current : new GameObject(typeof(T).Name).AddComponent<T>();
        }

        public static bool TryGetCurrent(out T current)
        {
            current = Current;
            return current != null;
        }
        
        protected virtual void Awake()
        {
            if (Current != null && Current != this)
            {
                Debug.LogWarning(
                    $"Singleton: Multiple instances of the same type {GetType()} detected! Destroying {gameObject}!");
                    
                Destroy(this);
                Destroy(gameObject);
            }
            
            Current = this as T;
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (hideInHierarchy)
            {
                gameObject.hideFlags |= HideFlags.HideInHierarchy;
            }
        }        

        protected virtual void OnDestroy()
        {
            if (Current != this)
            {
                return;
            }

            Current = null;
        }
    }
}