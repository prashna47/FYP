using UnityEngine;

public class NPCQuestController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;

    [Header("Quest Settings")]
    public int walkAfterObjectiveIndex = 2; // after this objective, NPC starts walking
    public int talkObjectiveIndex = 3;      // this objective is auto-completed when NPC arrives

    [Header("Interaction")]
    public CanvasGroup interactPromptGroup;
    public string playerTag = "Player";

    private Transform player;
    private bool isWalking = false;
    private bool hasArrived = false;
    private bool talkObjectiveCompleted = false;

    private Quaternion fixedRotation; // store initial rotation

    void Start()
    {
        // Store the NPC's initial rotation
        fixedRotation = transform.rotation;

        if (interactPromptGroup != null)
            interactPromptGroup.alpha = 0f;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        HandleMovement();
    }

    // ---------------- CALLED FROM QUEST MANAGER ----------------

    public void OnObjectiveCompleted(int completedIndex)
    {
        if (completedIndex == walkAfterObjectiveIndex)
        {
            StartCoroutine(StartWalk());
        }
    }

    System.Collections.IEnumerator StartWalk()
    {
        yield return new WaitForSeconds(0.2f); // small delay after dialogue
        isWalking = true;
        hasArrived = false;
    }

    // ---------------- MOVE TO PLAYER ----------------

    void HandleMovement()
    {
        if (!isWalking || hasArrived || player == null) return;

        Vector3 direction = (player.position - transform.position);
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;

            // Keep fixed rotation
            transform.rotation = fixedRotation;
        }
        else
        {
            // NPC reached player
            isWalking = false;
            hasArrived = true;

            // Auto-complete talk objective
            CompleteTalkObjective();
        }
    }

    void CompleteTalkObjective()
    {
        if (talkObjectiveCompleted) return;
        talkObjectiveCompleted = true;

        if (QuestManager.Instance.CurrentObjectiveIndex == talkObjectiveIndex)
        {
            QuestManager.Instance.TriggerReached();

            // Hide prompt if any
            if (interactPromptGroup != null)
                interactPromptGroup.alpha = 0f;
        }
    }
}