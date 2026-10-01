using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompendiumCardsPanel : MonoBehaviour
{
    [Header("Referencias")]
    public Transform groupsContainer; // dentro de un Scroll View, con Vertical Layout Group + Content Size Fitter
    public GameObject groupHeaderPrefab; // TextMeshProUGUI
    public GameObject cardSlotPrefab;    // Button + "Icon" (Image) + "QuestionMarkText" (TMP) + "PendingBadge" (GameObject)
    public EpistolaryResponsePanel responsePanel;

    void OnEnable()
    {
        Populate();
    }

    void Populate()
    {
        foreach (Transform child in groupsContainer) Destroy(child.gameObject);
        Canvas.ForceUpdateCanvases();

        var progress = AccountManager.Instance.CurrentUser.progress;

        foreach (var authorName in CardGroupData.SecondaryGroupsInOrder)
        {
            int total = CardGroupData.SecondaryGroupTotals[authorName];
            var collected = progress.allTimeCollectedCards.Where(c => c.authorName == authorName).ToList();
            BuildGroup(collected.Count > 0 ? authorName : "?", total, collected, progress);
        }

        var fatherCards = progress.allTimeCollectedCards.Where(IsFatherCard).ToList();
        BuildGroup(fatherCards.Count > 0 ? CardGroupData.FatherDisplayName : "?", CardGroupData.FatherTotalCards, fatherCards, progress);
    }

    bool IsFatherCard(CollectedCardRecord record)
    {
        var data = CardDatabase.Instance?.GetByID(record.cardID);
        return data != null && data.cardType == CardData.CardType.Father;
    }

    void BuildGroup(string displayName, int total, List<CollectedCardRecord> collected, StudentProgress progress)
    {
        GameObject header = Instantiate(groupHeaderPrefab, groupsContainer);
        var headerText = header.GetComponentInChildren<TextMeshProUGUI>();
        if (headerText != null) headerText.text = displayName;

        GameObject rowGO = new GameObject("CardsRow", typeof(RectTransform));
        rowGO.transform.SetParent(groupsContainer, false);
        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10; hlg.childControlWidth = false; hlg.childControlHeight = false;
        var csf = rowGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < total; i++)
        {
            GameObject slot = Instantiate(cardSlotPrefab, rowGO.transform);
            var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            var questionMark = slot.transform.Find("QuestionMarkText")?.gameObject;
            var badge = slot.transform.Find("PendingBadge")?.gameObject;
            var button = slot.GetComponent<Button>();

            if (i < collected.Count)
            {
                var record = collected[i];
                var cardData = CardDatabase.Instance?.GetByID(record.cardID);

                if (icon != null) { icon.enabled = true; icon.sprite = cardData != null ? cardData.cardSprite : null; }
                if (questionMark != null) questionMark.SetActive(false);

                bool answered = progress.epistolaryResponses.Exists(r => r.cardID == record.cardID);
                if (badge != null) badge.SetActive(!answered);

                string capturedID = record.cardID;
                if (button != null) { button.interactable = true; button.onClick.AddListener(() => OpenCard(capturedID)); }
            }
            else
            {
                if (icon != null) icon.enabled = false;
                if (questionMark != null) questionMark.SetActive(true);
                if (badge != null) badge.SetActive(false);
                if (button != null) button.interactable = false;
            }
        }
    }

    void OpenCard(string cardID)
    {
        var cardData = CardDatabase.Instance?.GetByID(cardID);
        if (cardData == null || responsePanel == null) return;
        responsePanel.OpenFor(cardID, cardData.cardText, cardData.cardSprite);
    }
}