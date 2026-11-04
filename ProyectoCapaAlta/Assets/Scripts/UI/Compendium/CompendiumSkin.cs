using UnityEngine;

[CreateAssetMenu(fileName = "CompendiumSkin", menuName = "Capa Alta/Compendium Skin")]
public class CompendiumSkin : ScriptableObject
{
    [Header("Slots de carta (48x69)")]
    public Sprite slotFather;
    public Sprite slotSecondary;
    public Sprite slotEmpty;
    public Sprite pendingBadge;   // badge_aviso

    [Header("Checks HSE (18x18)")]
    public Sprite checkOff;
    public Sprite checkOn;

    [Header("Campo de reflexión")]
    public Sprite inputField;     // campo_normal
}