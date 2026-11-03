using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DashboardSkin", menuName = "Capa Alta/Dashboard Skin")]
public class DashboardSkin : ScriptableObject
{
    public TMP_FontAsset font;

    [Header("Filas, pestañas y barras")]
    public Sprite row;          // fila
    public Sprite reflection;   // fila_reflexion
    public Sprite chip;
    public Sprite tabOn, tabOff;
    public Sprite barTrack, barFill;

    [Header("Etiquetas")]
    public Sprite tagGreen, tagOrange, tagYellow, tagRed, tagCream;

    [Header("Puntos de capítulo")]
    public Sprite dotLocked, dotPending, dotInProgress, dotDone;

    [Header("Caras")]
    public Sprite faceGood, faceNeutral, faceBad, faceNone;

    [Header("Cartas")]
    public Sprite cardFull, cardEmpty;

    [Header("Dona")]
    public Sprite donutBase, donutCap, donutGood, donutNeutral, donutBad;
}