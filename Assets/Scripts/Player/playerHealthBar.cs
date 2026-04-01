using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;           // Colored HP fill
    public Image ghostFill;           // White trailing ghost bar
    public Image barBackground;       // Dark background bar
    public CanvasGroup lowHPVignette; // Full-screen dark vignette for low HP pulse

    [Header("Ghost Bar")]
    public float ghostDelay = 0.45f;
    public float ghostDrainSpeed = 2f;

    [Header("Low HP")]
    [Range(0f, 1f)]
    public float lowHPThreshold = 0.3f;   // Below 30% triggers pulse
    public float pulseSpeed = 1.8f;
    public float pulseMinAlpha = 0.15f;
    public float pulseMaxAlpha = 0.55f;

    [Header("Colors")]
    public Color highHPColor = new Color(0.18f, 0.82f, 0.45f); // green
    public Color midHPColor = new Color(0.95f, 0.78f, 0.1f);  // yellow
    public Color lowHPColor = new Color(0.9f, 0.2f, 0.15f); // red

    private float currentFill = 1f;
    private float targetFill = 1f;
    private float ghostAmount = 1f;
    private float ghostTimer = 0f;
    private bool isLowHP = false;
    private float pulseT = 0f;

    void Awake()
    {
        if (ghostFill != null)
        {
            ghostFill.fillAmount = 1f;
            ghostFill.color = new Color(1f, 1f, 1f, 0.55f);
        }
        if (lowHPVignette != null)
        {
            lowHPVignette.alpha = 0f;
            lowHPVignette.blocksRaycasts = false;
            lowHPVignette.interactable = false;
        }

        SetFillColor(1f);
    }

    /// <summary>Call this from PlayerHealth whenever HP changes.</summary>
    public void OnHit(int current, int max)
    {
        targetFill = Mathf.Clamp01((float)current / max);
        ghostTimer = ghostDelay;  // Ghost waits before draining
        isLowHP = targetFill <= lowHPThreshold;
    }

    /// <summary>Call this from PlayerHealth on heal.</summary>
    public void OnHeal(int current, int max)
    {
        targetFill = Mathf.Clamp01((float)current / max);
        isLowHP = targetFill <= lowHPThreshold;

        // Ghost bar snaps up instantly on heal — no lag
        ghostAmount = targetFill;
        if (ghostFill != null) ghostFill.fillAmount = ghostAmount;
    }

    void Update()
    {
        // === Colored fill — fast lerp ===
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 10f);
        fillImage.fillAmount = currentFill;
        SetFillColor(currentFill);

        // === Ghost bar ===
        if (ghostFill != null)
        {
            if (ghostTimer > 0f)
            {
                ghostTimer -= Time.deltaTime;
            }
            else
            {
                ghostAmount = Mathf.Lerp(ghostAmount, targetFill, Time.deltaTime * ghostDrainSpeed);
                ghostFill.fillAmount = ghostAmount;
            }
        }

        // === Low HP vignette pulse ===
        if (lowHPVignette != null)
        {
            if (isLowHP)
            {
                pulseT += Time.deltaTime * pulseSpeed;
                // Ping-pong between min and max alpha
                float pingpong = Mathf.PingPong(pulseT, 1f);
                float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, pingpong);
                lowHPVignette.alpha = alpha;
            }
            else
            {
                // Fade vignette out when HP recovers
                lowHPVignette.alpha = Mathf.Lerp(lowHPVignette.alpha, 0f, Time.deltaTime * 4f);
                pulseT = 0f;
            }
        }
    }

    void SetFillColor(float fill)
    {
        Color c;
        if (fill > 0.5f)
            c = Color.Lerp(midHPColor, highHPColor, (fill - 0.5f) * 2f);
        else
            c = Color.Lerp(lowHPColor, midHPColor, fill * 2f);

        fillImage.color = c;
    }
}