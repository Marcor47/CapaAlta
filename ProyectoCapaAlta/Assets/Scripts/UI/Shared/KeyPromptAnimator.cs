using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class KeyPromptAnimator : MonoBehaviour
{
    public Sprite normal;
    public Sprite pressed;
    public float interval = 0.5f;

    private SpriteRenderer sr;
    private float t;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void OnEnable()
    {
        t = 0f;
        if (sr != null) sr.sprite = normal;
    }

    void Update()
    {
        t += Time.unscaledDeltaTime;
        sr.sprite = (Mathf.FloorToInt(t / interval) % 2 == 0) ? normal : pressed;
    }
}