// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)

using System;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    /// <summary>
    /// Class providing input based on the old input system.
    /// </summary>
    public class LegacyPlayerInput : MonitoredBehaviour, IPlayerInput
    {
        #region --- Inspector ---

        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode primaryFireKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryFireKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode toggleCursorKey = KeyCode.C;
        
        #endregion

        #region --- Interface: Iplayerinput ---

        public float Vertical { get; private set; }
        
        public float Horizontal { get; private set; }
        
        public float MouseX { get; private set; }
        
        public float MouseY { get; private set; }
        
        public bool JumpPressed { get; private set; }

        public bool PrimaryFirePressed { get; private set; }
        
        public bool SecondaryFirePressed { get; private set; }

        public bool DashPressed { get; private set; }
        public event Action ToggleCursor;

        #endregion
        
        private void Update()
        {
            Vertical = Input.GetAxisRaw("Vertical");
            Horizontal = Input.GetAxisRaw("Horizontal");
            MouseX = Input.GetAxis("Mouse X");
            MouseY = Input.GetAxis("Mouse Y");
            JumpPressed = Input.GetKey(jumpKey);
            PrimaryFirePressed = Input.GetKey(primaryFireKey);
            SecondaryFirePressed = Input.GetKey(secondaryFireKey);
            DashPressed = Input.GetKey(dashKey);

            if (Input.GetKey(toggleCursorKey))
            {
                ToggleCursor?.Invoke();                
            }
        }
    }
}