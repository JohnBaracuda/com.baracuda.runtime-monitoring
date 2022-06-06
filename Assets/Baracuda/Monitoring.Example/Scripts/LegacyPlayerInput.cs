// Copyright (c) 2022 Jonathan Lang

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

        [SerializeField] private InputMode inputMode = InputMode.Character;
        [Space]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode primaryFireKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryFireKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode toggleFilterKey = KeyCode.Semicolon;
        
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
        public event Action<InputMode> InputModeChanged;

        #endregion

        #region --- Fields ---

        private InputMode _currentInputMode;
        
        #endregion

        private void Start()
        {
            _currentInputMode = inputMode;
            InputModeChanged?.Invoke(_currentInputMode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleFilterKey))
            {
                _currentInputMode = _currentInputMode == InputMode.Character
                    ? InputMode.UserInterface
                    : InputMode.Character;
                if (_currentInputMode == InputMode.UserInterface)
                {
                    Vertical = 0f;
                    Horizontal = 0f;
                    MouseX = 0f;
                    MouseY = 0f;
                    JumpPressed = false;
                    PrimaryFirePressed  = false;
                    SecondaryFirePressed = false;
                    DashPressed = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                InputModeChanged?.Invoke(_currentInputMode);
            }

            if (_currentInputMode == InputMode.Character)
            {
                Vertical = Input.GetAxisRaw("Vertical");
                Horizontal = Input.GetAxisRaw("Horizontal");
                MouseX = Input.GetAxis("Mouse X");
                MouseY = Input.GetAxis("Mouse Y");
                JumpPressed = Input.GetKey(jumpKey);
                PrimaryFirePressed = Input.GetKey(primaryFireKey);
                SecondaryFirePressed = Input.GetKey(secondaryFireKey);
                DashPressed = Input.GetKey(dashKey);
            }
        }
    }
}