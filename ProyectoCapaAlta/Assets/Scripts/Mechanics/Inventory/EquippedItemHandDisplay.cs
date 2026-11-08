using UnityEngine;

public class EquippedItemHandDisplay : MonoBehaviour
{
    [Header("Tamaño en la mano")]
    [Tooltip("Tamaño deseado en unidades del mundo, sin importar el tamaño real del sprite")]
    public float targetSize = 0.5f;

    private SpriteRenderer sr;
    private Animator theoAnim;
    private bool hasItem;
    private bool poseHidden;   // NUEVO: true si el ítem está oculto por una pose de herramienta

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        theoAnim = GetComponentInParent<Animator>();
    }

    void Start()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnEquippedChanged += UpdateSprite;
            UpdateSprite(InventorySystem.Instance.Equipped);
        }
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquippedChanged -= UpdateSprite;
    }

    // Oculta el ítem de la mano mientras Theo está en una pose con la herramienta dibujada.
    // Solo escribe cuando cambia el estado, para no pisar el ocultamiento del respawn/hundimiento.
    void LateUpdate()
    {
        if (!hasItem || theoAnim == null) return;
        bool pose = theoAnim.IsInTransition(0)
            ? theoAnim.GetNextAnimatorStateInfo(0).IsTag("ToolPose")
            : theoAnim.GetCurrentAnimatorStateInfo(0).IsTag("ToolPose");
        if (pose == poseHidden) return;
        poseHidden = pose;
        sr.enabled = !pose;
    }

    void UpdateSprite(InventorySystem.ItemType item)
    {
        hasItem = item != InventorySystem.ItemType.None;
        poseHidden = false;   // NUEVO
        if (!hasItem)
        {
            sr.enabled = false;
            return;
        }

        sr.enabled = true;
        sr.sprite = InventorySystem.Instance.GetSpriteFor(item);

        if (sr.sprite != null)
        {
            float largestSide = Mathf.Max(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
            if (largestSide > 0f)
            {
                float scale = targetSize / largestSide;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}