using System.Collections.Generic;
using UnityEngine;

public class DecisionRecord : MonoBehaviour
{
    // ─── SINGLETON ─────────────────────────────────────────────
    public static DecisionRecord Instance { get; private set; }

    // ─── RELACIONES CON NPCs (-1, 0, +1) ──────────────────────
    // clave: npcID ("gaz", "ryland_astro", "haru", etc.)
    // valor: -1 mala / 0 neutral / +1 buena
    private Dictionary<string, int> npcRelationships = new Dictionary<string, int>();

    // ─── ENCUENTROS COMPLETADOS ────────────────────────────────
    // registra qué nodeIDs ya fueron completados
    private HashSet<string> completedNodes = new HashSet<string>();

    // ─── MANZANAS ──────────────────────────────────────────────
    private int manzanasAvailable = 0;
    private HashSet<string> manzanasUsedOn = new HashSet<string>();
    // registra a qué NPCs ya se les dio una manzana (1 por NPC)

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── RELACIONES ────────────────────────────────────────────
    public int GetRelationship(string npcID)
    {
        return npcRelationships.ContainsKey(npcID)
            ? npcRelationships[npcID]
            : 0; // neutral por defecto
    }

    public void SetRelationship(string npcID, int delta)
    {
        int current = GetRelationship(npcID);
        int newValue = Mathf.Clamp(current + delta, -1, 1);
        npcRelationships[npcID] = newValue;

        Debug.Log($"[DecisionRecord] {npcID}: {current} → {newValue}");
    }

    // ─── ENCUENTROS ────────────────────────────────────────────
    public bool IsNodeCompleted(string nodeID)
    {
        return completedNodes.Contains(nodeID);
    }

    public void MarkNodeCompleted(string nodeID)
    {
        completedNodes.Add(nodeID);
        Debug.Log($"[DecisionRecord] Encuentro completado: {nodeID}");
    }

    // Verifica si el encuentro anterior de un NPC fue completado
    // Usa esto para saber si el NPC aparece en el siguiente encuentro
    public bool CanAppear(string nodeID)
    {
        return completedNodes.Contains(nodeID);
    }

    // ─── MANZANAS ──────────────────────────────────────────────
    public int ManzanasAvailable => manzanasAvailable;

    public void AddManzana()
    {
        manzanasAvailable++;
        Debug.Log($"[DecisionRecord] Manzanas disponibles: {manzanasAvailable}");
    }

    // Intenta usar una manzana con un NPC
    // Retorna false si: no hay manzanas, ya se usó con ese NPC,
    //                   o el encuentro no está completado (regla: solo después del diálogo)
    public bool TryUseManzana(string npcID, string completedNodeID)
    {
        if (manzanasAvailable <= 0)
        {
            Debug.Log("[DecisionRecord] Sin manzanas disponibles.");
            return false;
        }

        if (!completedNodes.Contains(completedNodeID))
        {
            Debug.Log("[DecisionRecord] Debes terminar el diálogo primero.");
            return false;
        }

        if (manzanasUsedOn.Contains(npcID))
        {
            Debug.Log($"[DecisionRecord] Ya usaste una manzana con {npcID}.");
            return false;
        }

        // Aplica la manzana: sube 1 nivel la relación
        int current = GetRelationship(npcID);
        if (current >= 1)
        {
            Debug.Log($"[DecisionRecord] Relación con {npcID} ya está al máximo.");
            return false;
        }

        manzanasAvailable--;
        manzanasUsedOn.Add(npcID);
        SetRelationship(npcID, +1);

        Debug.Log($"[DecisionRecord] Manzana usada con {npcID}. Manzanas restantes: {manzanasAvailable}");
        return true;
    }

    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
    // Útil para el módulo docente (M10)
    public Dictionary<string, int> GetAllRelationships()
        => new Dictionary<string, int>(npcRelationships);

    public HashSet<string> GetCompletedNodes()
        => new HashSet<string>(completedNodes);

    // ─── DEBUG ─────────────────────────────────────────────────
    [ContextMenu("Listar estado actual")]
    void DebugListState()
    {
        Debug.Log("=== RELACIONES ===");
        foreach (var kvp in npcRelationships)
            Debug.Log($"  {kvp.Key}: {kvp.Value}");

        Debug.Log("=== ENCUENTROS COMPLETADOS ===");
        foreach (var node in completedNodes)
            Debug.Log($"  {node}");

        Debug.Log($"=== MANZANAS: {manzanasAvailable} disponibles ===");
    }
}
