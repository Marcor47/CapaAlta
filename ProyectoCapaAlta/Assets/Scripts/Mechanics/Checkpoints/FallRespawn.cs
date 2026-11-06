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

    [Header("Entrada al nivel")]
    public float introFadeIn = 0.6f;
    public float introMargin = 1f;   // cuánto fuera de pantalla empieza Theo

    IEnumerator Start()
    {
        theo = GetComponent<TheoController>();
        rb = GetComponent<Rigidbody2D>();
        if (CheckpointManager.Instance == null) yield break;

        Vector3 spawn = transform.position;
        bool fromCheckpoint = false;
        var account = AccountManager.Instance;

        if (account != null && account.resumeFromCheckpoint)
        {
            if (account.TryGetCheckpoint(CheckpointManager.Instance.chapter, out Vector2 saved))
            {
                spawn = new Vector3(saved.x, saved.y, transform.position.z);
                fromCheckpoint = true;
            }
            account.resumeFromCheckpoint = false;
        }

        transform.position = spawn;
        CheckpointManager.Instance.SetCheckpoint(spawn, persist: false);

        if (theo == null) yield break;
        yield return fromCheckpoint ? AppearAtCheckpoint() : WalkInFromLeft(spawn);
    }

    IEnumerator AppearAtCheckpoint()
    {
        theo.SetMovementLocked(true);
        var fader = ScreenFader.Instance;
        fader.SetAlpha(1f);
        yield return new WaitForSecondsRealtime(0.3f);   // la cámara se acomoda
        yield return fader.FadeTo(0f, introFadeIn);
        theo.SetMovementLocked(false);
    }

    IEnumerator WalkInFromLeft(Vector3 spawn)
    {
        theo.SetMovementLocked(true);
        var fader = ScreenFader.Instance;
        fader.SetAlpha(1f);

        yield return null;   // Cinemachine encuadra el punto de partida
        yield return null;

        var cam = Camera.main;
        float leftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        transform.position = new Vector3(leftEdge - introMargin, spawn.y, spawn.z);

        theo.BeginAutoWalk(1f);
        StartCoroutine(fader.FadeTo(0f, introFadeIn));   // el fundido corre mientras entra
        while (spawn.x - transform.position.x > theo.autoWalkSpeed * Time.fixedDeltaTime)
            yield return null;

        theo.EndAutoWalk();
        transform.position = spawn;
        theo.SetMovementLocked(false);
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