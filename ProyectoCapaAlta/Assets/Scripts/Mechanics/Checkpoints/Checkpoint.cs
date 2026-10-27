using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;                  // cuadro 0: apagado
    public SpriteFrameAnimator litAnimator;   // cuadros 1-3

    [Header("Reaparición")]
    [Tooltip("Dónde aparece Theo respecto a la base del faro. Ajusta Y para que sus pies toquen el piso (ver gizmo)")]
    public Vector2 spawnOffset = new Vector2(0.75f, 0.55f);
    public float matchRadius = 0.15f;

    private bool lit;
    Vector3 SpawnPos => transform.position + (Vector3)spawnOffset;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (litAnimator == null) litAnimator = GetComponent<SpriteFrameAnimator>();
        SetLit(false);
    }

    IEnumerator Start()
    {
        yield return null;
        yield return null; // FallRespawn.Start ya aplicó el checkpoint guardado
        if (CheckpointManager.Instance != null &&
            Vector2.Distance(CheckpointManager.Instance.GetCheckpoint(), SpawnPos) <= matchRadius)
            SetLit(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var cm = CheckpointManager.Instance;
        bool alreadyCurrent = lit && cm != null && Vector2.Distance(cm.GetCheckpoint(), SpawnPos) <= matchRadius;
        if (cm != null && !alreadyCurrent) cm.SetCheckpoint(SpawnPos);   // evita reescribir PlayerPrefs en cada respawn
        SetLit(true);
    }

    void SetLit(bool on)
    {
        if (on && lit) return;
        lit = on;

        if (litAnimator != null)
        {
            if (on) litAnimator.Play();
            else litAnimator.Stop();
        }
        if (!on && spriteRenderer != null && offSprite != null)
            spriteRenderer.sprite = offSprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(SpawnPos, 0.12f);
    }
}