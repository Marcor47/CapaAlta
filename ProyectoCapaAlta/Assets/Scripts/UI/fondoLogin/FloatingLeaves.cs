using UnityEngine;
using UnityEngine.UI;

public class FloatingLeaves : MonoBehaviour
{
    [Header("Hojas")]
    public Sprite[] leafSprites;
    public RectTransform container;       // el objeto Leaves
    [Range(1, 20)] public int count = 8;

    [Header("Movimiento")]
    public Vector2 speedRange = new Vector2(25f, 55f);   // px/seg hacia la derecha
    public Vector2 driftYRange = new Vector2(-8f, 12f);   // deriva vertical px/seg
    public Vector2 bobAmplitude = new Vector2(6f, 16f);   // vaivén px
    public Vector2 bobFrequency = new Vector2(0.4f, 1.1f);
    public float margin = 60f;

    class Leaf
    {
        public RectTransform rt;
        public Image img;
        public float x, y, speed, driftY, amp, freq, phase;
    }

    Leaf[] leaves;
    float width, height;

    void Start()
    {
        if (container == null) container = (RectTransform)transform;
        if (leafSprites == null || leafSprites.Length == 0) { enabled = false; return; }

        Rect r = container.rect;
        width = r.width; height = r.height;
        leaves = new Leaf[count];

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Leaf" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(container, false);
            var l = new Leaf { rt = (RectTransform)go.transform, img = go.GetComponent<Image>() };
            l.img.raycastTarget = false;
            l.rt.anchorMin = l.rt.anchorMax = new Vector2(0f, 0f);
            l.rt.pivot = new Vector2(0.5f, 0.5f);
            Respawn(l, true);
            leaves[i] = l;
        }
    }

    void Respawn(Leaf l, bool anywhere)
    {
        l.img.sprite = leafSprites[Random.Range(0, leafSprites.Length)];
        l.img.SetNativeSize();
        l.speed = Random.Range(speedRange.x, speedRange.y);
        l.driftY = Random.Range(driftYRange.x, driftYRange.y);
        l.amp = Random.Range(bobAmplitude.x, bobAmplitude.y);
        l.freq = Random.Range(bobFrequency.x, bobFrequency.y);
        l.phase = Random.Range(0f, Mathf.PI * 2f);
        l.x = anywhere ? Random.Range(-margin, width + margin) : -margin;
        l.y = Random.Range(0.1f * height, 0.9f * height);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;

        foreach (var l in leaves)
        {
            l.x += l.speed * dt;
            l.y += l.driftY * dt;

            if (l.x > width + margin || l.y < -margin || l.y > height + margin)
            {
                Respawn(l, false);
            }

            float by = l.y + Mathf.Sin(t * l.freq + l.phase) * l.amp;
            // píxeles enteros para que no tiemblen
            l.rt.anchoredPosition = new Vector2(Mathf.Round(l.x), Mathf.Round(by));
        }
    }
}