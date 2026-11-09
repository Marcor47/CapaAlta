using UnityEngine;

// Animación por cuadros de un NPC con dos estados: reposo y hablando.
// Va en el mismo GameObject que el SpriteRenderer. Lo controla DialogueManager.
[RequireComponent(typeof(SpriteRenderer))]
public class NpcTalkAnimator : MonoBehaviour
{
    [Header("Cuadros")]
    public Sprite[] idleFrames;
    public Sprite[] talkFrames;

    [Header("Velocidad (cuadros por segundo)")]
    public float idleFps = 4f;
    public float talkFps = 4f;

    private SpriteRenderer sr;
    private bool talking;
    private float t;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnEnable()
    {
        talking = false;
        t = 0f;
        Apply();
    }

    public void SetTalking(bool value)
    {
        if (talking == value) return;
        talking = value;
        t = 0f;
        Apply();
    }

    void Update()
    {
        Sprite[] set = talking ? talkFrames : idleFrames;
        if (set == null || set.Length < 2) return;

        t += Time.deltaTime;
        sr.sprite = set[Mathf.FloorToInt(t * (talking ? talkFps : idleFps)) % set.Length];
    }

    void Apply()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        Sprite[] set = talking ? talkFrames : idleFrames;
        if (set != null && set.Length > 0) sr.sprite = set[0];
    }
}