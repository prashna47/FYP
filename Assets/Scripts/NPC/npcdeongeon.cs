using UnityEngine;
using System.Collections;

/// <summary>
/// Attach this to an NPC that should:
///   1. Start disabled (inactive GameObject)
///   2. Appear (re-enable) when a specific objective index completes  (appearAfterObjectiveIndex)
///   3. Register as interactable via SphereCollider trigger
///   4. On E press — complete objective B (completeOnObjectiveIndex) via TriggerReached()
///   5. Stay in scene afterward (no despawn)
/// 
/// SETUP:
///   - Disable the NPC's GameObject in the Inspector (it will enable itself)
///   - Add a SphereCollider set to isTrigger on this GameObject
///   - Assign this component to the NPC
///   - Wire appearAfterObjectiveIndex and completeOnObjectiveIndex in the Inspector
///   - In QuestManager, call AppearOnObjectiveNPC.Instance.NotifyObjectiveCompleted(index)
///     inside CompleteAndAdvance — or use the static event hook below (recommended)
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class AppearOnObjectiveNPC : MonoBehaviour, IInteractable
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Trigger Indices")]
    [Tooltip("The objective index whose completion makes this NPC appear.")]
    public int appearAfterObjectiveIndex = 0;

    [Tooltip("The objective index this NPC completes when the player presses E.")]
    public int completeOnObjectiveIndex = 1;

    [Header("Interaction")]
    public string promptText = "Press [E] to Talk";

    [Header("Proximity")]
    [Tooltip("Radius of the SphereCollider trigger.")]
    public float interactRadius = 2f;

    // ── IInteractable ────────────────────────────────────────────────────────
    public string Prompt => promptText;

    // ── Private state ────────────────────────────────────────────────────────
    PlayerProximityInteractor playerInside;
    bool objectiveCompleted = false;

    // ── Static event — QuestManager calls this instead of being modified ─────
    /// <summary>
    /// Call this from QuestManager.CompleteAndAdvance after NPCQuestController.OnObjectiveCompleted.
    /// e.g.:  AppearOnObjectiveNPC.OnQuestObjectiveCompleted?.Invoke(currentObjectiveIndex);
    /// </summary>
    public static System.Action<int> OnQuestObjectiveCompleted;

    // ────────────────────────────────────────────────────────────────────────
    void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactRadius;
    }

    void Awake()
    {
        // Ensure collider matches inspector radius
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactRadius;
    }

    void OnEnable()
    {
        OnQuestObjectiveCompleted += HandleObjectiveCompleted;
    }

    void OnDisable()
    {
        OnQuestObjectiveCompleted -= HandleObjectiveCompleted;
    }

    // ── Appear logic ─────────────────────────────────────────────────────────
    void HandleObjectiveCompleted(int completedIndex)
    {
        if (completedIndex == appearAfterObjectiveIndex && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);   // instant pop-in
        }
    }

    // ── Proximity trigger ────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (objectiveCompleted) return;   // already done — no prompt

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null)
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

    // ── E press ──────────────────────────────────────────────────────────────
    public void Interact(PlayerProximityInteractor interactor)
    {
        // Guard: only fire when it's the right objective
        if (QuestManager.Instance.CurrentObjectiveIndex != completeOnObjectiveIndex) return;
        if (objectiveCompleted) return;

        objectiveCompleted = true;

        // Remove prompt — NPC stays but won't be interactable again
        interactor.Unregister(this);
        playerInside = null;

        // This completes the objective and plays the completion dialogue
        // defined on the objective inside QuestManager
        QuestManager.Instance.TriggerReached();
    }
}