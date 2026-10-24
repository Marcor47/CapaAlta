using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    public enum ItemType { None, Manzana, Telefono, Onda, Gancho, Pinchos }

    [Header("Ítems desbloqueados al inicio")]
    public bool telefonoDesbloqueadoDesdeElInicio = true;

    [Header("Sprites por ítem (caja de inventario y mano de Theo)")]
    public Sprite manzanaSprite;
    public Sprite telefonoSprite;
    public Sprite ondaSprite;
    public Sprite ganchoSprite;
    public Sprite pinchosSprite;

    private static readonly ItemType[] itemOrder =
        { ItemType.Manzana, ItemType.Telefono, ItemType.Onda, ItemType.Gancho, ItemType.Pinchos };

    private readonly HashSet<ItemType> unlockedItems = new HashSet<ItemType>();
    private ItemType equippedItem = ItemType.None;
    private TheoController theo;

    public event Action<ItemType> OnEquippedChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RefreshFromAccount();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start() { RefreshTheo(); }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;
        RefreshFromAccount();
        RefreshTheo();
    }

    void RefreshTheo()
    {
        theo = FindAnyObjectByType<TheoController>();
        SyncToTheo();
    }

    void Update()
    {
        if (theo != null && theo.IsInDialogue) return;

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            TryEquip(ItemType.None);

        // Si lo equipado ya no está disponible (ej. se lanzó la última manzana), se desequipa solo
        if (equippedItem != ItemType.None && !IsUnlocked(equippedItem))
            TryEquip(ItemType.None);
    }

    // ─── DESBLOQUEOS ───────────────────────────────────────────
    public void RefreshFromAccount()
    {
        unlockedItems.Clear();
        if (telefonoDesbloqueadoDesdeElInicio) unlockedItems.Add(ItemType.Telefono);

        var account = AccountManager.Instance;
        if (account != null && account.CurrentUser != null)
        {
            foreach (var saved in account.CurrentUser.progress.unlockedItems)
                if (Enum.TryParse(saved, out ItemType parsed) && parsed != ItemType.None && parsed != ItemType.Manzana)
                    unlockedItems.Add(parsed);
        }

        if (!IsUnlocked(equippedItem)) TryEquip(ItemType.None);
    }

    // La manzana no se "desbloquea": aparece mientras el jugador tenga al menos una
    public void UnlockItem(ItemType item)
    {
        if (item == ItemType.None || item == ItemType.Manzana) return;
        if (unlockedItems.Add(item)) SaveToAccount();
    }

    void SaveToAccount()
    {
        var account = AccountManager.Instance;
        if (account == null || account.CurrentUser == null) return;

        account.CurrentUser.progress.unlockedItems = unlockedItems.Select(i => i.ToString()).ToList();
        account.SaveProgress();
    }

    public bool IsUnlocked(ItemType item)
    {
        switch (item)
        {
            case ItemType.None: return true;
            case ItemType.Manzana:
                return DecisionRecord.Instance != null && DecisionRecord.Instance.ManzanasAvailable > 0;
            default: return unlockedItems.Contains(item);
        }
    }

    public List<ItemType> GetUnlockedItemsOrdered()
    {
        var list = new List<ItemType>();
        foreach (var item in itemOrder)
            if (IsUnlocked(item)) list.Add(item);
        return list;
    }

    public Sprite GetSpriteFor(ItemType item)
    {
        switch (item)
        {
            case ItemType.Manzana: return manzanaSprite;
            case ItemType.Telefono: return telefonoSprite;
            case ItemType.Onda: return ondaSprite;
            case ItemType.Gancho: return ganchoSprite;
            case ItemType.Pinchos: return pinchosSprite;
            default: return null;
        }
    }

    // ─── EQUIPAR ───────────────────────────────────────────────
    public void TryEquip(ItemType item)
    {
        if (!IsUnlocked(item) || item == equippedItem) return;

        equippedItem = item;
        SyncToTheo();
        OnEquippedChanged?.Invoke(equippedItem);
    }

    void SyncToTheo()
    {
        if (theo != null) theo.phoneEquipped = (equippedItem == ItemType.Telefono);
    }

    // Se llama al cerrar sesión (después de Logout), para que el siguiente alumno no herede nada
    public void ResetState()
    {
        RefreshFromAccount();
        TryEquip(ItemType.None);
    }

    public ItemType Equipped => equippedItem;
    public bool IsManzanaEquipped => equippedItem == ItemType.Manzana;

    [ContextMenu("DEBUG: desbloquear Onda, Gancho y Pinchos")]
    void DebugUnlockTools()
    {
        UnlockItem(ItemType.Onda);
        UnlockItem(ItemType.Gancho);
        UnlockItem(ItemType.Pinchos);
    }
}