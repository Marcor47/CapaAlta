using UnityEngine;
using UnityEngine.UI;

public class InventoryHUD : MonoBehaviour
{
    [Header("Referencias")]
    public Image equippedIcon;
    public Sprite manzanaSprite;
    public Sprite telefonoSprite;

    void OnEnable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquippedChanged += UpdateIcon;
    }

    void OnDisable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquippedChanged -= UpdateIcon;
    }

    void Start()
    {
        if (InventorySystem.Instance != null)
            UpdateIcon(InventorySystem.Instance.Equipped);
    }

    void UpdateIcon(InventorySystem.ItemType item)
    {
        if (item == InventorySystem.ItemType.None)
        {
            equippedIcon.enabled = false;
            return;
        }

        equippedIcon.enabled = true;
        equippedIcon.sprite = item == InventorySystem.ItemType.Manzana ? manzanaSprite : telefonoSprite;
    }
}