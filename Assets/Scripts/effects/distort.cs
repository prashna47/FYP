using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ScreenDistortionController : MonoBehaviour
{
    public static ScreenDistortionController Instance;

    [Header("Volume")]
    public Volume volume;

    LensDistortion lens;
    ChromaticAberration chroma;
    Vignette vignette;

    [Header("Distortion Settings")]
    public float maxDistortion = 0.5f;
    public float pulseSpeed = 5f;
    public float vignetteBase = 0.2f;

    [Header("Timing")]
    public float defaultDuration = 2f;

    Coroutine routine;

    void Awake()
    {
        Instance = this;

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out lens);
            volume.profile.TryGet(out chroma);
            volume.profile.TryGet(out vignette);
        }

        ResetEffects();
    }

    public void TriggerDistortion(float duration = -1f)
    {
        if (routine != null)
            StopCoroutine(routine);

        if (duration <= 0f)
            duration = defaultDuration;

        routine = StartCoroutine(DistortionRoutine(duration));
    }

    IEnumerator DistortionRoutine(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float strength = Mathf.Sin(t * pulseSpeed) * maxDistortion;

            if (lens != null) lens.intensity.value = strength;
            if (chroma != null) chroma.intensity.value = Mathf.Abs(strength);
            if (vignette != null) vignette.intensity.value = vignetteBase + Mathf.Abs(strength);

            yield return null;
        }

        ResetEffects();
    }

    void ResetEffects()
    {
        if (lens != null) lens.intensity.value = 0f;
        if (chroma != null) chroma.intensity.value = 0f;
        if (vignette != null) vignette.intensity.value = vignetteBase;
    }
}