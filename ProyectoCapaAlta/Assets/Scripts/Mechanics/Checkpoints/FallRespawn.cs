using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    [Header("Configuración")]
    public float fallThresholdY = -10f; // ajusta según el nivel más bajo jugable de cada capítulo
    public float regulacionPenaltyOnFall = 20f;

    private TheoController theo;
    private Rigidbody2D rb;

    void Start()
    {
        theo = GetComponent<TheoController>();
        rb = GetComponent<Rigidbody2D>();

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SetCheckpoint(transform.position); // punto de entrada del capítulo
    }

    void Update()
    {
        if (transform.position.y < fallThresholdY)
            Respawn();
    }

    void Respawn()
    {
        Vector3 checkpoint = CheckpointManager.Instance != null ? CheckpointManager.Instance.GetCheckpoint() : transform.position;
        transform.position = checkpoint;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (theo != null) theo.DecreaseRegulacion(regulacionPenaltyOnFall);
    }
}