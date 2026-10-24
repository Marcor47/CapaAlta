using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ThrownProjectile : MonoBehaviour
{
    [Header("Comportamiento")]
    public bool breaksBreakables = true;   // roca: sí | manzana: no
    public bool destroyOnHit = true;       // roca: sí | manzana: no (queda en el suelo)
    public float lifetime = 3f;            // 0 = no se destruye sola (manzana)
    public string breakableTag = "Breakable";

    [Header("Seguridad")]
    public float killY = -40f;
    public bool returnAppleIfLost = false; // manzana: se devuelve si cae al vacío

    private Rigidbody2D rb;
    private bool resolved;

    public void Launch(Vector2 direction, float speed, GameObject owner)
    {
        rb = GetComponent<Rigidbody2D>();

        // El proyectil no choca con quien lo lanza (los triggers sí se mantienen)
        if (owner != null)
            foreach (var mine in GetComponentsInChildren<Collider2D>())
                if (!mine.isTrigger)
                    foreach (var theirs in owner.GetComponentsInChildren<Collider2D>())
                        Physics2D.IgnoreCollision(mine, theirs);

        rb.linearVelocity = direction.normalized * speed;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (resolved || transform.position.y >= killY) return;
        resolved = true;

        if (returnAppleIfLost && DecisionRecord.Instance != null)
            DecisionRecord.Instance.AddManzana();
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (breaksBreakables && collision.collider.CompareTag(breakableTag))
            collision.collider.GetComponentInParent<BreakableObject>()?.Hit();

        if (destroyOnHit) Destroy(gameObject);
    }
}