using Baracuda.Monitoring;
using System;

namespace Baracuda.Example.Scripts
{
#pragma warning disable CS0414

    public class PlayerState : MonitoredBehaviour
    {
        public static event Action OnPlayerDeath;

        private void Start()
        {
            GameManager.Current.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            gameObject.SetActive(gameState == GameState.Playing);
        }

        public void Die()
        {
            OnPlayerDeath?.Invoke();
        }
    }
}