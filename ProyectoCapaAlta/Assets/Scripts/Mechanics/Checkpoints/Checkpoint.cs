using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SetCheckpoint(transform.position);
    }
}