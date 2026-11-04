using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentDashboardUI : MonoBehaviour
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

    public struct Enc { public int chapter; public string nodeID; public string npcName; public int num; }

    // nodeID = el del asset DialogueNode.
    public static readonly Enc[] Encounters =
    {
        new Enc { chapter = 1, nodeID = "Cap1_Gaz_Enc1",         npcName = "GAZ",            num = 1 },
        new Enc { chapter = 1, nodeID = "Cap1_RylandAstro_Enc1", npcName = "RYLAND Y ASTRO", num = 1 },
        new Enc { chapter = 2, nodeID = "Cap2_Haru_Enc1",        npcName = "HARU",           num = 1 },
        new Enc { chapter = 2, nodeID = "Cap2_Gaz_Enc2",         npcName = "GAZ",            num = 2 },
        new Enc { chapter = 3, nodeID = "Cap3_LeeWilber_Enc1",   npcName = "LEE Y WILBER",   num = 1 },
        new Enc { chapter = 3, nodeID = "Cap3_Haru_Enc2",        npcName = "HARU",           num = 2 },
        new Enc { chapter = 4, nodeID = "Cap4_Gaz_Enc3",         npcName = "GAZ",            num = 3 },
        new Enc { chapter = 4, nodeID = "Cap4_Padre_Enc1",       npcName = "JASON",          num = 1 },
    };

    public static readonly string[,] npcDisplay =
    {
        { "gaz", "GAZ" }, { "haru", "HARU" }, { "lee_wilber", "LEE Y WILBER" },
        { "ryland_astro", "RYLAND Y ASTRO" }, { "padre", "JASON" }
    };

    public static readonly string[] HseFallback =
    {
        "Autoconcepto", "Autoestima", "Conciencia emocional", "Regulación emocional", "Autocuidado",
        "Toma de decisiones responsables", "Comunicación asertiva", "Trabajo en equipo", "Empatía",
        "Resolución de conflictos", "Conciencia social", "Comportamiento prosocial", "Creatividad"
    };

    UserAccount student;
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

    public void Open(UserAccount s)
    {
        Wire();
        student = s;
        headerText.text = s.username.ToUpperInvariant();
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

        if (tab == 0) BuildSummary();
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
        responsePanel != null && responsePanel.hseNames.Count > 0 ? responsePanel.hseNames : HseFallback.ToList();

    Sprite DotFor(ChapterState st) => st == ChapterState.Bloqueado ? skin.dotLocked
        : st == ChapterState.Pendiente ? skin.dotPending
        : st == ChapterState.EnCurso ? skin.dotInProgress : skin.dotDone;

    static string StateName(ChapterState st) => st == ChapterState.Bloqueado ? "BLOQUEADO"
        : st == ChapterState.Pendiente ? "PENDIENTE"
        : st == ChapterState.EnCurso ? "EN CURSO" : "COMPLETO";

    int RelationOf(string npcKey) => StudentStatus.RelationOf(student.progress, npcKey);

    // ─── RESUMEN ──────────────────────────────────────────────
    void BuildSummary()
    {
        var p = student.progress;

        DashUI.Title(skin, content, "CAPÍTULOS");
        var ch = DashUI.Row(content, "Chapters", 36);
        for (int c = 1; c <= StudentStatus.TotalChapters; c++)
        {
            float x = 12 + (c - 1) * 146;
            var st = StudentStatus.GetChapterState(p, c);
            DashUI.Img(ch, DotFor(st), x, 7.5f, 21, 21);
            DashUI.Text(skin, ch, $"CAP.{c} {StateName(st)}", x + 27, 0, 115, 36, 11, DashUI.Ink);
        }

        var cards = AllCards();
        var collected = new HashSet<string>(p.allTimeCollectedCards.Select(x => x.cardID));
        var answered = new HashSet<string>(p.epistolaryResponses.Select(x => x.cardID));
        var groups = cards.Select(c => c.chapter).ToList();

        DashUI.Title(skin, content, $"CARTAS RECOGIDAS: {cards.Count(c => collected.Contains(c.cardID))} DE {cards.Count}");
        DashUI.Strip(skin, content, cards.Select(c => collected.Contains(c.cardID)).ToList(), groups);

        DashUI.Title(skin, content, $"CARTAS RESPONDIDAS: {cards.Count(c => answered.Contains(c.cardID))} DE {cards.Count}");
        DashUI.Strip(skin, content, cards.Select(c => answered.Contains(c.cardID)).ToList(), groups);

        DashUI.Title(skin, content, "RESPUESTAS EN LOS DIÁLOGOS");
        var eff = StudentStatus.EffectiveChoices(p);
        DashUI.Donut(skin, content,
            eff.Count(c => c.relationshipDelta > 0),
            eff.Count(c => c.relationshipDelta == 0),
            eff.Count(c => c.relationshipDelta < 0));

        DashUI.Title(skin, content, "RELACIÓN CON PERSONAJES");
        for (int i = 0; i < npcDisplay.GetLength(0); i++)
        {
            int rel = RelationOf(npcDisplay[i, 0]);
            var row = DashUI.Row(content, "Relation", 36);
            Sprite face = rel == 2 ? skin.faceNone : rel > 0 ? skin.faceGood : rel < 0 ? skin.faceBad : skin.faceNeutral;
            DashUI.Img(row, face, 12, 7.5f, 21, 21);
            DashUI.Text(skin, row, npcDisplay[i, 1], 45, 0, 300, 36, 14, DashUI.Ink);
            if (rel == 2) DashUI.Tag(skin, row, TagKind.Cream, "SIN ENCUENTRO");
            else if (rel > 0) DashUI.Tag(skin, row, TagKind.Green, "BUENA");
            else if (rel < 0) DashUI.Tag(skin, row, TagKind.Red, "MALA");
            else DashUI.Tag(skin, row, TagKind.Yellow, "NEUTRAL");
        }
    }

    // ─── CAPÍTULO ─────────────────────────────────────────────
    void BuildChapter(int c)
    {
        var p = student.progress;
        var st = StudentStatus.GetChapterState(p, c);

        var hdr = DashUI.Row(content, "ChapterStatus", 36);
        DashUI.Img(hdr, DotFor(st), 12, 7.5f, 21, 21);
        DashUI.Text(skin, hdr, $"CAPÍTULO {c} - {StateName(st)}", 45, 0, 450, 36, 14, DashUI.Ink,
                    TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

        DashUI.Title(skin, content, "PERSONAJES");
        DashUI.Note(skin, content, "Cada cuadrito = una decisión: verde buena · amarillo neutral · rojo mala.", 18);
        foreach (var e in Encounters.Where(x => x.chapter == c))
        {
            var rounds = p.choices.Where(x => x.nodeID == e.nodeID)
                .GroupBy(x => x.roundIndex).OrderBy(g => g.Key)
                .Select(g => g.Last().relationshipDelta).ToList();

            var row = DashUI.Row(content, "Encounter", 36);
            DashUI.Text(skin, row, $"{e.npcName} - ENCUENTRO {e.num}", 12, 0, 250, 36, 13, DashUI.Ink);
            float sx = 456f - Mathf.Min(rounds.Count, 6) * 17f;
            for (int k = 0; k < rounds.Count && k < 6; k++)
                DashUI.Swatch(row, sx + k * 17, 11, 12, DashUI.DeltaColor(rounds[k]));

            if (rounds.Count == 0) DashUI.Tag(skin, row, TagKind.Cream, "SIN ENCUENTRO");
            else
            {
                int sum = rounds.Sum();
                if (sum > 0) DashUI.Tag(skin, row, TagKind.Green, "BUENA");
                else if (sum < 0) DashUI.Tag(skin, row, TagKind.Red, "MALA");
                else DashUI.Tag(skin, row, TagKind.Yellow, "NEUTRAL");
            }
        }

        DashUI.Title(skin, content, "CARTAS");
        var cards = AllCards();
        var inChapter = cards.Where(x => x.chapter == c).ToList();
        if (inChapter.Count == 0)
        {
            DashUI.Note(skin, content, "No hay cartas asignadas a este capítulo.");
            return;
        }

        var collected = new HashSet<string>(p.allTimeCollectedCards.Select(x => x.cardID));
        var answers = new Dictionary<string, EpistolaryResponse>();
        foreach (var r in p.epistolaryResponses) answers[r.cardID] = r;

        foreach (var card in inChapter)
        {
            int n = cards.Where(x => x.authorName == card.authorName).ToList().IndexOf(card) + 1;
            BuildCard(card, n, collected.Contains(card.cardID),
                      answers.TryGetValue(card.cardID, out var resp) ? resp : null);
        }
    }

    void BuildCard(CardData card, int n, bool got, EpistolaryResponse resp)
    {
        var holder = new GameObject("Card_" + card.cardID, typeof(RectTransform));
        holder.transform.SetParent(content, false);
        var v = holder.AddComponent<VerticalLayoutGroup>();
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        v.spacing = 0;

        var head = DashUI.Row(holder.transform, "Head", 36);
        DashUI.Text(skin, head, $"{card.authorName.ToUpperInvariant()} - CARTA {n}", 12, 0, 280, 36, 13, DashUI.Ink);

        if (resp != null)
        {
            DashUI.Text(skin, head, "ver respuesta", 300, 0, 140, 36, 11, DashUI.Ink);
            DashUI.Tag(skin, head, TagKind.Green, "RESPONDIDA");

            var detail = BuildDetail(holder.transform, resp);
            detail.gameObject.SetActive(false);

            var img = head.GetComponent<Image>();
            img.raycastTarget = true;
            var btn = head.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                detail.gameObject.SetActive(!detail.gameObject.activeSelf);
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            });
        }
        else if (got) DashUI.Tag(skin, head, TagKind.Yellow, "SIN RESPONDER");
        else DashUI.Tag(skin, head, TagKind.Cream, "NO RECOGIDA");
    }

    RectTransform BuildDetail(Transform parent, EpistolaryResponse r)
    {
        const float x0 = 24f;
        float inner = DashUI.W - x0 - 18f;
        var d = DashUI.Block(parent, "Detail", 100, skin.reflection);

        DashUI.Text(skin, d, "REFLEXIÓN", x0, 10, inner, 18, 11, DashUI.Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        string txt = string.IsNullOrWhiteSpace(r.reflectionText) ? "(sin texto)" : r.reflectionText;
        var t = DashUI.Text(skin, d, txt, x0, 30, inner, 20, 13, DashUI.Black, TextAlignmentOptions.TopLeft, FontStyles.Normal, true);
        float th = t.GetPreferredValues(txt, inner, 0f).y;
        t.rectTransform.sizeDelta = new Vector2(inner, th);

        float y = 30 + th + 12;
        DashUI.Text(skin, d, "HSE RELACIONADAS", x0, y, inner, 18, 11, DashUI.Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        y += 24;

        if (r.linkedHSE.Count == 0)
        {
            DashUI.Text(skin, d, "Sin HSE relacionadas", x0, y, inner, 20, 12, DashUI.Ink);
            y += 24;
        }
        else
        {
            float x = x0;
            const float rowH = 27f, gap = 6f;
            foreach (var h in r.linkedHSE)
            {
                var chip = DashUI.Img(d, skin.chip, x, y, 60, rowH);
                var tt = DashUI.Text(skin, chip.transform, h, 0, 0, 300, rowH, 12, DashUI.Ink, TextAlignmentOptions.Center);
                float cw = tt.GetPreferredValues(h).x + 24f;
                if (x + cw > x0 + inner) { x = x0; y += rowH + gap; }
                chip.rectTransform.anchoredPosition = new Vector2(x, -y);
                chip.rectTransform.sizeDelta = new Vector2(cw, rowH);
                tt.rectTransform.sizeDelta = new Vector2(cw, rowH);
                x += cw + gap;
            }
            y += rowH;
        }

        float height = y + 14;
        var le = d.GetComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = height;
        d.sizeDelta = new Vector2(DashUI.W, height);
        return d;
    }

    // ─── HSE ──────────────────────────────────────────────────
    void BuildHSE()
    {
        var responses = student.progress.epistolaryResponses;
        int total = responses.Count;
        var names = HseNames();

        DashUI.Title(skin, content, "HSE QUE EL ALUMNO RELACIONÓ CON SUS CARTAS", 22);
        var sub = DashUI.Block(content, "Subtitle", 18);

        DashUI.Text(skin, sub, $"{total} {(total == 1 ? "REFLEXIÓN ESCRITA" : "REFLEXIONES ESCRITAS")}", 0, 0, DashUI.W, 18, 12, DashUI.Ink);

        foreach (var hse in names)
        {
            int count = responses.Count(r => r.linkedHSE.Contains(hse));
            float frac = total > 0 ? (float)count / total : 0f;

            var row = DashUI.Row(content, "HSE", 30);
            DashUI.Text(skin, row, hse, 12, 0, 280, 30, 13, DashUI.Ink);
            DashUI.Img(row, skin.barTrack, 296, 7.5f, 240, 15);
            if (frac > 0f) DashUI.Img(row, skin.barFill, 299, 10.5f, Mathf.Max(3f, 234f * frac), 9);
            DashUI.Text(skin, row, count.ToString(), 544, 0, 38, 30, 14, DashUI.Ink,
                        TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        }

        DashUI.Note(skin, content,
            "Cada barra cuenta cuántas reflexiones el alumno relacionó con esa habilidad. No mide el nivel de la habilidad.", 32);
    }
}