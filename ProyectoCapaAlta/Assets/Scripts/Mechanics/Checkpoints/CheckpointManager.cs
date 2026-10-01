using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    private Vector3 currentCheckpoint;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; // sin DontDestroyOnLoad — es propio de cada escena de capítulo
    }

    public void SetCheckpoint(Vector3 position) => currentCheckpoint = position;
    public Vector3 GetCheckpoint() => currentCheckpoint;
}