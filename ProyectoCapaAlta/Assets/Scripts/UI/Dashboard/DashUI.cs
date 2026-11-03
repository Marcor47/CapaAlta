using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TagKind { Green, Orange, Yellow, Red, Cream }

public static class DashUI
{
    public static readonly Color Ink = new Color32(0x96, 0x42, 0x53, 255);
    public static readonly Color Black = new Color32(0x17, 0x17, 0x17, 255);
    public static readonly Color Green = new Color32(0x8F, 0xDE, 0x5D, 255);
    public static readonly Color Yellow = new Color32(0xFF, 0xE4, 0x78, 255);
    public static readonly Color Red = new Color32(0xEB, 0x56, 0x4B, 255);
    static readonly Color RowBg = new Color32(0xFF, 0xFC, 0xEE, 255);
    static readonly Color RowLine = new Color32(0xE3, 0xCF, 0x9F, 255);
    public const float W = 594f;   // ancho útil: viewport 612 - padding 18

    // ─── básicos ──────────────────────────────────────────────
    static RectTransform NewRT(string name, Transform parent)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        return rt;
    }

    static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // Elemento de layout con altura fija (hijo directo del Content)
    public static RectTransform Block(Transform parent, string name, float h, Sprite bg = null)
    {
        var rt = NewRT(name, parent);
        rt.sizeDelta = new Vector2(W, h);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.minHeight = h; le.preferredHeight = h;
        if (bg != null)
        {
            var im = rt.gameObject.AddComponent<Image>();
            im.sprite = bg; im.type = Image.Type.Sliced; im.raycastTarget = false;
        }
        return rt;
    }

    public static RectTransform Row(Transform parent, string name, float h)
    {
        var rt = Block(parent, name, h);
        var bg = rt.gameObject.AddComponent<Image>();
        bg.color = RowBg; bg.raycastTarget = false;

        var line = Img(rt, null, 0, 0, 0, 3);
        line.color = RowLine;
        var lr = line.rectTransform;
        lr.anchorMin = new Vector2(0f, 0f);
        lr.anchorMax = new Vector2(1f, 0f);
        lr.pivot = new Vector2(0.5f, 0f);
        lr.offsetMin = Vector2.zero;
        lr.offsetMax = new Vector2(0f, 3f);
        return rt;
    }

    public static Image Img(Transform parent, Sprite sp, float x, float y, float w, float h)
    {
        var rt = NewRT(sp != null ? sp.name : "Img", parent);
        Place(rt, x, y, w, h);
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = sp; im.type = Image.Type.Sliced; im.raycastTarget = false;
        return im;
    }

    public static Image ImgRight(Transform parent, Sprite sp, float rightMargin, float y, float w, float h)
    {
        var im = Img(parent, sp, 0, 0, w, h);
        var rt = im.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-rightMargin, -y);
        return im;
    }

    public static TextMeshProUGUI Text(DashboardSkin s, Transform parent, string txt,
        float x, float y, float w, float h, float size, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft,
        FontStyles style = FontStyles.Normal, bool wrap = false)
    {
        var rt = NewRT("Text", parent);
        Place(rt, x, y, w, h);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (s != null && s.font != null) t.font = s.font;
        t.text = txt; t.fontSize = size; t.color = color;
        t.alignment = align; t.fontStyle = style;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static void Swatch(Transform parent, float x, float y, float size, Color c)
    {
        var edge = Img(parent, null, x, y, size + 2, size + 2); edge.color = Black;
        var fill = Img(parent, null, x + 1, y + 1, size, size); fill.color = c;
    }

    public static Color DeltaColor(int d) => d > 0 ? Green : d < 0 ? Red : Yellow;

    // ─── bloques de contenido ─────────────────────────────────
    public static void Spacer(Transform parent, float h) => Block(parent, "Spacer", h);

    public static void Title(DashboardSkin s, Transform parent, string text, float h = 24f)
    {
        var b = Block(parent, "Title", h);
        Text(s, b, text, 0, 0, W, h, 14, Ink, TextAlignmentOptions.BottomLeft, FontStyles.Bold);
    }

    public static void Note(DashboardSkin s, Transform parent, string text, float h = 30f)
    {
        var b = Block(parent, "Note", h);
        Text(s, b, text, 0, 0, W, h, 11, Ink, TextAlignmentOptions.TopLeft, FontStyles.Normal, true);
    }

    public static void Tag(DashboardSkin s, Transform parent, TagKind k, string text, float w = 120f, float y = 4.5f)
    {
        Sprite sp; Color c;
        switch (k)
        {
            case TagKind.Green: sp = s.tagGreen; c = Color.white; break;
            case TagKind.Red: sp = s.tagRed; c = Color.white; break;
            case TagKind.Orange: sp = s.tagOrange; c = Black; break;
            case TagKind.Yellow: sp = s.tagYellow; c = Black; break;
            default: sp = s.tagCream; c = Ink; break;
        }
        var im = ImgRight(parent, sp, 9, y, w, 27);
        Text(s, im.transform, text, 0, 0, w, 27, 11, c, TextAlignmentOptions.Center, FontStyles.Bold);
    }

    // Tira de cartas: un segmento por carta, con un hueco al cambiar de grupo (capítulo)
    public static void Strip(DashboardSkin s, Transform parent, IList<bool> flags, IList<int> groups)
    {
        var b = Block(parent, "Strip", 24);
        float x = 12f;
        for (int i = 0; i < flags.Count; i++)
        {
            if (i > 0 && groups[i] != groups[i - 1]) x += 9f;
            Img(b, flags[i] ? s.cardFull : s.cardEmpty, x, 0, 12, 24);
            x += 15f;
        }
    }

    // Dona con leyenda
    public static void Donut(DashboardSkin s, Transform parent, int good, int neutral, int bad)
    {
        var b = Block(parent, "Donut", 132);
        int total = good + neutral + bad;

        Img(b, s.donutBase, 12, 6, 120, 120);
        if (total > 0)
        {
            float g = (float)good / total, n = (float)neutral / total, m = (float)bad / total;
            Seg(s.donutGood, b, 0f, g);
            Seg(s.donutNeutral, b, g, n);
            Seg(s.donutBad, b, g + n, m);
        }
        Img(b, s.donutCap, 12, 6, 120, 120);
        Text(s, b, total.ToString(), 12, 6, 120, 120, 18, Ink, TextAlignmentOptions.Center, FontStyles.Bold);

        if (total == 0)
        {
            Text(s, b, "SIN DECISIONES REGISTRADAS", 150, 48, 400, 24, 14, Ink);
            return;
        }
        string[] names = { "BUENAS", "NEUTRAS", "MALAS" };
        int[] vals = { good, neutral, bad };
        Color[] cols = { Green, Yellow, Red };
        for (int i = 0; i < 3; i++)
        {
            float y = 22 + i * 32;
            Swatch(b, 150, y + 5, 12, cols[i]);
            Text(s, b, $"{names[i]}   {vals[i]} ({Mathf.RoundToInt(100f * vals[i] / total)}%)",
                 176, y, 360, 24, 14, Ink);
        }
    }

    static void Seg(Sprite sp, Transform parent, float start, float frac)
    {
        if (frac <= 0f) return;
        var im = Img(parent, sp, 12, 6, 120, 120);
        im.type = Image.Type.Filled;
        im.fillMethod = Image.FillMethod.Radial360;
        im.fillOrigin = (int)Image.Origin360.Top;
        im.fillClockwise = true;
        im.fillAmount = frac;
        var rt = im.rectTransform;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(12 + 60, -(6 + 60));
        rt.localRotation = Quaternion.Euler(0f, 0f, -start * 360f);
    }

    public static readonly Color Orange = new Color32(0xF2, 0xA6, 0x5E, 255);

    // Barra apilada con borde negro; cada tramo muestra su conteo
    public static void StackedBar(DashboardSkin s, Transform parent, float x, float y, float w, float h,
                                  int[] vals, Color[] cols)
    {
        int total = 0; foreach (var v in vals) total += v;
        var edge = Img(parent, null, x, y, w, h); edge.color = Black;
        float innerW = w - 4f, cx = x + 2f;
        if (total == 0)
        {
            var empty = Img(parent, null, cx, y + 2, innerW, h - 4); empty.color = RowLine;
            return;
        }
        for (int i = 0; i < vals.Length; i++)
        {
            if (vals[i] <= 0) continue;
            float sw = innerW * vals[i] / total;
            var seg = Img(parent, null, cx, y + 2, sw, h - 4);
            seg.color = cols[i];
            if (sw >= 18f)
                Text(s, seg.transform, vals[i].ToString(), 0, 0, sw, h - 4, 12,
                     cols[i] == Red ? Color.white : Black, TextAlignmentOptions.Center, FontStyles.Bold);
            cx += sw;
        }
    }

    // Pista + relleno (misma proporción que las filas HSE del alumno)
    public static void Meter(DashboardSkin s, Transform parent, float x, float y, float w, float frac)
    {
        Img(parent, s.barTrack, x, y, w, 15);
        if (frac > 0f) Img(parent, s.barFill, x + 3, y + 3, Mathf.Max(3f, (w - 6f) * Mathf.Clamp01(frac)), 9);
    }

    // Etiqueta pequeña de ancho automático; devuelve su ancho
    public static float Chip(DashboardSkin s, Transform parent, string text, float x, float y, float h = 24f, float size = 11f)
    {
        var chip = Img(parent, s.chip, x, y, 60, h);
        var t = Text(s, chip.transform, text, 0, 0, 400, h, size, Ink, TextAlignmentOptions.Center);
        float w = t.GetPreferredValues(text).x + 20f;
        chip.rectTransform.sizeDelta = new Vector2(w, h);
        t.rectTransform.sizeDelta = new Vector2(w, h);
        return w;
    }
}