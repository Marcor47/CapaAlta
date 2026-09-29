using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    public enum ItemType { None, Manzana, Telefono }

    [Header("Ítems desbloqueados al inicio")]
    public bool telefonoDesbloqueadoDesdeElInicio = true; // la madre lo entrega en Cap1

    [Header("Sprites por ítem (para la UI y la mano de Theo)")]
    public Sprite manzanaSprite;
    public Sprite telefonoSprite;

    private static readonly ItemType[] itemOrder = { ItemType.Manzana, ItemType.Telefono };


    private HashSet<ItemType> unlockedItems = new HashSet<ItemType>();
    private ItemType equippedItem = ItemType.None;
    private TheoController theo;

    public event Action<ItemType> OnEquippedChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (telefonoDesbloqueadoDesdeElInicio)
            unlockedItems.Add(ItemType.Telefono);
    }

    void Start()
    {
        theo = FindAnyObjectByType<TheoController>();
    }

    void Update()
    {
        if (theo != null && theo.IsInDialogue) return;
        if (Keyboard.current.qKey.wasPressedThisFrame) TryEquip(ItemType.None);
    }

    public void UnlockItem(ItemType item)
    {
        if (item == ItemType.None) return;
        unlockedItems.Add(item);
    }



    public System.Collections.Generic.List<ItemType> GetUnlockedItemsOrdered()
    {
        var list = new System.Collections.Generic.List<ItemType>();
        foreach (var item in itemOrder)
            if (unlockedItems.Contains(item)) list.Add(item);
        return list;
    }

    public Sprite GetSpriteFor(ItemType item)
    {
        switch (item)
        {
            case ItemType.Manzana: return manzanaSprite;
            case ItemType.Telefono: return telefonoSprite;
            default: return null;
        }
    }




    public bool IsUnlocked(ItemType item) => item == ItemType.None || unlockedItems.Contains(item);

    public void TryEquip(ItemType item)
    {
        Debug.Log("Intentando equipar: " + item);
        if (!IsUnlocked(item))
        {
            Debug.Log("El objeto NO está desbloqueado: " + item);
            return;
        }


        Debug.Log("Objeto equipado: " + equippedItem);

        equippedItem = item;
        SyncToTheo();
        OnEquippedChanged?.Invoke(equippedItem);
    }

    void SyncToTheo()
    {
        if (theo == null) theo = FindAnyObjectByType<TheoController>();
        if (theo != null) theo.phoneEquipped = (equippedItem == ItemType.Telefono);
    }

    public ItemType Equipped => equippedItem;
    public bool IsManzanaEquipped => equippedItem == ItemType.Manzana;
}