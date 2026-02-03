using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#region DATA

[System.Serializable]
public class Drawing
{
    public Sprite frameA;
    public Sprite frameB;
    public float secondsOnScreen = 2f;
    public float flickerInterval = 0.2f;
}

[System.Serializable]
public class TextScreen
{
    [TextArea(2, 6)]
    public string text;

    public float startDelay = 0f;
    public float fadeInTime = 0.5f;
    public float stayTime = 2f;
    public float fadeOutTime = 0.5f;
}

#endregion

public class CutsceneScript : MonoBehaviour
{
    [Header("UI")]
    public Image fadeOverlay;
    public Image cutsceneImage;
    public TextMeshProUGUI cutsceneText;
    public GameObject cutsceneCanvas;


    [Header("Data")]
    public Drawing[] drawings;
    public TextScreen[] textScreens;

    [Header("Timing")]
    public float blackHoldTime = 1f;
    public float blackFadeOutTime = 1f;
    public float imageFadeInTime = 0.3f;

    [Header("Exit Fade")]
    public float imageFadeOutTime = 1.5f;

    [Header("Motion")]
    public float jitterAmount = 2f;

    public AreaMusicTrigger areaMusic;


    Vector2 originalPos;

    Coroutine textRoutine;
    bool allowFlicker = true;

    void Awake()
    {
        originalPos = cutsceneImage.rectTransform.anchoredPosition;
        enabled = false; // wait for trigger
    }

    public void Play()
    {
        if (cutsceneCanvas == null)
        {
            Debug.LogError("CutsceneCanvas is not assigned!");
            return;
        }

        cutsceneCanvas.SetActive(true);

        // 🔊 START MUSIC DELAY AT CUTSCENE START
        if (areaMusic != null)
            areaMusic.StartMusicWithDelay();

        StartCoroutine(RunCutscene());
    }





    IEnumerator RunCutscene()
    {
        Time.timeScale = 0f;

        // BLACK SCREEN
        fadeOverlay.gameObject.SetActive(true);
        SetAlpha(fadeOverlay, 1f);

        SetAlpha(cutsceneImage, 0f);
        SetTextAlpha(0f);

        yield return new WaitForSecondsRealtime(blackHoldTime);

        // START DRAWINGS ONCE
        Coroutine drawingsRoutine = StartCoroutine(PlayAllDrawings());

        // START TEXT
        if (textScreens.Length > 0)
            textRoutine = StartCoroutine(PlayTextTimeline());

        // Fade image in (behind black)
        yield return FadeImageIn();

        // Reveal cutscene
        yield return FadeBlackOut();

        // Wait for drawings to finish
        yield return drawingsRoutine;

        // Stop text so it cannot reappear
        if (textRoutine != null)
            StopCoroutine(textRoutine);

        SetTextAlpha(0f);

        // Resume gameplay underneath
        Time.timeScale = 1f;

        // Keep flickering the LAST drawing during fade
        Drawing lastDrawing = drawings[drawings.Length - 1];
        Coroutine flickerRoutine = StartCoroutine(FlickerLastDrawing(lastDrawing));

        // Fade final frame into gameplay (slow → fast)
        yield return FadeImageOut();

        // Stop flicker
        allowFlicker = false;
        StopCoroutine(flickerRoutine);

        // Cleanup
        cutsceneImage.gameObject.SetActive(false);
        cutsceneText.gameObject.SetActive(false);
        fadeOverlay.gameObject.SetActive(false);

        enabled = false;
    }

    #region DRAWINGS

    IEnumerator PlayAllDrawings()
    {
        foreach (var d in drawings)
            yield return PlayDrawing(d);
    }

    IEnumerator PlayDrawing(Drawing d)
    {
        float start = Time.unscaledTime;
        bool showA = true;

        while (allowFlicker && Time.unscaledTime - start < d.secondsOnScreen)
        {
            cutsceneImage.sprite = showA ? d.frameA : d.frameB;
            showA = !showA;

            cutsceneImage.rectTransform.anchoredPosition =
                originalPos + Random.insideUnitCircle * jitterAmount;

            yield return new WaitForSecondsRealtime(d.flickerInterval);
        }

        cutsceneImage.rectTransform.anchoredPosition = originalPos;
    }

    IEnumerator FlickerLastDrawing(Drawing d)
    {
        bool showA = true;

        while (allowFlicker)
        {
            cutsceneImage.sprite = showA ? d.frameA : d.frameB;
            showA = !showA;

            cutsceneImage.rectTransform.anchoredPosition =
                originalPos + Random.insideUnitCircle * jitterAmount;

            yield return new WaitForSecondsRealtime(d.flickerInterval);
        }

        cutsceneImage.rectTransform.anchoredPosition = originalPos;
    }

    #endregion

    #region TEXT

    IEnumerator PlayTextTimeline()
    {
        foreach (var t in textScreens)
        {
            yield return new WaitForSecondsRealtime(t.startDelay);
            yield return FadeText(t);
        }
    }

    IEnumerator FadeText(TextScreen t)
    {
        cutsceneText.text = t.text;

        for (float i = 0; i < t.fadeInTime; i += Time.unscaledDeltaTime)
        {
            SetTextAlpha(i / t.fadeInTime);
            yield return null;
        }

        SetTextAlpha(1f);
        yield return new WaitForSecondsRealtime(t.stayTime);

        for (float i = 0; i < t.fadeOutTime; i += Time.unscaledDeltaTime)
        {
            SetTextAlpha(1f - i / t.fadeOutTime);
            yield return null;
        }

        SetTextAlpha(0f);
    }

    #endregion

    #region FADES

    IEnumerator FadeImageIn()
    {
        float t = 0f;
        while (t < imageFadeInTime)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(cutsceneImage, t / imageFadeInTime);
            yield return null;
        }

        SetAlpha(cutsceneImage, 1f);
    }

    IEnumerator FadeImageOut()
    {
        float startAlpha = cutsceneImage.color.a;
        float t = 0f;

        while (t < imageFadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / imageFadeOutTime);

            // Slow at first, fast at end
            float eased = Mathf.SmoothStep(0f, 1f, n);

            float a = Mathf.Lerp(startAlpha, 0f, eased);
            SetAlpha(cutsceneImage, a);

            yield return null;
        }

        SetAlpha(cutsceneImage, 0f);
    }

    IEnumerator FadeBlackOut()
    {
        float t = 0f;
        while (t < blackFadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(fadeOverlay, 1f - t / blackFadeOutTime);
            yield return null;
        }

        SetAlpha(fadeOverlay, 0f);
        fadeOverlay.gameObject.SetActive(false);
    }

    #endregion

    #region HELPERS

    void SetAlpha(Image img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    void SetTextAlpha(float a)
    {
        var c = cutsceneText.color;
        c.a = a;
        cutsceneText.color = c;
    }

    #endregion
}
