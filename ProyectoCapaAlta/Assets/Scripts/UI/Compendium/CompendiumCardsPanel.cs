using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompendiumCardsPanel : MonoBehaviour
{
    [Header("Estilo")]
    public DashboardSkin skin;
    public CompendiumSkin comp;

    [Header("Referencias")]
    public TextMeshProUGUI headerText;
    public ScrollRect scroll;
    public RectTransform content;
    public EpistolaryResponsePanel responsePanel;

    const float ColX = 300f, PairH = 106f;
    const float SlotW = 48f, SlotH = 69f, SlotPitch = 60f, SlotY = 30f;

    LoginSceneController loginController;

    class Group { public string name; public int total; public List<CardData> cards; }

    void Start() { loginController = FindAnyObjectByType<LoginSceneController>(); }
    void OnEnable() { Populate(); }

    void Populate()
    {
        if (headerText != null) headerText.text = "CARTAS";
        ClearChildren(content);
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;
        var progress = AccountManager.Instance.CurrentUser.progress;

        var groups = new List<Group>();
        foreach (var author in CardGroupData.SecondaryGroupsInOrder)
            groups.Add(new Group { name = author, total = CardGroupData.SecondaryGroupTotals[author], cards = CardsOfGroup(author, false) });
        groups.Add(new Group { name = CardGroupData.FatherDisplayName, total = CardGroupData.FatherTotalCards, cards = CardsOfGroup(null, true) });

        for (int i = 0; i < groups.Count; i += 2)
        {
            var pair = DashUI.Block(content, "Pair", PairH);
            BuildGroup(pair, 0f, groups[i], progress);
            if (i + 1 < groups.Count) BuildGroup(pair, ColX, groups[i + 1], progress);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (scroll != null) scroll.verticalNormalizedPosition = 1f;
    }

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

    void BuildGroup(RectTransform pair, float x, Group g, StudentProgress progress)
    {
        int slotCount = Mathf.Max(g.total, g.cards.Count);
        int collectedCount = g.cards.Count(c => progress.allTimeCollectedCards.Exists(r => r.cardID == c.cardID));

        string displayName = collectedCount > 0 ? g.name : "?";
        DashUI.Text(skin, pair, $"{displayName} ({collectedCount}/{slotCount})", x, 0, 290, 24, 16, DashUI.Ink);

        Sprite fullSprite = g.cards.Count > 0 && g.cards[0].cardType == CardData.CardType.Father ? comp.slotFather : comp.slotSecondary;

        for (int i = 0; i < slotCount; i++)
        {
            CardData card = i < g.cards.Count ? g.cards[i] : null;
            bool collected = card != null && progress.allTimeCollectedCards.Exists(r => r.cardID == card.cardID);
            bool answered = collected && progress.epistolaryResponses.Exists(r => r.cardID == card.cardID);

            var img = DashUI.Img(pair, collected ? fullSprite : comp.slotEmpty, x + i * SlotPitch, SlotY, SlotW, SlotH);
            img.type = Image.Type.Simple;

            if (!collected) continue;

            img.raycastTarget = true;
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var cb = button.colors;
            cb.highlightedColor = new Color(1f, 1f, 0.82f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = cb;
            string id = card.cardID;
            button.onClick.AddListener(() => OpenCard(id));

            if (!answered && comp.pendingBadge != null)
            {
                float bw = comp.pendingBadge.rect.width, bh = comp.pendingBadge.rect.height;
                var badge = DashUI.Img(img.transform, comp.pendingBadge, SlotW - bw + 8f, -4f, bw, bh);
                badge.type = Image.Type.Simple;
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