using UnityEngine;

namespace Baracuda.Example.Scripts
{
    public class DeathCameraController : MonoBehaviour
    {
        [SerializeField] private GameObject cameraGameObject;

        private void Start()
        {
            GameManager.Current.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            cameraGameObject.SetActive(gameState == GameState.Respawning);
        }

        private void Update()
        {
            transform.Rotate (0,5 * Time.deltaTime,0);
        }
    }
}
