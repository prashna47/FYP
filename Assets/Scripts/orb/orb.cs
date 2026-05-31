using UnityEngine;

public class QuestOrb : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptOverride = "Press [E] to Interact";

    [Header("Visuals")]
    public Light orbLight;
    public Renderer orbRenderer;
    public string emissionColorProperty = "_EmissionColor";
    public Color glowColor = new Color(0.3f, 0.8f, 1f);
    public float glowIntensity = 2f;

    [Header("Trigger Collider")]
    [Tooltip("A trigger collider on this GameObject — used to register the player")]
    public float triggerRadius = 2.5f;

    public string Prompt => string.IsNullOrEmpty(promptOverride) ? "Press [E] to Interact" : promptOverride;

    private bool isActive = false;
    private bool isCompleted = false;

    // ── IInteractable ────────────────────────────────────────────────────────

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (!isActive || isCompleted) return;

        isCompleted = true;
        interactor.Unregister(this);

        // Deactivate all sibling orbs in this objective via QuestManager
        QuestManager.Instance?.OrbInteracted();

        Deactivate();
    }
    void Start()
    {
        Deactivate();
    }

    // ── Proximity registration — same pattern as ProximityPotionInteractable ─

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || isCompleted) return;
        if (!other.CompareTag("Player")) return;

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        interactor?.Register(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        interactor?.Unregister(this);
    }

    // ── Activation ───────────────────────────────────────────────────────────

    public void Activate()
    {
        isActive = true;
        isCompleted = false;
        SetVisible(true);

        // Make sure the collider is on and is a trigger
        var col = GetComponent<SphereCollider>();
        if (col != null) col.enabled = true;
    }

    public void Deactivate()
    {
        isActive = false;
        SetVisible(false);

        // Disable collider so it stops registering the player
        var col = GetComponent<SphereCollider>();
        if (col != null) col.enabled = false;
    }

    // ── Visuals ──────────────────────────────────────────────────────────────

    void SetVisible(bool visible)
    {
        if (orbLight != null)
            orbLight.enabled = visible;

        if (orbRenderer != null)
        {
            orbRenderer.enabled = visible;

            if (visible)
            {
                orbRenderer.material.EnableKeyword("_EMISSION");
                orbRenderer.material.SetColor(emissionColorProperty, glowColor * glowIntensity);
            }
            else
            {
                orbRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }

    // ── Setup helper ─────────────────────────────────────────────────────────

    void Reset()
    {
        // Auto-add a trigger SphereCollider when component is first added
        if (GetComponent<SphereCollider>() == null)
        {
            var sc = gameObject.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = triggerRadius;
        }

        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var col = GetComponent<SphereCollider>();
        Gizmos.DrawWireSphere(transform.position, col != null ? col.radius : triggerRadius);
    }
}