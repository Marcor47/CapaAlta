using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        if (isOpenNow) RefreshCounts();
    }

    void RefreshCounts()
    {
        if (DecisionRecord.Instance == null) return;
        foreach (Transform slot in slotsContainer)
        {
            var countText = slot.Find("Count")?.GetComponent<TMP_Text>();
            if (countText != null && countText.gameObject.activeSelf)
                countText.text = DecisionRecord.Instance.ManzanasAvailable.ToString();
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

            var countText = go.transform.Find("Count")?.GetComponent<TMP_Text>();
            if (countText != null)
            {
                bool isApple = item == InventorySystem.ItemType.Manzana;
                countText.gameObject.SetActive(isApple);
            }

            var button = go.GetComponent<Button>();
            InventorySystem.ItemType capturedItem = item;
            if (button != null)
                button.onClick.AddListener(() => InventorySystem.Instance.TryEquip(capturedItem));
        }
    }
}