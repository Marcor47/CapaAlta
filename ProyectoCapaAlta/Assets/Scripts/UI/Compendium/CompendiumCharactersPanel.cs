using TMPro;
using UnityEngine;

public class CompendiumCharactersPanel : MonoBehaviour
{
    public Transform entriesContainer; // Content del Scroll View
    public GameObject entryPrefab;     // hijos "Name" y "Description" (TMP)

    void OnEnable() => Populate();

    void Populate()
    {
        for (int i = entriesContainer.childCount - 1; i >= 0; i--)
        {
            var child = entriesContainer.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;

        foreach (var entry in CharacterData.Entries)
        {
            bool known = entry.alwaysKnown || AccountManager.Instance.HasMetNpc(entry.npcID);

            GameObject go = Instantiate(entryPrefab, entriesContainer);
            var nameText = go.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            var descText = go.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null) nameText.text = known ? entry.displayName : "?";
            if (descText != null) descText.text = known ? entry.description : "?";
        }
    }
}