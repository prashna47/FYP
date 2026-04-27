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

    private Transform player;
    private bool isWalking = false;
    private bool hasArrived = false;
    private bool talkObjectiveCompleted = false;
    private Quaternion fixedRotation;

    // ✅ Reference to the player's movement script — assign in Inspector
    // Replace "PlayerMovement" with whatever your actual movement script is called
    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;

    void Start()
    {
        fixedRotation = transform.rotation;

        if (interactPromptGroup != null)
            interactPromptGroup.alpha = 0f;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            player = p.transform;

            // ✅ Auto-find the movement script if not assigned in Inspector
            if (playerMovementScript == null)
                playerMovementScript = p.GetComponent<MonoBehaviour>();
        }
    }

    void Update()
    {
        
        if (GameState.IsPaused || GameState.IsPlayerFrozen) return;
        // rest of movement + sprite flip logic
       
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
        isWalking = true;
        hasArrived = false;

        // ✅ Freeze the player as NPC begins walking
        SetPlayerFrozen(true);
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

            // ✅ NPC has arrived — unfreeze the player
            SetPlayerFrozen(false);

            CompleteTalkObjective();
        }

        if (hasArrived && player != null && animator != null)
        {
            Vector3 lookDir = (player.position - transform.position).normalized;
            animator.SetFloat("MoveX", lookDir.x);
            animator.SetFloat("MoveY", lookDir.z);
        }
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

    // ✅ Freeze/unfreeze the player
    private void SetPlayerFrozen(bool frozen)
    {
        // Option A — disable/enable the movement script component entirely
        if (playerMovementScript != null)
            playerMovementScript.enabled = !frozen;

        // Option B — use GameState so your player script can check it
        // (use this if your player already checks GameState.IsPaused)
        // GameState.IsPlayerFrozen = frozen;
    }
}