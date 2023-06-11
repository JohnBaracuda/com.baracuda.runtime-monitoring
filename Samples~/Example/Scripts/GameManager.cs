// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring;
using System;
using System.Collections;
using UnityEngine;

namespace Baracuda.Example.Scripts
{
    public class GameManager : MonitoredSingleton<GameManager>
    {
        [Monitor]
        [MOrder(100)]
        public event Action<GameState> GameStateChanged;

        [Monitor]
        [MOrder(100)]
        public GameState GameState
        {
            get => _gameState;
            set
            {
                _gameState = value;
                GameStateChanged?.Invoke(value);
            }
        }

        [Monitor]
        [MFontSize(26)]
        [MGroupElement(false)]
        [MOrder(100)]
        [MShowIf(Condition.Positive)]
        private int _deathTimer;

        private GameState _gameState = GameState.Playing;
#pragma warning disable
        [SerializeField] [Min(30)] private int maxFrameRate = 165;
        [SerializeField] [Range(0, 4)] private int vsyncCount;
        [SerializeField] private bool logInit;
#pragma warning restore

        protected override void Awake()
        {
            base.Awake();
#if !UNITY_WEBGL
            QualitySettings.vSyncCount = vsyncCount;
            Application.targetFrameRate = maxFrameRate;
#endif

            if (logInit)
            {
                Debug.Log($"Setting vSync to [{QualitySettings.vSyncCount}]!", this);
                Debug.Log($"Setting target frame rate to [{Application.targetFrameRate}]!", this);
            }
        }

        private IEnumerator Start()
        {
            PlayerState.OnPlayerDeath += delegate { StartCoroutine(PlayerDied()); };

            yield return new WaitForSeconds(3);

            Debug.Log($"[Tip] Press [{LegacyPlayerInput.ToggleMonitoringKey}] to toggle the monitoring display.");

            yield return new WaitForSeconds(3);

            Debug.Log($"[Tip] Press [{LegacyPlayerInput.ToggleFilterKey}] to open a filtering input field.");
        }

        private IEnumerator PlayerDied()
        {
            GameState = GameState.Respawning;
            _deathTimer = 5;
            while (_deathTimer > 0)
            {
                yield return new WaitForSeconds(1);
                _deathTimer--;
            }
            GameState = GameState.Playing;
        }
    }
}