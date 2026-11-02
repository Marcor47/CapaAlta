using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentRowUI : MonoBehaviour
{
    [Header("Textos")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI cardsText;
    public TextMeshProUGUI tagText;

    [Header("Imágenes")]
    public Image[] chapterDots;      // 4, en orden Cap1..Cap4
    public Image tagBackground;

    [Header("Botones")]
    public Button viewButton;
    public Button deleteButton;

    [Header("Sprites de punto")]
    public Sprite dotBloqueado;
    public Sprite dotPendiente;
    public Sprite dotEnCurso;
    public Sprite dotCompleto;

    [Header("Sprites de etiqueta")]
    public Sprite tagVerde;
    public Sprite tagNaranja;

    static readonly Color TagDarkText = new Color32(0x17, 0x17, 0x17, 255);

    public void Setup(UserAccount s, Action onView, Action onDelete)
    {
        nameText.text = s.username;

        for (int c = 1; c <= StudentStatus.TotalChapters && c <= chapterDots.Length; c++)
        {
            switch (StudentStatus.GetChapterState(s.progress, c))
            {
                case ChapterState.Bloqueado: chapterDots[c - 1].sprite = dotBloqueado; break;
                case ChapterState.Pendiente: chapterDots[c - 1].sprite = dotPendiente; break;
                case ChapterState.EnCurso: chapterDots[c - 1].sprite = dotEnCurso; break;
                default: chapterDots[c - 1].sprite = dotCompleto; break;
            }
        }

        int total = CardDatabase.Instance != null ? CardDatabase.Instance.allCards.Count : 16;
        cardsText.text = $"{s.progress.allTimeCollectedCards.Count}/{total}";

        bool alDia = StudentStatus.IsAlDia(s.progress);
        tagBackground.sprite = alDia ? tagVerde : tagNaranja;
        tagText.text = alDia ? "AL DÍA" : "PEND";
        tagText.color = alDia ? Color.white : TagDarkText;

        viewButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        viewButton.onClick.AddListener(() => onView());
        deleteButton.onClick.AddListener(() => onDelete());
    }
}