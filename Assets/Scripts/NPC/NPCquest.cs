using UnityEngine;

public class NPCQuestController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Quest Settings")]
    public int walkAfterObjectiveIndex = 2;
    public int talkObjectiveIndex = 3;

    [Header("Interaction")]
    public CanvasGroup interactPromptGroup;
    public string playerTag = "Player";

    // ✅ NEW
    [Header("Despawn")]
    public float despawnDelay = 3f;

    private Transform player;
    private bool isWalking = false;
    private bool hasArrived = false;
    private bool talkObjectiveCompleted = false;
    private Quaternion fixedRotation;

    void Start()
    {
        fixedRotation = transform.rotation;

        if (interactPromptGroup != null)
            interactPromptGroup.alpha = 0f;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (GameState.IsPaused || GameState.IsPlayerFrozen) return;
        HandleMovement();
    }

    public void OnObjectiveCompleted(int completedIndex)
    {
        if (completedIndex == walkAfterObjectiveIndex)
        {
            StartCoroutine(StartWalk());
        }
    }

    System.Collections.IEnumerator StartWalk()
    {
        yield return new WaitForSeconds(0.2f);

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;

        isWalking = true;
        hasArrived = false;

        PlayerControlLock.MovementLocked = true;
        InteractionLock.DialoguePlaying = true;
    }

    void HandleMovement()
    {
        if (!isWalking || hasArrived || player == null)
        {
            if (animator != null)
                animator.SetFloat("Speed", 0f);
            return;
        }

        Vector3 direction = (player.position - transform.position);
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.rotation = fixedRotation;

            if (animator != null)
            {
                animator.SetFloat("MoveX", moveDir.x);
                animator.SetFloat("MoveY", moveDir.z);
                animator.SetFloat("Speed", moveDir.magnitude);
            }
        }
        else
        {
            isWalking = false;
            hasArrived = true;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            StartCoroutine(ArriveAndWaitForDialogue());
        }

        if (hasArrived && player != null && animator != null)
        {
            Vector3 lookDir = (player.position - transform.position).normalized;
            animator.SetFloat("MoveX", lookDir.x);
            animator.SetFloat("MoveY", lookDir.z);
        }
    }

    System.Collections.IEnumerator ArriveAndWaitForDialogue()
    {
        CompleteTalkObjective();

        yield return null;
        yield return null;

        if (ObjectiveDialogueUI.Instance != null)
        {
            while (!ObjectiveDialogueUI.Instance.IsFinished)
                yield return null;
        }

        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;

        // ✅ NEW — start despawn timer after dialogue ends
        StartCoroutine(DespawnAfterDelay());
    }

    // ✅ NEW
    System.Collections.IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);
        gameObject.SetActive(false);
    }

    void CompleteTalkObjective()
    {
        if (talkObjectiveCompleted) return;
        talkObjectiveCompleted = true;

        if (QuestManager.Instance.CurrentObjectiveIndex == talkObjectiveIndex)
        {
            QuestManager.Instance.TriggerReached();

            if (interactPromptGroup != null)
                interactPromptGroup.alpha = 0f;
        }
    }
}