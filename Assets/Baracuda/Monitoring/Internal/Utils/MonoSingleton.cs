using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Utils
{
    /// <summary>
    /// Abstract class for making scene persistent MonoBehaviour singletons.
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region --- [ACCESS] ---
        
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        /// <summary>
        /// Try get the current instance.
        /// </summary>
        public static bool TryGetCurrent(out T current)
        {
            current = Current;
            return current != null;
        }

        /// <summary>
        /// Create a new instance of <see cref="T"/>
        /// </summary>
        /// <returns></returns>
        public static T EnsureExist()
        {
            return Current != null ? Current : new GameObject(typeof(T).Name).AddComponent<T>();
        }

        
        /// <summary>
        /// Get the active instance of <see cref="T"/>.
        /// </summary>
        public static T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---
        
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
        }        

        #endregion

        #region --- [TERMINATION] ---

        protected virtual void OnDestroy()
        {
            if (Current != this) return;
            Current = null;
        }

        #endregion
    }
}