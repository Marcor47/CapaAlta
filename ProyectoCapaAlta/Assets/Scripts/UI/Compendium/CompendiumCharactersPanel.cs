using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompendiumCharactersPanel : MonoBehaviour
{
    public DashboardSkin skin;
    public TextMeshProUGUI headerText;
    public ScrollRect scroll;
    public RectTransform content;

    static readonly Color Dark = new Color32(0x2B, 0x2B, 0x3F, 255);

    void OnEnable() { Populate(); }

    void Populate()
    {
        if (headerText != null) headerText.text = "PERSONAJES";
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;

        foreach (var entry in CharacterData.Entries)
        {
            bool known = entry.alwaysKnown || AccountManager.Instance.HasMetNpc(entry.npcID);
            string desc = known ? entry.description : "AÚN NO LO CONOCES.";

            var row = DashUI.Row(content, "Entry", 60f);
            DashUI.Text(skin, row, known ? entry.displayName : "?", 12, 8, 570, 24, 16, DashUI.Ink);
            var d = DashUI.Text(skin, row, desc, 12, 36, 570, 20, 16, Dark,
                TextAlignmentOptions.TopLeft, FontStyles.Normal, true);

            float h = d.GetPreferredValues(desc, 570f, 0f).y;
            d.rectTransform.sizeDelta = new Vector2(570f, h);
            float total = 36f + h + 12f;
            var le = row.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = total;
            row.sizeDelta = new Vector2(row.sizeDelta.x, total);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (scroll != null) scroll.verticalNormalizedPosition = 1f;
    }
}