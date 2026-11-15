using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Finales: páginas de libreta con dibujo + narración corta (con balbuceo).
/// Orden: final con el padre (2) -> epílogos de NPC con relación +1 -> Fin -> LoginScene.
/// </summary>
public class FinalesScene : MonoBehaviour
{
    [Header("Libreta")]
    public CanvasGroup page;              // contenedor de dibujo + texto
    public Image drawing;
    public TextMeshProUGUI narration;
    public GameObject continueArrow;

    [Header("Final con el padre (2 dibujos por final)")]
    public Sprite plusA;
    public Sprite plusB;
    public Sprite neutralA;
    public Sprite neutralB;
    public Sprite minusA;
    public Sprite minusB;

    [Header("Epílogos")]
    public Sprite epiGaz;
    public Sprite epiLeeWilber;
    public Sprite epiHaru;
    public Sprite epiRylandAstro;

    [Header("Cierre")]
    public TextMeshProUGUI finText;       // "Fin" centrado sobre la página

    [Header("Tiempos")]
    public float introFade = 1.2f;
    public float pageFade = 0.4f;
    public float outFade = 1.5f;
    public float charsPerSecond = 35f;

    [Header("Siguiente")]
    public string nextScene = "LoginScene";
    public int chapter = 4;

    [Header("Debug (solo pruebas)")]
    public bool debugActivar;
    public int debugPadre;                // -1, 0 o 1
    public bool debugTodosEpilogos;

    struct Slide
    {
        public Sprite img; public string text; public bool isFin;
        public Slide(Sprite i, string t, bool fin = false) { img = i; text = t; isFin = fin; }
    }

    readonly List<Slide> slides = new List<Slide>();

    IEnumerator Start()
    {
        BuildSlides();

        page.alpha = 0f;
        narration.text = "";
        if (finText != null) finText.gameObject.SetActive(false);
        if (continueArrow != null) continueArrow.SetActive(false);

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.SetAlpha(1f);
            yield return ScreenFader.Instance.FadeTo(0f, introFade);
        }

        foreach (var s in slides)
            yield return ShowSlide(s);

        yield return Exit();
    }

    // ─── ARMAR LA LISTA SEGÚN LO JUGADO ────────────────────────
    void BuildSlides()
    {
        int padre = DecisionRecord.Instance != null ? DecisionRecord.Instance.GetRelationship("padre") : 0;
        if (debugActivar) padre = debugPadre;

        // Misma diapositiva 1 para relación 0 y -1
        const string quedarsePueblo =
            "Nos quedamos unos días en el pueblo, caminando sin apuro. Cada rincón tenía algo que papá había cuidado con tanto esfuerzo. Entendí que amar un lugar y extrañar a alguien pueden doler al mismo tiempo.";

        if (padre >= 1)
        {
            slides.Add(new Slide(plusA,
                "Acompañé a papá a despedirse del pueblo. Lo vi abrazar a cada uno como quien se va sin soltar nada de lo que ama. Entendí que quedarse nunca fue falta de cariño, y que perdonar empezaba por mirarlo con empatía."));
            slides.Add(new Slide(plusB,
                "Volvimos juntos a casa de mamá. Sé que años de silencio no se borran en un día, pero los dos tuvieron el valor de intentarlo. Yo solo quise ayudarlos a darse esa oportunidad, y ver a papá dar el primer paso hacia esa puerta valió toda la montaña."));
        }
        else if (padre == 0)
        {
            slides.Add(new Slide(neutralA, quedarsePueblo));
            slides.Add(new Slide(neutralB,
                "Al final volvimos juntos a casa de mamá. No será fácil, los dos cargan años de dolor y de dudas. Pero vi a papá con ganas de cambiar, y creo que eso ya es empezar a sanar."));
        }
        else
        {
            slides.Add(new Slide(minusA, quedarsePueblo));
            slides.Add(new Slide(minusB,
                "Me enamoré del pueblo y entendí mejor a papá, pero terminé regresando solo a casa de mamá. Todavía le cuesta perdonarse, y aprendí que la empatía también sabe esperar. Le dejé la puerta abierta, por si algún día se anima a volver."));
        }

        if (Plus("gaz"))
            slides.Add(new Slide(epiGaz,
                "Usé mis poderes para volver por Gaz y llevarlo a la cima. Subimos con calma, y en el camino me habló de por qué se había exigido tanto para llegar hasta aquí. Desde arriba miramos el pueblo en silencio. Entendí que a veces uno solo necesita de alguien que se tome el tiempo de escucharlo, un amigo."));

        if (Plus("lee_wilber"))
            slides.Add(new Slide(epiLeeWilber,
                "Lee y Wilber abrieron un restaurante. Durante años cargaron con lo que les pasó, y ahora cada mañana abren la puerta como si fuera la primera vez. A veces un día más es justo lo que hace falta para empezar de nuevo, o incluso algo diferente."));

        if (Plus("haru"))
            slides.Add(new Slide(epiHaru,
                "Cuando ya volvía a la ciudad, encontré a Haru en la parada de bus. Nos sentamos a esperar juntos mientras me mostraba las fotos de su cámara. No sé si volveremos a vernos, pero pensé que no hace falta saberlo para disfrutar del rato que tenemos."));

        if (Plus("ryland_astro"))
            slides.Add(new Slide(epiRylandAstro,
                "Ryland y Astro dejaron una rosa al pie de un árbol, recordando a su hijo con una sonrisa tranquila. Los vi irse en su bicicleta doble y comprendí que se puede extrañar a alguien con todo el corazón y aun así seguir pedaleando."));

        slides.Add(new Slide(null, "Fin", true));
    }

    bool Plus(string npcID)
        => (debugActivar && debugTodosEpilogos) ||
           (DecisionRecord.Instance != null && DecisionRecord.Instance.GetRelationship(npcID) >= 1);

    // ─── MOSTRAR UNA PÁGINA ────────────────────────────────────
    IEnumerator ShowSlide(Slide s)
    {
        drawing.sprite = s.img;
        drawing.enabled = !s.isFin && s.img != null;
        narration.gameObject.SetActive(!s.isFin);
        if (finText != null) finText.gameObject.SetActive(s.isFin);

        if (!s.isFin)
        {
            narration.text = s.text;
            narration.maxVisibleCharacters = 0;
        }

        yield return FadePage(1f);

        if (!s.isFin)
        {
            float duration = Mathf.Max(0.5f, s.text.Length / charsPerSecond);
            AudioManager.PlayBabble(s.text, duration);

            // máquina de escribir; un clic muestra todo el texto
            float t = 0f;
            narration.ForceMeshUpdate();
            int total = narration.textInfo.characterCount;
            while (narration.maxVisibleCharacters < total)
            {
                if (Clicked()) break;
                t += Time.unscaledDeltaTime;
                narration.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(t * charsPerSecond));
                yield return null;
            }
            narration.maxVisibleCharacters = total;
            AudioManager.StopBabble();
            yield return null;                   // evita que el mismo clic avance la página
        }

        if (continueArrow != null) continueArrow.SetActive(true);
        while (!Clicked()) yield return null;
        if (continueArrow != null) continueArrow.SetActive(false);

        yield return FadePage(0f);
    }

    IEnumerator FadePage(float target)
    {
        float start = page.alpha, t = 0f;
        while (t < pageFade)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            page.alpha = Mathf.Lerp(start, target, t / pageFade);
            yield return null;
        }
        page.alpha = target;
    }

    bool Clicked()
    {
        bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool key = Keyboard.current != null &&
                   (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
        return mouse || key;
    }

    // ─── SALIDA ────────────────────────────────────────────────
    IEnumerator Exit()
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeTo(1f, outFade);

        if (AccountManager.Instance != null)
        {
            // para que LoginScene muestre la notificación de fin de capítulo del Cap4
            AccountManager.Instance.justFinishedChapter = true;
            AccountManager.Instance.justFinishedChapterNumber = chapter;
        }
        SceneManager.LoadScene(nextScene);
    }
}