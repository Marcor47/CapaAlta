using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompendiumCardsPanel : MonoBehaviour
{
    [Header("Referencias")]
    public Transform groupsContainer;   // Content del Scroll View
    public GameObject groupPrefab;      // CardGroup: hijos "Header" (TMP) y "CardsRow"
    public GameObject cardSlotPrefab;   // Button + "Icon" + "QuestionMarkText" + "PendingBadge"
    public EpistolaryResponsePanel responsePanel;

    private LoginSceneController loginController;

    void Start()
    {
        loginController = FindAnyObjectByType<LoginSceneController>();
    }

    void OnEnable()
    {
        Populate();
    }

    void Populate()
    {
        ClearChildren(groupsContainer);

        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;
        var progress = AccountManager.Instance.CurrentUser.progress;

        foreach (var author in CardGroupData.SecondaryGroupsInOrder)
            BuildGroup(author, CardGroupData.SecondaryGroupTotals[author], CardsOfGroup(author, false), progress);

        BuildGroup(CardGroupData.FatherDisplayName, CardGroupData.FatherTotalCards, CardsOfGroup(null, true), progress);
    }

    // Separa el hijo antes de destruirlo, para que el layout no vea objetos "fantasma"
    void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    List<CardData> CardsOfGroup(string author, bool father)
    {
        if (CardDatabase.Instance == null) return new List<CardData>();

        return CardDatabase.Instance.allCards
            .Where(c => c != null && (father
                ? c.cardType == CardData.CardType.Father
                : c.cardType == CardData.CardType.Secondary && c.authorName == author))
            .OrderBy(c => c.chapter).ThenBy(c => c.cardID)
            .ToList();
    }

    void BuildGroup(string authorName, int expectedTotal, List<CardData> groupCards, StudentProgress progress)
    {
        int slotCount = Mathf.Max(expectedTotal, groupCards.Count);
        int collectedCount = groupCards.Count(c => progress.allTimeCollectedCards.Exists(r => r.cardID == c.cardID));

        GameObject group = Instantiate(groupPrefab, groupsContainer);
        var header = group.transform.Find("Header")?.GetComponent<TextMeshProUGUI>();
        var row = group.transform.Find("CardsRow");
        if (row == null) return;

        string displayName = collectedCount > 0 ? authorName : "?";
        if (header != null) header.text = $"{displayName}  ({collectedCount}/{slotCount})";

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(cardSlotPrefab, row);
            var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            var questionMark = slot.transform.Find("QuestionMarkText")?.gameObject;
            var badge = slot.transform.Find("PendingBadge")?.gameObject;
            var button = slot.GetComponent<Button>();

            CardData card = i < groupCards.Count ? groupCards[i] : null;
            bool collected = card != null && progress.allTimeCollectedCards.Exists(r => r.cardID == card.cardID);
            bool answered = collected && progress.epistolaryResponses.Exists(r => r.cardID == card.cardID);

            if (icon != null) { icon.enabled = collected; if (collected) icon.sprite = card.cardSprite; }
            if (questionMark != null) questionMark.SetActive(!collected);
            if (badge != null) badge.SetActive(collected && !answered);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.interactable = collected;
                if (collected)
                {
                    string capturedID = card.cardID;
                    button.onClick.AddListener(() => OpenCard(capturedID));
                }
            }
        }
    }

    void OpenCard(string cardID)
    {
        var cardData = CardDatabase.Instance?.GetByID(cardID);
        if (cardData == null || responsePanel == null) return;

        if (loginController != null) loginController.OpenPanel(responsePanel.gameObject);
        responsePanel.OpenFor(cardID, cardData.authorName, cardData.cardText, cardData.cardSprite);
    }
}