using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    public float duration = 0.4f;

    IEnumerator Start()
    {
        ScreenFader.Instance.SetAlpha(1f);
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.2f);
        yield return ScreenFader.Instance.FadeTo(0f, duration);
    }
}