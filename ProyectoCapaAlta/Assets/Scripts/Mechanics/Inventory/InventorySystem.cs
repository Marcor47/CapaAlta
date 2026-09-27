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
        if (theo != null && theo.IsInDialogue) return; // no cambiar de ítem en medio de un diálogo

        if (Keyboard.current.digit1Key.wasPressedThisFrame) TryEquip(ItemType.Manzana);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) TryEquip(ItemType.Telefono);
        if (Keyboard.current.digit0Key.wasPressedThisFrame) TryEquip(ItemType.None);
    }

    public void UnlockItem(ItemType item)
    {
        if (item == ItemType.None) return;
        unlockedItems.Add(item);
    }

    public bool IsUnlocked(ItemType item) => item == ItemType.None || unlockedItems.Contains(item);

    public void TryEquip(ItemType item)
    {
        if (!IsUnlocked(item)) return;

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