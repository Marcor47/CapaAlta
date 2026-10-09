using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Tooltip("Número de capítulo de esta escena (1-4)")]
    public int chapter = 1;

    private Vector3 currentCheckpoint;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetCheckpoint(Vector3 position, bool persist = true)
    {
        currentCheckpoint = position;
        if (persist && AccountManager.Instance != null)
            AccountManager.Instance.SaveCheckpoint(chapter, position);
    }

    public Vector3 GetCheckpoint() => currentCheckpoint;
}