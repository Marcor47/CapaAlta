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
        if (current < 1)
        {
            SetRelationship(npcID, +1);
        }
        else
        {
            Debug.Log($"[DecisionRecord] Relación con {npcID} ya estaba al máximo; la manzana se usó igual (sin subir más).");
        }

        Debug.Log($"[DecisionRecord] Manzana usada con {npcID}. Manzanas restantes: {manzanasAvailable}");
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