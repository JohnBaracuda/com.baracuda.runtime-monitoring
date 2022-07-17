using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class GameSetup : MonoBehaviour
    {
        //[RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 165;
        
            Debug.Log("Setting vSync to 0!");
            Debug.Log("Setting target frame rate to 165!");
        }

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 165;
        
            Debug.Log("Setting vSync to 0!", this);
            Debug.Log("Setting target frame rate to 165!", this);
        }
    }
}
