using System.Collections.Generic;
using UnityEngine;

public class DecisionRecord : MonoBehaviour
{
    // ─── SINGLETON ─────────────────────────────────────────────
    public static DecisionRecord Instance { get; private set; }

    // ─── RELACIONES CON NPCs (-1, 0, +1) ──────────────────────
    private Dictionary<string, int> npcRelationships = new Dictionary<string, int>();

    // ─── ENCUENTROS COMPLETADOS ────────────────────────────────
    private HashSet<string> completedNodes = new HashSet<string>();

    // ─── MANZANAS ──────────────────────────────────────────────
    private int manzanasAvailable = 0;
    private HashSet<string> manzanasUsedOn = new HashSet<string>();

    // ─── HISTORIAL DE ELECCIONES (para M10) ────────────────────
    private List<ChoiceRecord> choiceHistory = new List<ChoiceRecord>();

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromAccount(); // NUEVO
    }

    public void LoadFromAccount()
    {
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;
        var p = AccountManager.Instance.CurrentUser.progress;

        npcRelationships.Clear();
        for (int i = 0; i < p.npcRelationshipKeys.Count; i++)
            npcRelationships[p.npcRelationshipKeys[i]] = p.npcRelationshipValues[i];

        completedNodes = new HashSet<string>(p.completedNodesSaved);
        manzanasAvailable = p.manzanasAvailable;
        manzanasUsedOn = new HashSet<string>(p.manzanasUsedOnSaved);
    }


    public void SaveToAccount()
    {
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;
        var p = AccountManager.Instance.CurrentUser.progress;

        p.npcRelationshipKeys = new List<string>(npcRelationships.Keys);
        p.npcRelationshipValues = new List<int>(npcRelationships.Values);
        p.completedNodesSaved = new List<string>(completedNodes);
        p.manzanasAvailable = manzanasAvailable;
        p.manzanasUsedOnSaved = new List<string>(manzanasUsedOn);

        AccountManager.Instance.SaveProgress();
    }

    public void ResetState()
    {
        npcRelationships.Clear();
        completedNodes.Clear();
        manzanasAvailable = 0;
        manzanasUsedOn.Clear();
        choiceHistory.Clear();
    }

    // ─── RELACIONES ────────────────────────────────────────────
    public int GetRelationship(string npcID)
    {
        return npcRelationships.ContainsKey(npcID)
            ? npcRelationships[npcID]
            : 0;
    }

    public void SetRelationship(string npcID, int delta)
    {
        int current = GetRelationship(npcID);
        int newValue = Mathf.Clamp(current + delta, -1, 1);
        npcRelationships[npcID] = newValue;

        Debug.Log($"[DecisionRecord] {npcID}: {current} → {newValue}");
        SaveToAccount();
    }

    // Relación con la que el jugador LLEGA a un encuentro (solo cuentan encuentros anteriores).
    public int GetRelationshipBefore(string nodeID, string npcID)
    {
        var user = AccountManager.Instance != null ? AccountManager.Instance.CurrentUser : null;
        if (user != null && RelationCalculator.HasEncounter(user.progress, npcID))
        {
            SaveToAccount(); // vuelca los encuentros completados en memoria al progreso
            return RelationCalculator.Compute(user.progress, npcID, nodeID);
        }
        return GetRelationship(npcID);
    }

    // Relación justo después de un encuentro (sin contar los encuentros posteriores).
    // Se usa para elegir la respuesta del NPC al recibir la manzana en ese encuentro.
    public int GetRelationshipAfter(string nodeID, string npcID)
    {
        var user = AccountManager.Instance != null ? AccountManager.Instance.CurrentUser : null;
        if (user != null && RelationCalculator.HasEncounter(user.progress, npcID))
        {
            SaveToAccount();
            return RelationCalculator.ComputeThrough(user.progress, npcID, nodeID);
        }
        return GetRelationship(npcID);
    }

    // Recalcula TODAS las relaciones desde las decisiones y manzanas guardadas
    // (misma cuenta que usan los paneles del profesor).
    public void RecomputeRelations()
    {
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;
        SaveToAccount();
        RelationCalculator.RecomputeAll(AccountManager.Instance.CurrentUser.progress);
        LoadFromAccount();
        AccountManager.Instance.SaveProgress();
    }

    // Se llama al terminar la última ronda de un encuentro: lo marca completado y recalcula.
    // fallbackDelta solo se usa si no hay cuenta iniciada (pruebas directas en el editor).
    public void CompleteNode(string nodeID, string npcID, int fallbackDelta)
    {
        completedNodes.Add(nodeID);
        if (AccountManager.Instance != null && AccountManager.Instance.CurrentUser != null)
        {
            Debug.Log($"[DecisionRecord] Encuentro completado: {nodeID}");
            int before = GetRelationship(npcID);
            RecomputeRelations();
            Debug.Log($"[DecisionRecord] {npcID}: {before} → {GetRelationship(npcID)}");
        }
        else SetRelationship(npcID, fallbackDelta);
    }

    public void ForceRelationship(string npcID, int value)
    {
        npcRelationships[npcID] = Mathf.Clamp(value, -1, 1);
        Debug.Log($"[DecisionRecord] {npcID} fijado en {npcRelationships[npcID]}");
        SaveToAccount();
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
        SaveToAccount();
    }

    public bool CanAppear(string nodeID)
    {
        return completedNodes.Contains(nodeID);
    }

    // ─── MANZANAS ──────────────────────────────────────────────
    public int ManzanasAvailable => manzanasAvailable;

    // ¿Se puede entregar ahora una manzana a este personaje en este encuentro?
    // Mismas condiciones que TryUseManzana: así el "E" solo aparece cuando la entrega es posible.
    public bool CanGiveManzana(string npcID, string nodeID)
        => manzanasAvailable > 0
        && completedNodes.Contains(nodeID)
        && !manzanasUsedOn.Contains(npcID);

    public void AddManzana()
    {
        manzanasAvailable++;
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.UnlockItem(InventorySystem.ItemType.Manzana); // NUEVO
        Debug.Log($"[DecisionRecord] Manzanas disponibles: {manzanasAvailable}");
        SaveToAccount();
    }

    // Manzana entregada por un encuentro: una sola vez por nodo, aunque se rejuegue
    public void AddManzanaFromNode(string nodeID)
    {
        if (AccountManager.Instance != null && AccountManager.Instance.CurrentUser != null)
        {
            var given = AccountManager.Instance.CurrentUser.progress.manzanaNodesGiven;
            if (given.Contains(nodeID)) return;
            given.Add(nodeID);
        }
        AddManzana();
    }

    public bool WasManzanaGivenFor(string nodeID)
    {
        return AccountManager.Instance != null
            && AccountManager.Instance.CurrentUser != null
            && AccountManager.Instance.CurrentUser.progress.manzanaNodesGiven.Contains(nodeID);
    }

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

        manzanasAvailable--;
        manzanasUsedOn.Add(npcID);

        int current = GetRelationship(npcID);
        var user = AccountManager.Instance != null ? AccountManager.Instance.CurrentUser : null;
        if (user != null)
        {
            // La manzana queda registrada en ese encuentro ("recibió"): suma +1 en el cálculo único.
            if (!user.progress.manzanaReceivedNodes.Contains(completedNodeID))
                user.progress.manzanaReceivedNodes.Add(completedNodeID);
            RecomputeRelations();
            if (GetRelationship(npcID) == current)
                Debug.Log($"[DecisionRecord] Relación con {npcID} ya estaba al máximo; la manzana se usó igual (sin subir más).");
            else
                Debug.Log($"[DecisionRecord] {npcID}: {current} → {GetRelationship(npcID)} (manzana)");
        }
        else if (current < 1) SetRelationship(npcID, +1);

        Debug.Log($"[DecisionRecord] Manzana usada con {npcID}. Manzanas restantes: {manzanasAvailable}");
        SaveToAccount();
        return true;
    }

    public bool RemoveManzana()
    {
        if (manzanasAvailable <= 0) return false;
        manzanasAvailable--;
        SaveToAccount();
        return true;
    }

    // ─── HISTORIAL DE ELECCIONES ────────────────────────────────
    [System.Serializable]
    public class ChoiceRecord
    {
        public string nodeID;
        public string npcID;
        public int roundIndex;
        public int optionIndex;
        public int relationshipDelta;
    }

    // Dentro de RecordChoice(), agregar antes del Debug.Log final:
    public void RecordChoice(string nodeID, string npcID, int roundIndex, int optionIndex, int relationshipDelta)
    {
        choiceHistory.Add(new ChoiceRecord
        {
            nodeID = nodeID,
            npcID = npcID,
            roundIndex = roundIndex,
            optionIndex = optionIndex,
            relationshipDelta = relationshipDelta
        });

        if (AccountManager.Instance != null && AccountManager.Instance.CurrentUser != null)
        {
            AccountManager.Instance.CurrentUser.progress.decisionsCount++;

            AccountManager.Instance.CurrentUser.progress.choices.Add(new SavedChoice
            {
                nodeID = nodeID,
                npcID = npcID,
                roundIndex = roundIndex,
                optionIndex = optionIndex,
                relationshipDelta = relationshipDelta
            });

            AccountManager.Instance.SaveProgress();
        }

        Debug.Log($"[DecisionRecord] Elección registrada: {nodeID} ronda {roundIndex} → opción {optionIndex} (delta {relationshipDelta})");
    }

    public List<ChoiceRecord> GetChoiceHistory()
        => new List<ChoiceRecord>(choiceHistory);

    public List<ChoiceRecord> GetChoicesForNode(string nodeID)
        => choiceHistory.FindAll(c => c.nodeID == nodeID);

    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
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

        Debug.Log("=== HISTORIAL DE ELECCIONES ===");
        foreach (var c in choiceHistory)
            Debug.Log($"  {c.nodeID} ronda {c.roundIndex}: opción {c.optionIndex} (delta {c.relationshipDelta})");

        Debug.Log($"=== MANZANAS: {manzanasAvailable} disponibles ===");
    }
}