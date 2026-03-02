using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

public class CutsceneScript : MonoBehaviour
{
    [Header("AUTO")]
    public bool playOnSceneLoad = false;
    public CutsceneScript nextCutscene; // optional chain

    [Header("UI")]
    public GameObject cutsceneCanvas;
    public Image fadeOverlay;
    public Image cutsceneImage;
    public TextMeshProUGUI cutsceneText;

    [Header("Data")]
    public Drawing[] drawings;
    public TextScreen[] textScreens;

    [Header("Timing")]
    public float blackHoldTime = 1f;
    public float blackFadeOutTime = 1f;
    public float imageFadeInTime = 0.3f;
    public float imageFadeOutTime = 1.5f;

    [Header("Motion")]
    public float jitterAmount = 2f;

    Vector2 originalPos;
    Coroutine textRoutine;
    bool allowFlicker = true;

    void Start()
    {
        originalPos = cutsceneImage.rectTransform.anchoredPosition;

        // Play automatically only if enabled
        if (playOnSceneLoad)
            Play();
    }

    public void Play()
    {
        if (cutsceneCanvas == null)
        {
            Debug.LogError("CutsceneCanvas is not assigned!");
            return;
        }

        
            // Activate the cutscene GameObject (in case it was inactive)
            gameObject.SetActive(true);

            // Activate canvas
            cutsceneCanvas.SetActive(true);
            fadeOverlay.gameObject.SetActive(true);
            cutsceneImage.gameObject.SetActive(true);
            cutsceneText.gameObject.SetActive(true);

            // Reset everything
            SetAlpha(fadeOverlay, 1f);
            SetAlpha(cutsceneImage, 0f);
            SetTextAlpha(0f);
            cutsceneImage.rectTransform.anchoredPosition = originalPos;
            allowFlicker = true;

            StartCoroutine(RunCutscene());
        
    }

    IEnumerator RunCutscene()
    {
        // Freeze game (player, objectives, etc.)
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(blackHoldTime);

        Coroutine drawingsRoutine = null;
        if (drawings.Length > 0)
            drawingsRoutine = StartCoroutine(PlayAllDrawings());

        if (textScreens.Length > 0)
            textRoutine = StartCoroutine(PlayTextTimeline());

        yield return FadeImageIn();
        yield return FadeBlackOut();

        if (drawingsRoutine != null)
            yield return drawingsRoutine;

        if (textRoutine != null)
            StopCoroutine(textRoutine);

        SetTextAlpha(0f);

        if (drawings.Length > 0)
        {
            Drawing lastDrawing = drawings[drawings.Length - 1];
            Coroutine flickerRoutine = StartCoroutine(FlickerLastDrawing(lastDrawing));

            yield return FadeImageOut();

            allowFlicker = false;
            StopCoroutine(flickerRoutine);
        }

        fadeOverlay.gameObject.SetActive(false);
        cutsceneImage.gameObject.SetActive(false);
        cutsceneText.gameObject.SetActive(false);

        cutsceneCanvas.SetActive(false);

        // Resume game AFTER all cutscene visuals
        Time.timeScale = 1f;

        // Play next cutscene if chained
        if (nextCutscene != null)
            nextCutscene.Play();
    }

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
        float t = 0f;
        while (t < imageFadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / imageFadeOutTime);
            SetAlpha(cutsceneImage, 1f - eased);
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
}