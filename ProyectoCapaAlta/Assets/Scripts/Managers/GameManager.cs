using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        // vSync en 0 para que targetFrameRate funcione correctamente en WebGL
    }
}