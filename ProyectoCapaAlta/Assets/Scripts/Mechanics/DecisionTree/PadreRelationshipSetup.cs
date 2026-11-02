using UnityEngine;

public class PadreRelationshipSetup : MonoBehaviour
{
    [Tooltip("cardID exactos (CardData.cardID) de las 4 cartas del padre")]
    public string[] fatherCardIDs;

    void Start()
    {
        if (CardInventory.Instance != null)
            CardInventory.Instance.OnCardAdded += OnCardAdded;
        Evaluate();
    }

    void OnDestroy()
    {
        if (CardInventory.Instance != null)
            CardInventory.Instance.OnCardAdded -= OnCardAdded;
    }

    void OnCardAdded(CardData _) => Evaluate();

    public void Evaluate()
    {
        if (DecisionRecord.Instance == null || fatherCardIDs == null || fatherCardIDs.Length == 0) return;

        int collected = 0;
        foreach (var id in fatherCardIDs)
            if (HasCard(id)) collected++;

        int value = collected >= fatherCardIDs.Length ? 1 : (collected > 0 ? 0 : -1);
        DecisionRecord.Instance.ForceRelationship("padre", value);
    }

    bool HasCard(string id)
    {
        if (CardInventory.Instance != null && CardInventory.Instance.IsCollected(id)) return true;

        var user = AccountManager.Instance != null ? AccountManager.Instance.CurrentUser : null;
        return user != null && user.progress.allTimeCollectedCards.Exists(c => c.cardID == id);
    }
}