using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathEffectsController : MonoBehaviour
{
    public static DeathEffectsController Instance;

    [Header("Post Processing")]
    public Volume globalVolume;

    [Header("Slow Motion")]
    public float slowMotionScale = 0.2f;
    public float slowMotionRampTime = 0.5f;

    [Header("Camera Zoom")]
    public camera cameraScript;
    public Vector3 deathZoomOffset = new Vector3(0, 6, -6);
    public float zoomSpeed = 1.5f;

    [Header("Vignette")]
    public float targetVignetteIntensity = 0.5f;
    public Color vignetteColor = new Color(0.6f, 0f, 0f);

    [Header("Desaturation")]
    public float targetDesaturation = -80f;

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private Vector3 originalOffset;
    private Coroutine effectRoutine;
    private Coroutine resetRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (cameraScript != null)
            originalOffset = cameraScript.offset;

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out colorAdjustments);
        }
    }

    public void TriggerDeathEffects()
    {
        if (resetRoutine != null) StopCoroutine(resetRoutine);
        if (effectRoutine != null) StopCoroutine(effectRoutine);
        effectRoutine = StartCoroutine(DeathEffectRoutine());
    }

    public void ResetEffects()
    {
        if (effectRoutine != null) StopCoroutine(effectRoutine);
        if (resetRoutine != null) StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(ResetEffectRoutine());
    }

    IEnumerator DeathEffectRoutine()
    {
        // Set vignette color
        if (vignette != null)
            vignette.color.Override(vignetteColor);

        float elapsed = 0f;

        while (elapsed < slowMotionRampTime)
        {
            float t = elapsed / slowMotionRampTime;

            // Slow motion ramp
            Time.timeScale = Mathf.Lerp(1f, slowMotionScale, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            // Camera zoom — use unscaled so it works in slow mo
            if (cameraScript != null)
                cameraScript.offset = Vector3.Lerp(originalOffset, deathZoomOffset, t);

            // Vignette fade in
            if (vignette != null)
                vignette.intensity.Override(Mathf.Lerp(0f, targetVignetteIntensity, t));

            // Desaturate
            if (colorAdjustments != null)
                colorAdjustments.saturation.Override(Mathf.Lerp(0f, targetDesaturation, t));

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Lock in final values
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        if (vignette != null) vignette.intensity.Override(targetVignetteIntensity);
        if (colorAdjustments != null) colorAdjustments.saturation.Override(targetDesaturation);
        if (cameraScript != null) cameraScript.offset = deathZoomOffset;
    }

    IEnumerator ResetEffectRoutine()
    {
        float elapsed = 0f;
        float resetDuration = 0.3f;

        float startTimeScale = Time.timeScale;
        float startVignette = vignette != null ? vignette.intensity.value : 0f;
        float startSaturation = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
        Vector3 startOffset = cameraScript != null ? cameraScript.offset : originalOffset;

        while (elapsed < resetDuration)
        {
            float t = elapsed / resetDuration;

            Time.timeScale = Mathf.Lerp(startTimeScale, 1f, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            if (cameraScript != null)
                cameraScript.offset = Vector3.Lerp(startOffset, originalOffset, t);

            if (vignette != null)
                vignette.intensity.Override(Mathf.Lerp(startVignette, 0f, t));

            if (colorAdjustments != null)
                colorAdjustments.saturation.Override(Mathf.Lerp(startSaturation, 0f, t));

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Clean reset
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (vignette != null) vignette.intensity.Override(0f);
        if (colorAdjustments != null) colorAdjustments.saturation.Override(0f);
        if (cameraScript != null) cameraScript.offset = originalOffset;
    }
}