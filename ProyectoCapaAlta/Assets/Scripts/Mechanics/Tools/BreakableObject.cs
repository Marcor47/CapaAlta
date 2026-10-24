using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour
{
    public float fallGravityScale = 2f;
    public float destroyAfterSeconds = 0f;  // 0 = se queda donde cae
    public UnityEvent onBroken;             // para reacciones propias (abrir paso, etc.)

    private bool broken;

    public void Hit()
    {
        if (broken) return;
        broken = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = fallGravityScale;
        }

        onBroken?.Invoke();
        if (destroyAfterSeconds > 0f) Destroy(gameObject, destroyAfterSeconds);
    }
}