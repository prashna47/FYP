using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Fixed screen-space health bar for the Mimic enemy.
///
/// SETUP:
///  1. In a Screen Space – Overlay Canvas, build this hierarchy:
///
///       MimicHealthBar          ← this script + CanvasGroup
///        └── Background         ← Image (dark, e.g. 300×18 px)
///             ├── GhostFill     ← Image, Fill Method=Horizontal, Source Image=white sprite, Color = white 55% alpha
///             ├── Fill          ← Image, Fill Method=Horizontal, Source Image=white sprite
///             └── Label         ← Text / TextMeshProUGUI  "MIMIC" (optional)
///
///  2. Assign Fill → fillImage and GhostFill → ghostFill in the Inspector.
///  3. Position MimicHealthBar's RectTransform wherever you want on screen.
///     It will NOT move.
///  4. Drag this scene GameObject into MimicEnemy → healthBar slot.
///
/// MimicEnemy calls Show() / Hide() / ShowHit() / PlayDeathAnimation() automatically.
/// </summary>
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

    [Header("Hit Punch")]
    public float punchScale = 1.14f;
    public float punchDuration = 0.09f;

    // ── private ──────────────────────────────────────────────────────────────
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private float targetAlpha = 0f;
    private float currentFill = 1f;
    private float targetFill = 1f;
    private float ghostFillAmt = 1f;
    private float ghostTimer = 0f;

    private bool isDying = false;

    private Coroutine punchRoutine;
    private Coroutine deathRoutine;

    // =========================================================================

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (ghostFill != null)
        {
            ghostFill.fillAmount = 1f;
            ghostFill.color = new Color(1f, 0.85f, 0.3f, 0.55f);   // amber ghost
        }
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = Color.green;
        }
    }

    void Update()
    {
        if (isDying) return;

        // ── smooth fill ──────────────────────────────────────────────────────
        if (fillImage != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 10f);
            fillImage.fillAmount = currentFill;
            fillImage.color = Color.Lerp(Color.red, Color.green, currentFill);
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

    /// <summary>Fade out the bar (Mimic goes back to wander).</summary>
    public void Hide()
    {
        if (isDying) return;
        targetAlpha = 0f;
    }

    /// <summary>Called every time the Mimic takes a hit. Updates fill and punches the bar.</summary>
    public void ShowHit(int current, int max)
    {
        if (isDying) return;

        targetFill = Mathf.Clamp01((float)current / max);
        ghostTimer = ghostDelay;
        targetAlpha = 1f;   // always make sure bar is visible on hit

        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(PunchScale());
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

    IEnumerator PunchScale()
    {
        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(punchScale, 1f, t / punchDuration);
            rectTransform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        rectTransform.localScale = Vector3.one;
    }

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