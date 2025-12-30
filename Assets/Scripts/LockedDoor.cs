/*using System.Collections;
using TMPro;
using UnityEngine;

public class DoorProximityMessage : MonoBehaviour
{
    public CanvasGroup promptGroup;   // Parent with CanvasGroup
    public TMP_Text messageText;
    public string playerTag = "Player";

    public float fadeDuration = 0.5f;
    public float stayDuration = 3f;

    Coroutine fadeRoutine;

    private void Start()
    {
        promptGroup.alpha = 0f;
        promptGroup.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        promptGroup.gameObject.SetActive(true);
        messageText.text = "Door is Locked";

        fadeRoutine = StartCoroutine(FadeSequence());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOut());
    }

    IEnumerator FadeSequence()
    {
        // Fade in
        yield return Fade(0f, 1f);

        // Stay visible
        yield return new WaitForSeconds(stayDuration);

        // Fade out
        yield return Fade(1f, 0f);

        promptGroup.gameObject.SetActive(false);
    }

    IEnumerator FadeOut()
    {
        yield return Fade(promptGroup.alpha, 0f);
        promptGroup.gameObject.SetActive(false);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            promptGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        promptGroup.alpha = to;
    }
}
*/