using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Image = UnityEngine.UI.Image;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public RectTransform barRoot;
    public Image fillImage;
    public Image ghostFill;

    [Header("Ghost Bar")]
    public float ghostDelay = 0.4f;
    public float ghostDrainSpeed = 2.5f;

    [Header("Bar Punch on Hit")]
    public float punchScale = 1.15f;
    public float punchDuration = 0.12f;

    private float currentFill = 1f;
    private float targetFill = 1f;
    private float ghostFillAmount = 1f;
    private float ghostTimer = 0f;

    private Coroutine punchCoroutine;

    void Awake()
    {
        if (ghostFill != null)
        {
            ghostFill.fillAmount = 1f;
            ghostFill.color = new Color(1f, 1f, 1f, 0.6f);
        }
    }

    public void UpdateHealth(int current, int max)
    {
        targetFill = Mathf.Clamp01((float)current / max);
        ghostTimer = ghostDelay;

        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchScale());
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

    void Update()
    {
        // Colored fill
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 12f);
        fillImage.fillAmount = currentFill;
        fillImage.color = Color.Lerp(Color.red, Color.green, currentFill);

        // Ghost bar
        if (ghostFill != null)
        {
            if (ghostTimer > 0f)
                ghostTimer -= Time.deltaTime;
            else
                ghostFillAmount = Mathf.Lerp(ghostFillAmount, targetFill, Time.deltaTime * ghostDrainSpeed);

            ghostFill.fillAmount = ghostFillAmount;
        }
    }
}