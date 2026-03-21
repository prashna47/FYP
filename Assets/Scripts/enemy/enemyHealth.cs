using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public RectTransform barRoot;
    public Image fillImage;          // Colored HP bar (front)
    public Image ghostFill;          // White trailing bar (behind fillImage)
    public Image backgroundImage;

    [Header("Settings")]
    public Vector3 worldOffset = new Vector3(0, 2.2f, 0);
    public float fadeInSpeed = 6f;
    public float fadeOutSpeed = 3f;
    public float hideDelay = 2.5f;

    [Header("Ghost Bar")]
    public float ghostDelay = 0.4f;      // Seconds before ghost starts draining
    public float ghostDrainSpeed = 2.5f; // How fast ghost catches up

    [Header("Bar Punch on Hit")]
    public float punchScale = 1.15f;
    public float punchDuration = 0.12f;

    private CanvasGroup canvasGroup;
    private Camera mainCam;
    private Transform trackedTarget;

    private float targetAlpha = 0f;
    private float currentFill = 1f;     // What the colored bar shows
    private float targetFill = 1f;      // Where colored bar is heading
    private float ghostFillAmount = 1f; // What the ghost bar shows
    private float ghostTimer = 0f;      // Countdown before ghost drains

    private float hideTimer = 0f;
    private bool isDying = false;
    private Coroutine punchCoroutine;
    private Coroutine deathCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        mainCam = Camera.main;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Ghost bar starts full, white/light grey
        if (ghostFill != null)
        {
            ghostFill.fillAmount = 1f;
            ghostFill.color = new Color(1f, 1f, 1f, 0.6f);
        }
    }

    public void SetTarget(Transform enemy)
    {
        trackedTarget = enemy;
    }

    public void ShowHit(int current, int max)
    {
        if (isDying) return;

        targetFill = Mathf.Clamp01((float)current / max);

        // Reset ghost delay — ghost won't move until the player stops hitting
        ghostTimer = ghostDelay;

        targetAlpha = 1f;
        hideTimer = hideDelay;

        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchScale());
    }

    public void PlayDeathAnimation()
    {
        if (deathCoroutine != null) StopCoroutine(deathCoroutine);
        deathCoroutine = StartCoroutine(DeathFade());
    }

    IEnumerator PunchScale()
    {
        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(punchScale, 1f, t / punchDuration);
            barRoot.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        barRoot.localScale = Vector3.one;
    }

    IEnumerator DeathFade()
    {
        isDying = true;

        // Snap colored bar to 0, drain ghost quickly
        float t = 0f;
        float startFill = currentFill;
        float startGhost = ghostFillAmount;

        while (t < 0.35f)
        {
            t += Time.deltaTime;
            float progress = t / 0.35f;
            currentFill = Mathf.Lerp(startFill, 0f, progress);
            ghostFillAmount = Mathf.Lerp(startGhost, 0f, progress);
            fillImage.fillAmount = currentFill;
            if (ghostFill != null) ghostFill.fillAmount = ghostFillAmount;
            yield return null;
        }

        // Fade out whole bar
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (isDying) return;
        if (trackedTarget == null) { Destroy(gameObject); return; }

        // === Positioning ===
        Vector3 worldPos = trackedTarget.position + worldOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        barRoot.position = screenPos;

        // === Colored bar — snaps ahead fast ===
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 12f);
        fillImage.fillAmount = currentFill;

        // Gradient color: green → yellow → red
        fillImage.color = Color.Lerp(Color.red, Color.green, currentFill);

        // === Ghost bar — waits, then drains slowly ===
        if (ghostFill != null)
        {
            if (ghostTimer > 0f)
            {
                ghostTimer -= Time.deltaTime;
            }
            else
            {
                // Ghost chases the colored bar from above
                ghostFillAmount = Mathf.Lerp(ghostFillAmount, targetFill, Time.deltaTime * ghostDrainSpeed);
                ghostFill.fillAmount = ghostFillAmount;
            }
        }

        // === Visibility ===
        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            targetAlpha = 1f;
        }
        else
        {
            targetAlpha = 0f;
        }

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha,
            Time.deltaTime * (targetAlpha > 0.5f ? fadeInSpeed : fadeOutSpeed));
    }
}
