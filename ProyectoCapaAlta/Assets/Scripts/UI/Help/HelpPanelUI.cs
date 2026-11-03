using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanelUI : MonoBehaviour
{
    [Header("Skins y datos")]
    public DashboardSkin skin;
    public HelpSkin help;
    public HSEGlossary glossary;

    [Header("Cabecera y pestañas (orden: CÓMO JUGAR, GLOSARIO HSE)")]
    public TextMeshProUGUI headerText;
    public Button[] tabButtons;
    public TextMeshProUGUI[] tabLabels;

    [Header("Contenido")]
    public ScrollRect scroll;
    public RectTransform content;

    static readonly string[] TabNames = { "CÓMO JUGAR", "GLOSARIO HSE" };
    const float KeyRowH = 54f;
    const float DescX = 232f;
    const float InfoRowMin = 70f;
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

    public void Open()
    {
        Wire();
        headerText.text = "AYUDA";
        for (int i = 0; i < tabLabels.Length && i < TabNames.Length; i++)
            tabLabels[i].text = TabNames[i];
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

        if (tab == 0) BuildHowToPlay();
        else BuildGlossary();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;
    }

    // ─── CÓMO JUGAR ───────────────────────────────────────────
    void BuildHowToPlay()
    {
        DashUI.Title(skin, content, "MOVIMIENTO");
        KeyRow("Caminar a la izquierda y a la derecha.", help.keyA, help.keyD, "o", help.keyLeft, help.keyRight);
        KeyRow("Saltar. Gasta un poco de Motivación.", help.keySpace, "o", help.keyUp);
        KeyRow("Impulso rápido (dash). Gasta bastante Motivación.", help.keyShift);
        KeyRow("Mantén 1 segundo para sentarte y abrir tu libreta. Sentado recuperas Motivación y Regulación emocional.", help.keyS, "o", help.keyDown);

        DashUI.Spacer(content, 6);
        DashUI.Title(skin, content, "ACCIONES");
        KeyRow("Interactuar: hablar con los personajes, leer y recoger cartas.", help.keyE);
        KeyRow("Mantén E para abrir la mochila y elegir una herramienta.", help.keyE);
        KeyRow("Guardar la herramienta que llevas en la mano.", help.keyQ);
        KeyRow("Usar la herramienta (con algunas se mantiene el clic para apuntar y se suelta para lanzar). También avanza los diálogos.", help.mouseLeft);
        KeyRow("Cancelar la acción en curso.", help.mouseRight);
        KeyRow("Con Pinchos en una pared: mantén el clic y sube o baja.", help.mouseLeft, "+", help.keyW, help.keyS);
        KeyRow("Cerrar una carta.", help.keyE, "o", help.keySpace);
        KeyRow("Pausar el juego (dentro de un capítulo).", help.keyEsc);
        KeyRow("Terminar el capítulo cuando llegues al hito de la meta.", help.keyEnter);

        DashUI.Spacer(content, 6);
        DashUI.Title(skin, content, "HERRAMIENTAS");
        DashUI.Note(skin, content, "Equípalas desde la mochila (mantén E) y úsalas con el clic izquierdo.", 20);
        InfoRow(help.iconManzana, "MANZANA", "Es un regalo para un personaje con el que ya hablaste: equípala, acércate y presiona E. Mejora tu relación con él (una vez por personaje).");
        InfoRow(help.iconTelefono, "TELÉFONO", "Llama a mamá: haz clic y espera a que conteste. Mientras hablas recuperas Regulación emocional más rápido.");
        InfoRow(help.iconOnda, "ONDA", "Lanza una pelota que rompe las rocas azules. Mantén el clic para cargar y suelta para lanzar.");
        InfoRow(help.iconGancho, "GANCHO", "Apunta y lanza una cuerda a una rama enganchable para balancearte.");
        InfoRow(help.iconPinchos, "PINCHOS", "Pégate a una pared: mantén el clic izquierdo y sube o baja con W y S. Salta con Espacio. Clic derecho para soltarte. Gasta Motivación.");

        DashUI.Spacer(content, 6);
        DashUI.Title(skin, content, "BARRAS");
        InfoRow(help.iconMotivacion, "MOTIVACIÓN", "Es tu energía para saltar, hacer dash y trepar. Se recarga sola, y más rápido si te sientas a descansar. Su máximo sube cuando reúnes todas las cartas de un mismo personaje.");
        InfoRow(help.iconRegulacion, "REGULACIÓN EMOCIONAL", "Mide qué tan tranquilo estás. Baja si caes al vacío o al agua, y cuando eliges mal en una conversación difícil. No se recarga sola: se recupera sentado con tu libreta o llamando a mamá. Si llega a cero, la visión se cierra alrededor de Theo y tu Motivación máxima baja a la mitad. Su máximo sube con cada carta del padre de Theo.");

        DashUI.Spacer(content, 6);
        DashUI.Title(skin, content, "CARTAS");
        InfoRow(null, "RECÓGELAS Y RESPÓNDELAS", "Recoge cartas con E. Al volver al menú, ábrelas en el Compendio, escribe tu reflexión y marca las habilidades (HSE) que relacionas con la carta.");
    }

    // tokens: Sprite (tecla/mouse) o string ("o", "+")
    void KeyRow(string desc, params object[] tokens)
    {
        var row = DashUI.Row(content, "KeyRow", KeyRowH);
        float x = 12f;
        foreach (var tk in tokens)
        {
            if (tk is Sprite sp)
            {
                float w = sp.rect.width, h = sp.rect.height;
                DashUI.Img(row, sp, x, (KeyRowH - h) * 0.5f, w, h);
                x += w + 6f;
            }
            else if (tk is string s)
            {
                DashUI.Text(skin, row, s, x, 0, 22, KeyRowH, 12, DashUI.Ink, TextAlignmentOptions.Center);
                x += 28f;
            }
        }
        DashUI.Text(skin, row, desc, DescX, 0, DashUI.W - DescX - 10f, KeyRowH, 12, DashUI.Black,
            TextAlignmentOptions.MidlineLeft, FontStyles.Normal, true);
    }

    void InfoRow(Sprite icon, string title, string desc)
    {
        float tx = icon != null ? 72f : 12f;
        float w = DashUI.W - tx - 10f;
        var row = DashUI.Row(content, "InfoRow", InfoRowMin);

        DashUI.Text(skin, row, title, tx, 6, w, 20, 13, DashUI.Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        float h = PlaceWrapped(row, desc, tx, 27, w, 11, DashUI.Black);
        float total = Mathf.Max(InfoRowMin, 27f + h + 10f);
        SetRowHeight(row, total);

        if (icon != null)
        {
            var im = DashUI.Img(row, icon, 12, (total - 48f) * 0.5f, 48, 48);
            im.preserveAspect = true;
        }
    }

    // ─── GLOSARIO HSE ─────────────────────────────────────────
    void BuildGlossary()
    {
        DashUI.Title(skin, content, "HABILIDADES SOCIOEMOCIONALES (HSE)");
        if (glossary == null || glossary.entries.Count == 0)
        {
            DashUI.Note(skin, content, "Falta asignar o cargar el glosario.", 20);
            return;
        }

        if (!string.IsNullOrEmpty(glossary.intro)) DashUI.Note(skin, content, glossary.intro, 44);
        foreach (var e in glossary.entries) GlossaryRow(e);
        if (!string.IsNullOrEmpty(glossary.source)) DashUI.Note(skin, content, glossary.source, 44);
    }

    void GlossaryRow(HSEGlossary.Entry e)
    {
        float w = DashUI.W - 24f;
        var row = DashUI.Row(content, "HSE", 100f);

        DashUI.Text(skin, row, e.name, 12, 8, w, 20, 14, DashUI.Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        float y = 32f;

        y += PlaceWrapped(row, e.simple, 12, y, w, 12, DashUI.Black) + 8f;
        DashUI.Text(skin, row, "DEFINICIÓN MINEDU", 12, y, w, 14, 10, DashUI.Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        y += 16f;
        y += PlaceWrapped(row, e.minedu, 12, y, w, 11, DashUI.Ink) + 10f;

        SetRowHeight(row, y);
    }

    // ─── utilidades ───────────────────────────────────────────
    float PlaceWrapped(Transform parent, string txt, float x, float y, float w, float size, Color color)
    {
        var t = DashUI.Text(skin, parent, txt, x, y, w, 20, size, color,
            TextAlignmentOptions.TopLeft, FontStyles.Normal, true);
        float h = Mathf.Ceil(t.GetPreferredValues(txt, w, 0f).y);
        t.rectTransform.sizeDelta = new Vector2(w, h);
        return h;
    }

    static void SetRowHeight(RectTransform row, float h)
    {
        row.sizeDelta = new Vector2(row.sizeDelta.x, h);
        var le = row.GetComponent<LayoutElement>();
        le.minHeight = h;
        le.preferredHeight = h;
    }
}