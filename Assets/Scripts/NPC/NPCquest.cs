using UnityEngine;
using UnityEngine.AI;

public class NPCQuestController : MonoBehaviour
{
    [Header("NPC Movement")]
    public Transform walkTarget;          // Where NPC should walk
    public float walkSpeed = 3.5f;

    [Header("Quest Trigger")]
    public int triggerObjectiveIndex = 0; // Objective index after which NPC starts walking
    public int talkObjectiveIndex = 1;    // Objective index for talking to NPC

    [Header("Player Interaction")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    public CanvasGroup interactPromptGroup;

    [Header("Dialogue")]
    public NPCDialogueTypewriterFade dialogueScript;

    private NavMeshAgent agent;
    private bool walkingTriggered = false;
    private bool playerInRange = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.enabled = false; // Start disabled
    }

    void Update()
    {
        HandleWalkTrigger();
        HandlePlayerInteraction();
    }

    void HandleWalkTrigger()
    {
        if (walkingTriggered) return;

        // Start walking after the specified objective is complete
        if (QuestManager.Instance.CurrentObjectiveIndex > triggerObjectiveIndex)
        {
            walkingTriggered = true;
            StartWalking();
        }
    }

    void StartWalking()
    {
        if (walkTarget == null) return;

        agent.enabled = true;
        agent.SetDestination(walkTarget.position);
    }

    void HandlePlayerInteraction()
    {
        if (!playerInRange) return;

        // Only allow interaction when current objective is the talk-to-NPC objective
        if (QuestManager.Instance.CurrentObjectiveIndex != talkObjectiveIndex) return;

        if (Input.GetKeyDown(interactKey))
        {
            // Trigger dialogue
            if (dialogueScript != null)
                dialogueScript.StartDialogue();

            // Mark objective as complete
            QuestManager.Instance.TriggerReached();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;

        if (QuestManager.Instance.CurrentObjectiveIndex == talkObjectiveIndex)
        {
            // Show prompt
            if (interactPromptGroup != null)
                interactPromptGroup.alpha = 1f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;

        // Hide prompt
        if (interactPromptGroup != null)
            interactPromptGroup.alpha = 0f;
    }

    void FixedUpdate()
    {
        // Stop NavMeshAgent when close to target
        if (agent.enabled && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.enabled = false;
            }
        }
    }
}