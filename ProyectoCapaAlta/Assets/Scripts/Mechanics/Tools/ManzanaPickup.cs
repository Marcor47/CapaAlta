using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ManzanaPickup : MonoBehaviour
{
    public float pickupDelay = 0f;   // manzanas lanzadas: ~0.6, para no recogerlas al instante

    private float readyTime;
    private bool picked;

    void Start() => readyTime = Time.time + pickupDelay;

    void OnTriggerEnter2D(Collider2D other) => TryPickup(other);
    void OnTriggerStay2D(Collider2D other) => TryPickup(other);

    void TryPickup(Collider2D other)
    {
        if (picked || Time.time < readyTime || !other.CompareTag("Player")) return;
        picked = true;

        if (DecisionRecord.Instance != null) DecisionRecord.Instance.AddManzana();
        Destroy(gameObject);
    }
}