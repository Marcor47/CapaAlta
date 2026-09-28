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
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("[InventoryHUD] No hay InventorySystem en la escena.");
            return;
        }

        InventorySystem.Instance.OnEquippedChanged += UpdateIcon;
        UpdateIcon(InventorySystem.Instance.Equipped);
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquippedChanged -= UpdateIcon;
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