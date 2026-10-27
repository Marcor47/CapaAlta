using UnityEngine;

// Animación por cuadros sin Animator: agua, faro encendido, puf de impacto.
// Se pone en el mismo GameObject que el SpriteRenderer.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 6f;
    public bool loop = true;
    public bool playOnStart = true;
    public bool destroyOnEnd = false;   // puf: se destruye al terminar

    private SpriteRenderer sr;
    private float t;
    private bool playing;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void OnEnable()
    {
        if (playOnStart) Play();
    }

    public void Play()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        t = 0f;
        playing = frames != null && frames.Length > 0;
        if (playing) sr.sprite = frames[0];
    }

    public void Stop() => playing = false;

    void Update()
    {
        if (!playing || fps <= 0f) return;

        t += Time.deltaTime;
        int i = Mathf.FloorToInt(t * fps);

        if (i >= frames.Length)
        {
            if (loop) i %= frames.Length;
            else
            {
                sr.sprite = frames[frames.Length - 1];
                playing = false;
                if (destroyOnEnd) Destroy(gameObject);
                return;
            }
        }
        sr.sprite = frames[i];
    }
}
