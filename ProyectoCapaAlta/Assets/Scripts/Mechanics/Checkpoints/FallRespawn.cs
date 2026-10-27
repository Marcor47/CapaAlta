using System.Collections;
using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    [Header("Configuración")]
    public float fallThresholdY = -10f;          // red de seguridad bajo el vacío
    public float regulacionPenaltyOnFall = 20f;

    [Header("Transición")]
    public float fadeOutTime = 0.3f;
    public float blackHold = 0.35f;              // pantalla negra: la cámara se acomoda
    public float fadeInTime = 0.3f;
    public bool restoreStaminaOnRespawn = true;
    public float totalRespawnTime = 4f;   // desde que cae hasta que recupera el control (3 a 5)

    private TheoController theo;
    private Rigidbody2D rb;
    private bool respawning;

    public bool IsRespawning => respawning;

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
        CheckpointManager.Instance.SetCheckpoint(spawn, persist: false);
    }

    void Update()
    {
        if (!respawning && transform.position.y < fallThresholdY)
            Respawn();
    }

    // hideTheo = true (agua): Theo se congela y se oculta. false (vacío): sigue cayendo mientras oscurece.
    // preDelay: segundos antes de empezar el fundido.
    public void Respawn(float penaltyOverride = -1f, bool hideTheo = false, float preDelay = 0f)
    {
        if (respawning) return;
        respawning = true;
        StartCoroutine(RespawnRoutine(penaltyOverride, hideTheo, preDelay));
    }

    IEnumerator RespawnRoutine(float penaltyOverride, bool hideTheo, float preDelay)
    {
        float startTime = Time.unscaledTime;

        if (theo != null)
        {
            theo.BeginRespawn(hideTheo);
            theo.DecreaseRegulacion(penaltyOverride >= 0f ? penaltyOverride : regulacionPenaltyOnFall);
        }

        if (preDelay > 0f) yield return new WaitForSecondsRealtime(preDelay);

        var fader = ScreenFader.Instance;
        yield return fader.FadeTo(1f, fadeOutTime);

        Vector3 cp = CheckpointManager.Instance != null ? CheckpointManager.Instance.GetCheckpoint() : transform.position;
        if (theo != null)
        {
            theo.EndRespawn(cp);
            if (restoreStaminaOnRespawn) theo.RestoreStamina();
        }
        else
        {
            transform.position = cp;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // completa el tiempo total en pantalla negra
        float remaining = totalRespawnTime - (Time.unscaledTime - startTime) - fadeInTime;
        yield return new WaitForSecondsRealtime(Mathf.Max(blackHold, remaining));

        yield return fader.FadeTo(0f, fadeInTime);
        if (theo != null) theo.FinishRespawn();
        respawning = false;
    }
}