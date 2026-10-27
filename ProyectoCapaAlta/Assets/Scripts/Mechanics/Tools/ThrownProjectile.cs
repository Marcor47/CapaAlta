using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ThrownProjectile : MonoBehaviour
{
    [Header("Comportamiento")]
    public bool breaksBreakables = true;   // pelota: sí | manzana: no
    public bool destroyOnHit = true;       // pelota: sí (sin rebote) | manzana: no (queda en el suelo)
    public float lifetime = 3f;            // 0 = no se destruye sola (manzana)
    public string breakableTag = "Breakable";

    [Header("Efecto de impacto (opcional)")]
    public GameObject impactFx;            // prefab del puf (SpriteFrameAnimator con Destroy On End)
    public bool fxOnExpire = true;         // también al agotarse el alcance

    [Header("Seguridad")]
    public float killY = -40f;
    public bool returnAppleIfLost = false; // manzana: se devuelve si cae al vacío

    private Rigidbody2D rb;
    private bool resolved;

    public void Launch(Vector2 direction, float speed, GameObject owner)
    {
        rb = GetComponent<Rigidbody2D>();
        // Un proyectil chiquito y rápido atraviesa colisionadores finos si no es Continuous
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // El proyectil no choca con quien lo lanza (los triggers sí se mantienen)
        if (owner != null)
            foreach (var mine in GetComponentsInChildren<Collider2D>())
                if (!mine.isTrigger)
                    foreach (var theirs in owner.GetComponentsInChildren<Collider2D>())
                        Physics2D.IgnoreCollision(mine, theirs);

        rb.linearVelocity = direction.normalized * speed;
        if (lifetime > 0f) Invoke(nameof(Expire), lifetime);
    }

    void Expire()
    {
        if (resolved) return;
        resolved = true;
        if (fxOnExpire) SpawnFx(transform.position);
        Destroy(gameObject);
    }

    void Update()
    {
        if (resolved || transform.position.y >= killY) return;
        resolved = true;

        if (returnAppleIfLost && DecisionRecord.Instance != null)
            DecisionRecord.Instance.AddManzana();
        Destroy(gameObject);
    }

    // Choque con colisionadores sólidos (roca intacta, suelo, paredes...)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (resolved) return;

        if (breaksBreakables && collision.collider.CompareTag(breakableTag))
            collision.collider.GetComponentInParent<BreakableObject>()?.Hit();

        if (destroyOnHit)
        {
            resolved = true;
            SpawnFx(collision.contactCount > 0 ? collision.GetContact(0).point : (Vector2)transform.position);
            Destroy(gameObject);
        }
    }

    // Los escombros son trigger (Theo los atraviesa) pero la pelota los tiene que detectar
    void OnTriggerEnter2D(Collider2D other)
    {
        if (resolved || !breaksBreakables || !other.CompareTag(breakableTag)) return;

        other.GetComponentInParent<BreakableObject>()?.Hit();

        if (destroyOnHit)
        {
            resolved = true;
            SpawnFx(transform.position);
            Destroy(gameObject);
        }
    }

    void SpawnFx(Vector2 pos)
    {
        if (impactFx != null) Instantiate(impactFx, pos, Quaternion.identity);
    }
}
