using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeOutTime = 0.4f;
    public float fadeInTime = 0.2f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!fadeImage) return;

        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.raycastTarget = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndSwitch(sceneName));
    }

    IEnumerator FadeAndSwitch(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
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
        float startAlpha = fadeImage.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
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