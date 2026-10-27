using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterHazard : MonoBehaviour
{
    public enum Kind { Water, Void }

    public Kind kind = Kind.Water;
    [Tooltip("-1 = usa la penalización de FallRespawn")]
    public float regulacionPenalty = -1f;

    [Header("Tiempos")]
    public float waterDelay = 0.6f;     // agua: se ve la salpicadura antes del fundido
    public float voidDelay = 0.25f;     // vacío: sigue cayendo un instante

    [Header("Efectos (opcionales)")]
    public GameObject splashFx;         // puf (solo agua)
    public AudioClip sfx;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var fall = other.GetComponentInParent<FallRespawn>();
        if (fall == null || fall.IsRespawning) return;

        Vector3 pos = fall.transform.position;

        if (kind == Kind.Water)
        {
            float surfaceY = GetComponent<Collider2D>().bounds.max.y;
            if (splashFx != null) Instantiate(splashFx, new Vector3(pos.x, surfaceY, 0f), Quaternion.identity);
            if (sfx != null) AudioSource.PlayClipAtPoint(sfx, pos);
            fall.Respawn(regulacionPenalty, hideTheo: true, preDelay: waterDelay);
        }
        else
        {
            if (sfx != null) AudioSource.PlayClipAtPoint(sfx, pos);
            fall.Respawn(regulacionPenalty, hideTheo: false, preDelay: voidDelay);
        }
    }
}