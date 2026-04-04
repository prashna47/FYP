using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.15f;

    public Vector3 ShakeOffset { get; private set; }

    private Coroutine shakeCoroutine;

    void Awake()
    {
        Instance = this;
    }

    public void Shake()
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(shakeMagnitude, 0f, elapsed / shakeDuration);
            ShakeOffset = (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }
        ShakeOffset = Vector3.zero;
    }
}