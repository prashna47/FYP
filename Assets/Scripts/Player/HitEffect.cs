using UnityEngine;
using System.Collections;
using UnityEditor.Rendering.LookDev;

public class HitEffects : MonoBehaviour
{
    [Header("Sprite Flash")]
    public SpriteRenderer spriteRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.08f;
    public int flashCount = 3;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.15f;

    private Rigidbody rb;
    private Coroutine flashCoroutine;
    private Coroutine knockbackCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PlayHitEffects(Vector3 attackerPosition)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashSprite());

        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(DoKnockback(attackerPosition));

        CameraShake.Instance?.Shake();
    }

    IEnumerator FlashSprite()
    {
        Color original = Color.white;
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = original;
            yield return new WaitForSeconds(flashDuration);
        }
        spriteRenderer.color = original;
    }

    IEnumerator DoKnockback(Vector3 attackerPosition)
    {
        if (rb == null) yield break;

        Vector3 dir = (transform.position - attackerPosition).normalized;
        dir.y = 0f;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float force = Mathf.Lerp(knockbackForce, 0f, elapsed / knockbackDuration);
            rb.MovePosition(rb.position + dir * force * Time.deltaTime);
            yield return null;
        }
    }
}