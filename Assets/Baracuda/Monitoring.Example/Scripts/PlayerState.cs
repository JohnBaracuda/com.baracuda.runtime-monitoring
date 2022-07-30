using System;

namespace Baracuda.Monitoring.Example.Scripts
{
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