using UnityEngine;

[CreateAssetMenu(fileName = "HelpSkin", menuName = "Capa Alta/Help Skin")]
public class HelpSkin : ScriptableObject
{
    [Header("Teclas")]
    public Sprite keyA, keyD, keyW, keyS, keyQ, keyE, keyEnter, keyEsc, keyShift, keySpace;
    public Sprite keyUp, keyDown, keyLeft, keyRight;

    [Header("Mouse")]
    public Sprite mouseLeft, mouseRight;

    [Header("Iconos de herramientas (los de la mochila)")]
    public Sprite iconManzana, iconTelefono, iconOnda, iconGancho, iconPinchos;

    [Header("Iconos de barras (opcional)")]
    public Sprite iconMotivacion, iconRegulacion;
}