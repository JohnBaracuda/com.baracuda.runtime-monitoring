using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 165;
    }
}
