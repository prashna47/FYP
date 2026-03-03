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
    public Transform[] arrowTargets;
    public Transform[] triggerPoints;
    private int exploreIndex;

    [Header("Objectives")]
    public Transform keyTarget;
    public Transform doorTarget;
    public DoorInteractable door;

    [Header("Player")]
    public Transform player;

    private int currentObjective = 0;
    private float timer;
    private bool hintShown;
    private bool completingObjective;
    private bool questFinished;

    public static QuestManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        DisableArrowHard();
        // ✅ Do NOT start key objective here — let cutscene handle UI fade
    }

    void Update()
    {
        if (questFinished || completingObjective)
            return;

        timer += Time.deltaTime;

        if (!hintShown && timer >= hintDelay)
            ShowArrow();

        // ================= OBJECTIVE CHECKS =================

        if (currentObjective == 0 && GameState.HasKey)
        {
            completingObjective = true;
            CompleteObjective(StartDoorObjective, 10);
        }
        else if (currentObjective == 1 && door.IsOpen)
        {
            completingObjective = true;
            CompleteObjective(StartExploreObjective, 10);
        }
        else if (currentObjective == 2)
        {
            float distance = Vector3.Distance(player.position, triggerPoints[exploreIndex].position);
            if (distance < 2f)
                AdvanceExplorePoint();
        }
    }

    // ================= EXPLORE LOGIC =================

    void AdvanceExplorePoint()
    {
        completingObjective = true;
        exploreIndex++;

        if (exploreIndex >= arrowTargets.Length)
        {
            questFinished = true;
            CompleteObjective(null, 10);
        }
        else
        {
            arrow.target = arrowTargets[exploreIndex];
            ShowArrow();
            completingObjective = false;
        }
    }

    // ================= OBJECTIVES =================

    public void StartKeyObjective()
    {
        currentObjective = 0;
        ResetTimer();

        arrow.target = keyTarget;
        DisableArrowHard();
        completingObjective = false;

        // ✅ Show objective like before
        QuestUI.Instance.ShowObjective("Find the key");
    }

    void StartDoorObjective()
    {
        currentObjective = 1;
        ResetTimer();

        arrow.target = doorTarget;
        DisableArrowHard();
        completingObjective = false;

        QuestUI.Instance.ShowObjective("Open the door");
    }

    void StartExploreObjective()
    {
        if (arrowTargets.Length == 0 || arrowTargets.Length != triggerPoints.Length)
        {
            Debug.LogError("Explore objective arrays are not set correctly.");
            questFinished = true;
            return;
        }

        currentObjective = 2;
        exploreIndex = 0;
        ResetTimer();

        arrow.target = arrowTargets[exploreIndex];
        DisableArrowHard();
        completingObjective = false;

        QuestUI.Instance.ShowObjective("Explore further");
    }

    void CompleteObjective(System.Action nextObjective, int pointsForObjective)
    {
        DisableArrowHard();
        StartCoroutine(CompleteAndAdvance(nextObjective, pointsForObjective));
    }

    IEnumerator CompleteAndAdvance(System.Action nextObjective, int pointsForObjective)
    {
        QuestUI.Instance.PlayObjectiveComplete();

        while (QuestUI.Instance.IsAnimating)
            yield return null;

        yield return null;

        // ✅ Add points for story progress
        StoryProgress.Instance.AddPointsSmooth(pointsForObjective);

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
            arrowGroup.alpha = Mathf.MoveTowards(arrowGroup.alpha, targetAlpha, Time.deltaTime * 4f);
            yield return null;
        }
    }

    void ResetTimer()
    {
        timer = 0f;
        hintShown = false;
    }
}