// Copyright (c) 2022 Jonathan Lang
using System;
using UnityEngine;

namespace Baracuda.Threading
{
    public partial class Dispatcher
    {
        #region --- Singleton ---

        // backing field for Dispatcher.Current
        private static Dispatcher current;
        
        // flag to determine if an invalid operation exception should be thrown when destroying the GameObject.
        private bool _throw = true;
        
        /// <summary>
        /// Get the current instance of <see cref="Dispatcher"/>. If no instance can be found a new object is created.
        /// </summary>
        public static Dispatcher Current
        {
            get
            {
                if (current == null)
                {
                    current = FindObjectOfType<Dispatcher>() 
                               ?? new GameObject(nameof(Dispatcher)).AddComponent<Dispatcher>();
                }
                return current;
            }
        }
        
        private void Awake()
        {
            if(this == null) return;
            
            if (Current != null && Current != this)
            {
                Debug.LogWarning($"Multiple Dispatcher detected! Destroying {gameObject.name} Please ensure that there is only one Dispatcher in your scene!");
                _throw = false;
                Destroy(gameObject);
                return;
            }
            
            current = this;

            DontDestroyOnLoad(gameObject);
        }


        private void OnDestroy()
        {
            if (Current != this) return;
            current = null;
            
            if (_throw && gameObject.scene.isLoaded)
            {
                Validate();
                throw new InvalidOperationException(
                    $"{nameof(Dispatcher)} was destroyed during playmode. Please ensure that the {nameof(Dispatcher)} is not destroyed during playmode!");
            }
        }
        
        #endregion
    }
}

