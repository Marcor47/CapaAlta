using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ManzanaPickup : MonoBehaviour
{
    public float pickupDelay = 0f;   // manzanas lanzadas: ~0.6, para no recogerlas al instante

    private float readyTime;
    private bool picked;
    private string worldID;          // solo manzanas colocadas en el mundo (pickupDelay = 0)

    void Start()
    {
        readyTime = Time.time + pickupDelay;

        if (pickupDelay > 0f) return;   // lanzada: siempre se devuelve al inventario

        var pos = transform.position;
        worldID = $"{SceneManager.GetActiveScene().name}_{gameObject.name}_{pos.x:F2}_{pos.y:F2}";

        var user = AccountManager.Instance != null ? AccountManager.Instance.CurrentUser : null;
        if (user != null && user.progress.manzanaNodesGiven.Contains(worldID))
            Destroy(gameObject);        // ya la recogió esta cuenta
    }

    void OnTriggerEnter2D(Collider2D other) => TryPickup(other);
    void OnTriggerStay2D(Collider2D other) => TryPickup(other);

    void TryPickup(Collider2D other)
    {
        if (picked || Time.time < readyTime || !other.CompareTag("Player")) return;
        picked = true;

        if (DecisionRecord.Instance != null)
        {
            if (worldID != null) DecisionRecord.Instance.AddManzanaFromNode(worldID);
            else DecisionRecord.Instance.AddManzana();
        }
        Destroy(gameObject);
    }
}