using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeOutTime = 0.4f;
    public float fadeInTime = 0.2f;

    void Awake()
    {
        if (!fadeImage) return;

        var c = fadeImage.color;
        c.a = 0f;                 // start transparent
        fadeImage.color = c;
        fadeImage.raycastTarget = false;
    }

    public IEnumerator FadeOut()
    {
        yield return FadeTo(1f, fadeOutTime);
    }

    public IEnumerator FadeIn()
    {
        yield return FadeTo(0f, fadeInTime);
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (!fadeImage) yield break;

        float startAlpha = fadeImage.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, duration <= 0f ? 1f : t / duration);
            var c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        var final = fadeImage.color;
        final.a = targetAlpha;
        fadeImage.color = final;
    }
}
