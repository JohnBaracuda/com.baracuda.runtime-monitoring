using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class GameSetup : MonoBehaviour
    {
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 200;
        
            Debug.Log($"Setting vSync to [{QualitySettings.vSyncCount}]!", this);
            Debug.Log($"Setting target frame rate to [{Application.targetFrameRate}]!", this);
        }
    }
}
