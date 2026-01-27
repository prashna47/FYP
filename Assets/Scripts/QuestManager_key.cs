using UnityEngine;
using System.Collections;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    public float hintDelay = 30f;

    [Header("Arrow")]
    public CanvasGroup arrowGroup;
    public ScreenDirectionArrow arrow;

    [Header("Explore Objective")]
    public Transform[] arrowTargets;   // where the arrow points
    public Transform[] triggerPoints; // where player must reach
    private int exploreIndex;

    [Header("Objectives")]
    public Transform keyTarget;
    public Transform doorTarget;
    public DoorInteractable door;

    [Header("Player")]
    public Transform player; // 🔥 IMPORTANT

    private int currentObjective = 0;
    private float timer;
    private bool hintShown;
    private bool completingObjective;
    private bool questFinished;

    void Start()
    {
        DisableArrowHard();
        StartKeyObjective();
    }

    void Update()
    {
        if (questFinished || completingObjective)
            return;

        timer += Time.deltaTime;

        if (!hintShown && timer >= hintDelay)
        {
            ShowArrow();
        }

        // ================= OBJECTIVE CHECKS =================

        // 0 — Find Key
        if (currentObjective == 0 && GameState.HasKey)
        {
            completingObjective = true;
            CompleteObjective(StartDoorObjective);
        }

        // 1 — Open Door
        else if (currentObjective == 1 && door.IsOpen)
        {
            completingObjective = true;
            CompleteObjective(StartExploreObjective);
        }

        // 2 — Explore Further
        else if (currentObjective == 2)
        {
            float distance = Vector3.Distance(
                player.position, // ✅ FIX
                triggerPoints[exploreIndex].position
            );

            if (distance < 2f)
            {
                AdvanceExplorePoint();
            }
        }
    }

    // ================= EXPLORE LOGIC =================

    void AdvanceExplorePoint()
    {
        completingObjective = true; // lock for this frame

        exploreIndex++;

        if (exploreIndex >= arrowTargets.Length)
        {
            questFinished = true;
            CompleteObjective(null);
        }
        else
        {
            arrow.target = arrowTargets[exploreIndex];
            ShowArrow(); // instant redirect
            completingObjective = false;
        }
    }

    // ================= OBJECTIVES =================

    void StartKeyObjective()
    {
        currentObjective = 0;
        ResetTimer();

        QuestUI.Instance.ShowObjective("Find the key");

        arrow.target = keyTarget;
        DisableArrowHard();
        completingObjective = false;
    }

    void StartDoorObjective()
    {
        currentObjective = 1;
        ResetTimer();

        QuestUI.Instance.ShowObjective("Open the door");

        arrow.target = doorTarget;
        DisableArrowHard();
        completingObjective = false;
    }

    void StartExploreObjective()
    {
        // Safety check
        if (arrowTargets.Length == 0 ||
            arrowTargets.Length != triggerPoints.Length)
        {
            Debug.LogError("Explore objective arrays are not set correctly.");
            questFinished = true;
            return;
        }

        currentObjective = 2;
        exploreIndex = 0;
        ResetTimer();

        QuestUI.Instance.ShowObjective("Explore further");

        arrow.target = arrowTargets[exploreIndex];
        DisableArrowHard(); // wait hintDelay
        completingObjective = false;
    }

    void CompleteObjective(System.Action nextObjective)
    {
        DisableArrowHard();
        StartCoroutine(CompleteAndAdvance(nextObjective));
    }

    IEnumerator CompleteAndAdvance(System.Action nextObjective)
    {
        QuestUI.Instance.PlayObjectiveComplete();

        while (QuestUI.Instance.IsAnimating)
            yield return null;

        yield return null; // safety frame

        if (nextObjective != null)
            nextObjective.Invoke();
    }

    // ================= ARROW CONTROL =================

    void ShowArrow()
    {
        hintShown = true;

        arrow.enabled = true;
        arrowGroup.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeArrow(1f));
    }

    void DisableArrowHard()
    {
        hintShown = false;

        StopAllCoroutines();

        if (arrow)
            arrow.enabled = false;

        if (arrowGroup)
        {
            arrowGroup.alpha = 0f;
            arrowGroup.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeArrow(float targetAlpha)
    {
        while (!Mathf.Approximately(arrowGroup.alpha, targetAlpha))
        {
            arrowGroup.alpha = Mathf.MoveTowards(
                arrowGroup.alpha,
                targetAlpha,
                Time.deltaTime * 4f
            );
            yield return null;
        }
    }

    void ResetTimer()
    {
        timer = 0f;
        hintShown = false;
    }
}
