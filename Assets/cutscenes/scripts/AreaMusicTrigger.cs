using System.Collections;
using UnityEngine;

public class AreaMusicTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource music;
    public string playerTag = "Player";

    [Header("Timing")]
    public float startDelay = 2f;
    public float fadeInTime = 1.5f;
    public float fadeOutTime = 1.5f;

    Collider trigger;
    Transform player;

    Coroutine delayRoutine;
    Coroutine fadeRoutine;

    void Awake()
    {
        trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        player = other.transform;
        StartMusicWithDelay();
    }

    void Update()
    {
        if (player == null)
            return;

        // If player left trigger bounds
        if (!trigger.bounds.Contains(player.position))
        {
            player = null;
            StartFadeOut();
        }
    }

    public void StartMusicWithDelay()
    {
        if (music == null)
        {
            Debug.LogError("AreaMusicTrigger: AudioSource not assigned!");
            return;
        }

        if (delayRoutine != null)
            StopCoroutine(delayRoutine);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        delayRoutine = StartCoroutine(DelayedFadeIn());
    }

    IEnumerator DelayedFadeIn()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        if (!music.isPlaying)
        {
            music.volume = 0f;
            music.Play();
        }

        fadeRoutine = StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            music.volume = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }

        music.volume = 1f;
    }

    void StartFadeOut()
    {
        if (delayRoutine != null)
        {
            StopCoroutine(delayRoutine);
            delayRoutine = null;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = music.volume;
        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            music.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutTime);
            yield return null;
        }

        music.volume = 0f;
        music.Stop();
    }
}
