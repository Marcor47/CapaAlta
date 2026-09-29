using UnityEngine;

public class EquippedItemHandDisplay : MonoBehaviour
{
    [Header("Tamaño en la mano")]
    [Tooltip("Tamaño deseado en unidades del mundo, sin importar el tamaño real del sprite")]
    public float targetSize = 0.5f; // ajusta este valor mirando la escena — ver nota abajo

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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

    void UpdateSprite(InventorySystem.ItemType item)
    {
        if (item == InventorySystem.ItemType.None)
        {
            sr.enabled = false;
            return;
        }

        sr.enabled = true;
        sr.sprite = InventorySystem.Instance.GetSpriteFor(item);

        // NUEVO — normaliza el tamaño visual sin importar el tamaño real del sprite
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