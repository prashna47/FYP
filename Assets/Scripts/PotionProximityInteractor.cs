using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class ProximityPotionInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptOverride = "Press [E] to Drink Potion";

    [Header("Effect Duration")]
    public float effectDuration = 1.5f;

    [Header("Effect Color")]
    public Color effectColor = new Color(0f, 1f, 0.3f, 1f);

    [Header("Particle Count")]
    public int maxParticles = 60;
    public float emissionRate = 40f;

    [Header("Particle Size")]
    public float startSize = 0.15f;

    [Header("Particle Speed")]
    public float startSpeed = 2.5f;

    [Header("Particle Lifetime")]
    public float startLifetime = 1.2f;

    [Header("Gravity (negative = float up)")]
    public float gravityModifier = -0.3f;

    [Header("Spawn Radius Around Player")]
    public float spawnRadius = 0.6f;

    PlayerProximityInteractor playerInside;

    public string Prompt => string.IsNullOrEmpty(promptOverride) ? "Press [E] to Drink Potion" : promptOverride;

    void Reset()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        var rb = gameObject.GetOrAddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null && playerInside == null)
        {
            playerInside = interactor;
            playerInside.Register(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null)
        {
            interactor.Unregister(this);
            playerInside = null;
        }
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        PlayerHealth health = interactor.GetComponent<PlayerHealth>();
        if (health != null)
            health.HealToFull();

        interactor.Unregister(this);

        StartCoroutine(SpawnGreenEffect(interactor.transform));

        gameObject.SetActive(false);
    }

    IEnumerator SpawnGreenEffect(Transform target)
    {
        GameObject fxObj = new GameObject("PotionEffect");
        fxObj.transform.position = target.position;
        fxObj.transform.SetParent(target);

        ParticleSystem ps = fxObj.AddComponent<ParticleSystem>();
        ps.Stop();

        var main = ps.main;
        main.duration = effectDuration;
        main.loop = false;
        main.startLifetime = startLifetime;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.startColor = effectColor;
        main.maxParticles = maxParticles;
        main.gravityModifier = gravityModifier;

        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spawnRadius;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(effectColor, 0f),
                new GradientColorKey(effectColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = effectColor;

        ps.Play();

        yield return new WaitForSeconds(effectDuration + 1f);
        Destroy(fxObj);
    }
}