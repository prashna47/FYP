using UnityEngine;
using System.Collections;

public class NPCQuestController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Quest Settings")]
    public int walkAfterObjectiveIndex = 2;
    public int talkObjectiveIndex = 9; // FIXED USAGE

    [Header("Interaction")]
    public CanvasGroup interactPromptGroup;
    public string playerTag = "Player";

    [Header("Despawn")]
    public float despawnDelay = 5f;

    private Transform player;
    private bool isWalking = false;
    private bool hasArrived = false;

    private Quaternion fixedRotation;

    // 🔥 DESPAWN CONTROL
    private bool talkTriggered;
    private bool despawnQueued;

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

        // 🎯 mark TALK objective
        if (completedIndex == talkObjectiveIndex)
        {
            talkTriggered = true;
            return;
        }

        // 🎯 NEXT objective after talk triggers despawn
        if (talkTriggered && completedIndex > talkObjectiveIndex)
        {
            StartDespawnTimer();
        }
    }

    IEnumerator StartWalk()
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

    IEnumerator ArriveAndWaitForDialogue()
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

        InteractionLock.NpcInRange = false;
    }

    // 🔥 SAFE DESPAWN START
    void StartDespawnTimer()
    {
        if (despawnQueued) return;

        despawnQueued = true;
        StartCoroutine(DelayedFadeOut());
    }

    // 🔥 FADE + DESPAWN
    IEnumerator DelayedFadeOut()
    {
        yield return new WaitForSeconds(despawnDelay);

        float t = 0f;
        float duration = 1f;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / duration);

            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                }
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    void CompleteTalkObjective()
    {
        if (QuestManager.Instance.CurrentObjectiveIndex == talkObjectiveIndex)
        {
            QuestManager.Instance.TriggerReached();

            if (interactPromptGroup != null)
                interactPromptGroup.alpha = 0f;
        }
    }
}