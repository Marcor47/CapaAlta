using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupDashboardUI : MonoBehaviour
{
    [Header("Skin")]
    public DashboardSkin skin;

    [Header("Cabecera y pestañas (orden: RESUMEN, CAP.1-4, HSE)")]
    public TextMeshProUGUI headerText;
    public Button[] tabButtons;
    public TextMeshProUGUI[] tabLabels;

    [Header("Contenido")]
    public ScrollRect scroll;
    public RectTransform content;

    [Header("HSE (opcional: usa la lista del panel epistolar)")]
    public EpistolaryResponsePanel responsePanel;

    List<UserAccount> students = new List<UserAccount>();
    bool wired;

    void Wire()
    {
        if (wired) return;
        wired = true;
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int idx = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(idx));
        }
    }

    public void Open(List<UserAccount> list)
    {
        Wire();
        students = list;
        headerText.text = $"GRUPO - {students.Count} {(students.Count == 1 ? "ALUMNO" : "ALUMNOS")}";
        ShowTab(0);
    }

    void ShowTab(int tab)
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            bool on = i == tab;
            tabButtons[i].image.sprite = on ? skin.tabOn : skin.tabOff;
            tabLabels[i].color = DashUI.Ink;
            tabLabels[i].fontStyle = on ? FontStyles.Bold : FontStyles.Normal;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var c = content.GetChild(i);
            c.SetParent(null);
            Destroy(c.gameObject);
        }

        if (students.Count == 0) DashUI.Note(skin, content, "No hay alumnos registrados todavía.");
        else if (tab == 0) BuildSummary();
        else if (tab <= 4) BuildChapter(tab);
        else BuildHSE();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;
    }

    // ─── datos ────────────────────────────────────────────────
    List<CardData> AllCards() =>
        CardDatabase.Instance == null ? new List<CardData>()
        : CardDatabase.Instance.allCards.Where(c => c != null)
            .OrderBy(c => c.chapter).ThenBy(c => c.cardType).ThenBy(c => c.cardID).ToList();

    List<string> HseNames() =>
        responsePanel != null && responsePanel.hseNames.Count > 0
            ? responsePanel.hseNames : StudentDashboardUI.HseFallback.ToList();

    static List<int> RoundsOf(UserAccount s, string nodeID) =>
        s.progress.choices.Where(x => x.nodeID == nodeID)
            .GroupBy(x => x.roundIndex).OrderBy(g => g.Key)
            .Select(g => g.Last().relationshipDelta).ToList();

    bool ChapterUnlocked(int c) => CourseSettings.Instance != null && CourseSettings.Instance.IsChapterUnlocked(c);

    void CountRow(string label, int count, int total, float labelW = 140f)
    {
        var row = DashUI.Row(content, "CountRow", 30);
        DashUI.Text(skin, row, label, 12, 0, labelW, 30, 13, DashUI.Ink);
        DashUI.Meter(skin, row, 12 + labelW, 7.5f, 340, total > 0 ? (float)count / total : 0f);
        DashUI.Text(skin, row, $"{count}/{total}", 500, 0, 84, 30, 14, DashUI.Ink,
                    TextAlignmentOptions.MidlineRight, FontStyles.Bold);
    }

    // ─── RESUMEN ──────────────────────────────────────────────
    void BuildSummary()
    {
        int n = students.Count;

        DashUI.Title(skin, content, "ESTADO DE LOS ALUMNOS");
        int alDia = students.Count(s => StudentStatus.IsAlDia(s.progress));
        var st = DashUI.Row(content, "State", 36);
        DashUI.Text(skin, st, "AL DÍA / PEND.", 12, 0, 150, 36, 13, DashUI.Ink);
        DashUI.StackedBar(skin, st, 170, 7, 410, 22, new[] { alDia, n - alDia },
                          new[] { DashUI.Green, DashUI.Orange });
        DashUI.Note(skin, content, "Verde = al día · Naranja = pendientes", 18);

        DashUI.Title(skin, content, "ALUMNOS QUE COMPLETARON CADA CAPÍTULO");
        for (int c = 1; c <= StudentStatus.TotalChapters; c++)
        {
            int done = students.Count(s => s.progress.chaptersCompleted.Contains(c));
            if (!ChapterUnlocked(c) && done == 0)
            {
                var row = DashUI.Row(content, "ChLocked", 30);
                DashUI.Text(skin, row, $"CAPÍTULO {c}", 12, 0, 140, 30, 13, DashUI.Ink);
                DashUI.Tag(skin, row, TagKind.Cream, "BLOQUEADO", 120, 1.5f);
            }
            else CountRow($"CAPÍTULO {c}", done, n);
        }

        var cards = AllCards();
        int possible = n * cards.Count;
        int collected = students.Sum(s => cards.Count(cd => s.progress.allTimeCollectedCards.Any(x => x.cardID == cd.cardID)));
        int answered = students.Sum(s => cards.Count(cd => s.progress.epistolaryResponses.Any(x => x.cardID == cd.cardID)));
        DashUI.Title(skin, content, "CARTAS DEL GRUPO");
        CountRow("RECOGIDAS", collected, possible);
        CountRow("RESPONDIDAS", answered, possible);

        DashUI.Title(skin, content, "RESPUESTAS EN LOS DIÁLOGOS");
        var all = students.SelectMany(s => StudentStatus.EffectiveChoices(s.progress)).ToList();
        DashUI.Donut(skin, content,
            all.Count(x => x.relationshipDelta > 0),
            all.Count(x => x.relationshipDelta == 0),
            all.Count(x => x.relationshipDelta < 0));

        DashUI.Title(skin, content, "RELACIÓN CON PERSONAJES");
        for (int i = 0; i < StudentDashboardUI.npcDisplay.GetLength(0); i++)
        {
            string key = StudentDashboardUI.npcDisplay[i, 0];
            int good = 0, neu = 0, bad = 0;
            foreach (var s in students)
            {
                int v = StudentStatus.RelationOf(s.progress, key);
                if (v == StudentStatus.NoEncounter) continue;
                if (v > 0) good++; else if (v < 0) bad++; else neu++;
            }
            var row = DashUI.Row(content, "Relation", 36);
            DashUI.Text(skin, row, StudentDashboardUI.npcDisplay[i, 1], 12, 0, 150, 36, 13, DashUI.Ink);
            DashUI.StackedBar(skin, row, 170, 7, 410, 22, new[] { good, neu, bad },
                              new[] { DashUI.Green, DashUI.Yellow, DashUI.Red });
            if (good + neu + bad == 0)
                DashUI.Text(skin, row, "SIN ENCUENTROS", 180, 7, 300, 22, 11, DashUI.Ink);
        }
        DashUI.Note(skin, content,
            "Cuenta solo a los alumnos que ya tuvieron el encuentro. Verde = buena · Amarillo = neutral · Rojo = mala.", 30);
    }

    // ─── CAPÍTULO ─────────────────────────────────────────────
    void BuildChapter(int c)
    {
        int n = students.Count;
        int done = 0, prog = 0, pend = 0;
        foreach (var s in students)
        {
            var st = StudentStatus.GetChapterState(s.progress, c);
            if (st == ChapterState.Completo) done++;
            else if (st == ChapterState.EnCurso) prog++;
            else if (st == ChapterState.Pendiente) pend++;
        }

        DashUI.Title(skin, content, "ESTADO DEL CAPÍTULO");
        var hdr = DashUI.Row(content, "ChapterState", 36);
        if (!ChapterUnlocked(c) && done == 0 && prog == 0)
        {
            DashUI.Img(hdr, skin.dotLocked, 12, 7.5f, 21, 21);
            DashUI.Text(skin, hdr, $"CAPÍTULO {c} - BLOQUEADO PARA EL GRUPO", 45, 0, 500, 36, 14, DashUI.Ink,
                        TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        }
        else
        {
            DashUI.Text(skin, hdr, $"CAPÍTULO {c}", 12, 0, 150, 36, 13, DashUI.Ink);
            DashUI.StackedBar(skin, hdr, 170, 7, 410, 22, new[] { done, prog, pend },
                              new[] { DashUI.Green, DashUI.Yellow, DashUI.Orange });
            DashUI.Note(skin, content, "Verde = completado · Amarillo = en curso · Naranja = pendiente", 18);
        }

        DashUI.Title(skin, content, "PERSONAJES");
        foreach (var e in StudentDashboardUI.Encounters.Where(x => x.chapter == c))
        {
            int good = 0, neu = 0, bad = 0;
            foreach (var s in students)
            {
                var r = RoundsOf(s, e.nodeID);
                if (r.Count == 0) continue;
                int sum = r.Sum();
                if (sum > 0) good++; else if (sum < 0) bad++; else neu++;
            }
            var row = DashUI.Row(content, "Encounter", 36);
            DashUI.Text(skin, row, $"{e.npcName} - ENCUENTRO {e.num}", 12, 0, 240, 36, 13, DashUI.Ink);
            DashUI.StackedBar(skin, row, 260, 7, 320, 22, new[] { good, neu, bad },
                              new[] { DashUI.Green, DashUI.Yellow, DashUI.Red });
            if (good + neu + bad == 0)
                DashUI.Text(skin, row, "SIN ENCUENTROS", 270, 7, 300, 22, 11, DashUI.Ink);
        }

        DashUI.Title(skin, content, "CARTAS");
        var cards = AllCards();
        var inChapter = cards.Where(x => x.chapter == c).ToList();
        if (inChapter.Count == 0)
        {
            DashUI.Note(skin, content, "No hay cartas asignadas a este capítulo.");
            return;
        }

        var hse = HseNames();
        foreach (var card in inChapter)
        {
            int num = cards.Where(x => x.authorName == card.authorName).ToList().IndexOf(card) + 1;
            int collectedBy = students.Count(s => s.progress.allTimeCollectedCards.Any(x => x.cardID == card.cardID));
            var resp = students
                .Select(s => s.progress.epistolaryResponses.Find(r => r.cardID == card.cardID))
                .Where(r => r != null).ToList();

            var top = hse.Select(h => (name: h, count: resp.Count(r => r.linkedHSE.Contains(h))))
                         .Where(t => t.count > 0).OrderByDescending(t => t.count).Take(2).ToList();

            var row = DashUI.Row(content, "Card", resp.Count > 0 ? 60 : 36);
            DashUI.Text(skin, row, $"{card.authorName.ToUpperInvariant()} - CARTA {num}", 12, 0, 260, 36, 13, DashUI.Ink);
            string right = collectedBy == 0 ? "NINGUNO LA RECOGIÓ" : $"RESPONDIERON {resp.Count} DE {collectedBy}";
            DashUI.Text(skin, row, right, 280, 0, 300, 36, 11, DashUI.Ink, TextAlignmentOptions.MidlineRight);

            float x = 12f;
            foreach (var t in top)
                x += DashUI.Chip(skin, row, $"{t.name} ({t.count})", x, 34, 22, 11) + 6f;
        }
        DashUI.Note(skin, content, "Bajo cada carta: las 2 HSE que más alumnos relacionaron con ella (entre paréntesis, cuántos).", 30);
    }

    // ─── HSE ──────────────────────────────────────────────────
    void BuildHSE()
    {
        var withResp = students.Where(s => s.progress.epistolaryResponses.Count > 0).ToList();
        int k = withResp.Count;

        DashUI.Title(skin, content, "ALUMNOS QUE RELACIONARON CADA HSE CON SUS CARTAS", 22);
        var sub = DashUI.Block(content, "Subtitle", 18);
        DashUI.Text(skin, sub, $"{k} {(k == 1 ? "ALUMNO CON RESPUESTAS" : "ALUMNOS CON RESPUESTAS")}", 0, 0, DashUI.W, 18, 12, DashUI.Ink);

        foreach (var h in HseNames())
        {
            int count = withResp.Count(s => s.progress.epistolaryResponses.Any(r => r.linkedHSE.Contains(h)));
            var row = DashUI.Row(content, "HSE", 30);
            DashUI.Text(skin, row, h, 12, 0, 280, 30, 13, DashUI.Ink);
            DashUI.Meter(skin, row, 296, 7.5f, 220, k > 0 ? (float)count / k : 0f);
            DashUI.Text(skin, row, $"{count}/{k}", 524, 0, 60, 30, 14, DashUI.Ink,
                        TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        }

        DashUI.Note(skin, content,
            "Cada barra cuenta cuántos alumnos relacionaron esa habilidad con alguna de sus cartas. No mide el nivel de la habilidad.", 32);
    }
}