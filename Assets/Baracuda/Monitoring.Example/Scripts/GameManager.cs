// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public enum GameState
    {
        Playing,
        Respawning
    }
    
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
        [MFontSize(24)]
        [MShowIf(Condition.Positive)]
        private int _deathTimer = 0;

        private GameState _gameState = GameState.Playing;
        [SerializeField] [Min(30)] private int maxFrameRate = 165;
        [SerializeField] [Range(0,4)] private int vsyncCount = 0;
        [SerializeField] private bool logInit = false;
        
        protected override void Awake()
        {
            base.Awake();
            
            QualitySettings.vSyncCount = vsyncCount;
            Application.targetFrameRate = maxFrameRate;

            if (logInit)
            {
                Debug.Log($"Setting vSync to [{QualitySettings.vSyncCount}]!", this);
                Debug.Log($"Setting target frame rate to [{Application.targetFrameRate}]!", this);
            }
            
            PlayerState.OnPlayerDeath += delegate
            {
                StartCoroutine(PlayerDied());
            };
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(3);
            
            Debug.Log("[Tip] Press F3 to toggle the monitoring display.");
            
            yield return new WaitForSeconds(3);
            
            Debug.Log("[Tip] Press F5 to open a filtering input field.");
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
