using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class MimicHealthBar : MonoBehaviour
{
    [Header("Bar References")]
    public Image fillImage;
    public Image ghostFill;

    [Header("Fade")]
    public float fadeInSpeed = 10f;
    public float fadeOutSpeed = 3f;

    [Header("Ghost Bar")]
    public float ghostDelay = 0.45f;
    public float ghostDrainSpeed = 2.2f;

    [Header("Hit Flash")]
    [Tooltip("Color the fill briefly flashes to when hit (white looks good)")]
    public Color hitFlashColor = Color.white;
    [Tooltip("How quickly the flash fades back to the normal health color")]
    public float hitFlashDuration = 0.12f;

    // ── private ──────────────────────────────────────────────────────────────
    private CanvasGroup canvasGroup;

    private float targetAlpha = 0f;
    private float currentFill = 1f;
    private float targetFill = 1f;
    private float ghostFillAmt = 1f;
    private float ghostTimer = 0f;

    // flash state — driven in Update, no coroutine needed
    private float flashTimer = 0f;

    private bool isDying = false;

    private Coroutine deathRoutine;

    // =========================================================================

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (ghostFill != null)
        {
            ghostFill.fillAmount = 1f;
            ghostFill.color = Color.white;   // amber ghost
        }
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = Color.red;
        }
    }

    void Update()
    {
        if (isDying) return;

        // ── smooth fill + hit flash ──────────────────────────────────────────
        if (fillImage != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 10f);
            fillImage.fillAmount = currentFill;

            // Flash white on hit, then lerp back to the green→red health colour
            Color healthColor = Color.Lerp(Color.yellow, Color.red, currentFill);
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                float t = flashTimer / hitFlashDuration;           // 1→0 as flash fades
                fillImage.color = Color.Lerp(healthColor, hitFlashColor, t);
            }
            else
            {
                fillImage.color = healthColor;
            }
        }

        // ── ghost bar ────────────────────────────────────────────────────────
        if (ghostFill != null)
        {
            if (ghostTimer > 0f)
                ghostTimer -= Time.deltaTime;
            else
                ghostFillAmt = Mathf.Lerp(ghostFillAmt, targetFill, Time.deltaTime * ghostDrainSpeed);

            ghostFill.fillAmount = ghostFillAmt;
        }

        // ── alpha fade ───────────────────────────────────────────────────────
        float speed = targetAlpha > canvasGroup.alpha ? fadeInSpeed : fadeOutSpeed;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * speed);
    }

    // =========================================================================
    //  Public API
    // =========================================================================

    /// <summary>Fade in the bar (player enters range or Mimic is alerted).</summary>
    public void Show()
    {
        if (isDying) return;
        targetAlpha = 1f;
    }

    public void ResetBar()
    {
        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = null;
        isDying = false;

        currentFill = 1f;
        targetFill = 1f;
        ghostFillAmt = 1f;
        ghostTimer = 0f;
        flashTimer = 0f;
        targetAlpha = 0f;      // hidden until combat starts

        if (fillImage != null) fillImage.fillAmount = 1f;
        if (ghostFill != null) ghostFill.fillAmount = 1f;
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        gameObject.SetActive(true);  // re-enable after DeathFade disabled it
    }

    /// <summary>Fade out the bar (Mimic goes back to wander).</summary>
    public void Hide()
    {
        if (isDying) return;
        targetAlpha = 0f;
    }

    /// <summary>Called every time the Mimic takes a hit. Updates fill and flashes the bar.</summary>
    public void ShowHit(int current, int max)
    {
        if (isDying) return;

        targetFill = Mathf.Clamp01((float)current / max);
        ghostTimer = ghostDelay;
        targetAlpha = 1f;
        flashTimer = hitFlashDuration;   // triggers the white flash in Update
    }

    /// <summary>Called by MimicEnemy.Die() — drains bar to zero then fades out.</summary>
    public void PlayDeathAnimation()
    {
        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = StartCoroutine(DeathFade());
    }

    // =========================================================================
    //  Coroutines
    // =========================================================================

    IEnumerator DeathFade()
    {
        isDying = true;

        // Drain fill to 0 over 0.3 s
        float t = 0f, startFill = currentFill, startGhost = ghostFillAmt;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float p = t / 0.3f;
            currentFill = Mathf.Lerp(startFill, 0f, p);
            ghostFillAmt = Mathf.Lerp(startGhost, 0f, p);
            if (fillImage != null) fillImage.fillAmount = currentFill;
            if (ghostFill != null) ghostFill.fillAmount = ghostFillAmt;
            yield return null;
        }

        // Fade the whole bar out
        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Hide but don't destroy — it's a persistent scene object
        gameObject.SetActive(false);
    }
}