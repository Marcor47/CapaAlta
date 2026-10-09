using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashboardHSEChart : MonoBehaviour
{
    [Header("Referencias")]
    public Transform barsContainer; // Vertical Layout Group + Content Size Fitter
    public GameObject barRowPrefab; // hijo "Label" (TMP) + hijo "BarFill" (Image, Type=Filled, Horizontal)

    public void Populate(UserAccount student)
    {
        foreach (Transform child in barsContainer) Destroy(child.gameObject);
        Canvas.ForceUpdateCanvases();

        if (EpistolaryResponsePanel.Instance == null) return;
        var hseNames = EpistolaryResponsePanel.Instance.hseNames;

        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var hse in hseNames) counts[hse] = 0;

        foreach (var response in student.progress.epistolaryResponses)
            foreach (var hse in response.linkedHSE)
                if (counts.ContainsKey(hse)) counts[hse]++;

        int maxCount = counts.Values.Count > 0 ? Mathf.Max(1, counts.Values.Max()) : 1;

        foreach (var hse in hseNames)
        {
            GameObject row = Instantiate(barRowPrefab, barsContainer);

            var label = row.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = $"{hse} ({counts[hse]})";

            var barFill = row.transform.Find("BarFill")?.GetComponent<Image>();
            if (barFill != null) barFill.fillAmount = (float)counts[hse] / maxCount;
        }
    }
}