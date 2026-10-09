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
        if (CheckpointManager.Instance == null) return;

        Vector3 spawn = transform.position;
        var account = AccountManager.Instance;

        if (account != null && account.resumeFromCheckpoint)
        {
            if (account.TryGetCheckpoint(CheckpointManager.Instance.chapter, out Vector2 saved))
                spawn = new Vector3(saved.x, saved.y, transform.position.z);
            account.resumeFromCheckpoint = false;
        }

        transform.position = spawn;
        CheckpointManager.Instance.SetCheckpoint(spawn, persist: false); // el punto de entrada no pisa el checkpoint guardado
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