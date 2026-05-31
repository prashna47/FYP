using System.Collections;
using UnityEngine;
using TMPro;

public class RespawnUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Text")]
    public TMP_Text messageText;
    public CanvasGroup textCanvasGroup;

    [Header("Timing")]
    public float textFadeInDuration = 0.8f;
    public float countdownSeconds = 3f;

    private Coroutine countdownRoutine;

    private void Start()
    {
        panel.SetActive(false);
        if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;
    }

    public void Show()
    {
        panel.SetActive(true);
        BigCursorChangeTrigger.UIOverride = true;
        Cursor.visible = true;

        DeathEffectsController.Instance?.TriggerDeathEffects();

        if (countdownRoutine != null) StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    public void Hide()
    {
        panel.SetActive(false);
        BigCursorChangeTrigger.UIOverride = false;
        Cursor.visible = false;

        DeathEffectsController.Instance?.ResetEffects();
    }

    IEnumerator CountdownRoutine()
    {
        // Fade in text first
        if (textCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < textFadeInDuration)
            {
                textCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeInDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            textCanvasGroup.alpha = 1f;
        }

        // Then countdown
        float remaining = countdownSeconds;
        while (remaining > 0f)
        {
            messageText.text = $"Respawning in {Mathf.CeilToInt(remaining)}s";
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        messageText.text = "Respawning...";
        RespawnManager.Instance.Respawn();
    }
}