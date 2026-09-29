using UnityEngine;

public class EquippedItemHandDisplay : MonoBehaviour
{
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
    }
}