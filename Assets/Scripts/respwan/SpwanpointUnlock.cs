using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SpawnPointUnlockUI : MonoBehaviour
{
    [Header("Toast Panel")]
    public GameObject panel;

    [Header("Message Text")]
    public TMP_Text messageText;

    [Header("Timing")]
    public float displayDuration = 3f;
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine showRoutine;

    private void Awake()
    {
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(string spawnLabel)
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowRoutine(spawnLabel));
    }

    private IEnumerator ShowRoutine(string spawnLabel)
    {
        messageText.text = $"Respwan point Unlocked";

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}