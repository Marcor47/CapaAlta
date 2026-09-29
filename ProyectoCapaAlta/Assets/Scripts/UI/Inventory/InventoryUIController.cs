using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject inventoryBox;
    public Transform slotsContainer; // con Horizontal Layout Group + Content Size Fitter
    public GameObject slotPrefab;    // Button + un hijo "Icon" (Image)

    private TheoController theo;
    private bool wasOpen = false;

    void Start()
    {
        theo = FindAnyObjectByType<TheoController>();
        inventoryBox.SetActive(false);
    }

    void Update()
    {
        if (theo == null) return;

        bool isOpenNow = theo.IsBackpackOpen;
        if (isOpenNow != wasOpen)
        {
            inventoryBox.SetActive(isOpenNow);
            if (isOpenNow) PopulateSlots();
            wasOpen = isOpenNow;
        }
    }

    void PopulateSlots()
    {
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);
        Canvas.ForceUpdateCanvases(); // NUEVO — limpia el layout pendiente antes de seguir
        if (InventorySystem.Instance == null) return;

        foreach (var item in InventorySystem.Instance.GetUnlockedItemsOrdered())
        {
            GameObject go = Instantiate(slotPrefab, slotsContainer);

            var icon = go.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null) icon.sprite = InventorySystem.Instance.GetSpriteFor(item);

            var button = go.GetComponent<Button>();
            InventorySystem.ItemType capturedItem = item;
            if (button != null)
                button.onClick.AddListener(() => InventorySystem.Instance.TryEquip(capturedItem));
        }
    }
}