// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring;
using System;
using UnityEngine;

namespace Baracuda.Example.Scripts
{
    /// <summary>
    /// Class providing input based on the old input system.
    /// </summary>
    public class LegacyPlayerInput : MonitoredBehaviour, IPlayerInput
    {
        #region Inspector ---

        [Header("Monitoring")]
        [SerializeField] private KeyCode toggleFilterKey = KeyCode.F5;
        [SerializeField] private KeyCode toggleMonitoringKey = KeyCode.F3;

        [Header("Input Mode")]
        [SerializeField] private InputMode inputMode = InputMode.Character;

        [Header("Movement")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode primaryFireKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryFireKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode clearConsoleKey = KeyCode.C;

        #endregion

        #region Static ---

        public static KeyCode ToggleFilterKey;
        public static KeyCode ToggleMonitoringKey;

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

        public event Action ToggleMonitoring;
        public event Action<InputMode> InputModeChanged;
        public event Action ClearConsole;

        #endregion

        #region --- Fields ---

        private InputMode _currentInputMode;

        #endregion

        protected override void Awake()
        {
            base.Awake();
#if UNITY_WEBGL && !UNITY_EDITOR
            toggleFilterKey = KeyCode.Alpha1;
            toggleMonitoringKey = KeyCode.Alpha2;
#endif
            ToggleFilterKey = toggleFilterKey;
            ToggleMonitoringKey = toggleMonitoringKey;
        }

        private void Start()
        {
            _currentInputMode = inputMode;
            InputModeChanged?.Invoke(_currentInputMode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleMonitoringKey))
            {
                ToggleMonitoring?.Invoke();
            }

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
                    PrimaryFirePressed = false;
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

            if (Input.GetKeyDown(clearConsoleKey))
            {
                ClearConsole?.Invoke();
            }
        }
    }
}