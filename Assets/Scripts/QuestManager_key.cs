using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Settings")]
    public float hintDelay = 30f;

    public bool QuestStarted { get; private set; } = false;

    [Header("Arrow")]
    public CanvasGroup arrowGroup;
    public ScreenDirectionArrow arrow;

    [Header("Player")]
    public Transform player;

    [Header("Objectives")]
    public Objective[] objectives;

    [Header("Interactables")]
    public DoorInteractable doorInteractable;
    public ProximityBookInteractable bookInteractable;
    public BedInteractable bedInteractable;

    public int CurrentObjectiveIndex => currentObjectiveIndex;

    int currentObjectiveIndex = 0;
    int currentStepIndex = 0;

    float timer;
    bool hintShown;
    bool completingObjective;
    bool questFinished;

    Coroutine arrowFadeRoutine;
    Coroutine completeRoutine;

    Objective CurrentObjective => objectives[currentObjectiveIndex];

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initially: door active, book and bed hidden
        if (doorInteractable) doorInteractable.gameObject.SetActive(true);
        if (bookInteractable) bookInteractable.gameObject.SetActive(false);
        if (bedInteractable) bedInteractable.gameObject.SetActive(false);
    }

    void Update()
    {
        if (questFinished || completingObjective) return;

        timer += Time.deltaTime;
        if (!hintShown && timer >= hintDelay) ShowArrow();

        CheckObjective();
    }

    public void BeginQuest()
    {
        if (objectives.Length == 0 || QuestStarted) return;

        QuestStarted = true;
        currentObjectiveIndex = 0;
        currentStepIndex = 0;
        StartObjective();
    }

    public void ShowCurrentObjective()
    {
        if (objectives.Length == 0 || questFinished) return;

        Objective obj = CurrentObjective;
        QuestUI.Instance.ShowObjective(obj.objectiveName);

        UpdateArrowTarget();
        ResetTimer();
        ShowArrow();
    }

    void StartObjective()
    {
        Objective obj = CurrentObjective;

        ResetTimer();
        DisableArrowHard();

        currentStepIndex = 0;
        UpdateArrowTarget();

        QuestUI.Instance.ShowObjective(obj.objectiveName);
        completingObjective = false;

        UpdateInteractablesForObjective(currentObjectiveIndex);
    }

    void UpdateArrowTarget()
    {
        Objective obj = CurrentObjective;
        if (obj.arrowTargets != null && obj.arrowTargets.Length > currentStepIndex)
            arrow.target = obj.arrowTargets[currentStepIndex];
        else
            arrow.target = obj.arrowTarget;
    }

    void CheckObjective()
    {
        Objective obj = CurrentObjective;

        bool completed = false;

        switch (obj.type)
        {
            case ObjectiveType.CollectKey:
                completed = GameState.HasKey;
                break;

            case ObjectiveType.OpenDoor:
                if (obj.door != null) completed = obj.door.IsOpen;
                break;

            case ObjectiveType.ReachLocation:
                // handled manually for triggers
                return;
        }

        if (completed)
        {
            completingObjective = true;
            CompleteObjective(obj.pointsReward);
        }
    }

    public bool IsCorrectTrigger(int objectiveIndex, int stepIndex)
    {
        return currentObjectiveIndex == objectiveIndex &&
               currentStepIndex == stepIndex;
    }

    public void TriggerReached()
    {
        if (questFinished || completingObjective) return;

        AdvanceStep();
    }

    void AdvanceStep()
    {
        Objective obj = CurrentObjective;
        currentStepIndex++;

        if (obj.triggerPoints == null || currentStepIndex >= obj.triggerPoints.Length)
        {
            completingObjective = true;
            CompleteObjective(obj.pointsReward);
        }
        else
        {
            completingObjective = false;
            UpdateArrowTarget();
            ResetTimer();
            ShowArrow();
        }
    }

    void CompleteObjective(int points)
    {
        DisableArrowHard();

        if (completeRoutine != null) StopCoroutine(completeRoutine);
        completeRoutine = StartCoroutine(CompleteAndAdvance(points));
    }

    IEnumerator CompleteAndAdvance(int points)
    {
        QuestUI.Instance.PlayObjectiveComplete();

        while (QuestUI.Instance.IsAnimating) yield return null;

        StoryProgress.Instance.AddPointsSmooth(points);

        currentObjectiveIndex++;

        if (currentObjectiveIndex >= objectives.Length)
        {
            questFinished = true;
            yield break;
        }

        StartObjective();
    }

    void ShowArrow()
    {
        hintShown = true;
        arrow.enabled = true;
        arrowGroup.gameObject.SetActive(true);

        if (arrowFadeRoutine != null) StopCoroutine(arrowFadeRoutine);
        arrowFadeRoutine = StartCoroutine(FadeArrow(1f));
    }

    void DisableArrowHard()
    {
        hintShown = false;

        if (arrowFadeRoutine != null) StopCoroutine(arrowFadeRoutine);

        if (arrow) arrow.enabled = false;

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

    /// <summary>
    /// 🔹 Updates the visibility of door/book/bed based on objective index.
    /// Only one interactable shows its prompt depending on which is closest to the player.
    /// </summary>
    void UpdateInteractablesForObjective(int objectiveIndex)
    {
        // Enable only the correct interactables
        if (doorInteractable) doorInteractable.gameObject.SetActive(objectiveIndex < 4);
        if (bookInteractable) bookInteractable.gameObject.SetActive(objectiveIndex == 4);
        if (bedInteractable) bedInteractable.gameObject.SetActive(objectiveIndex == 5);
    }
}

public enum ObjectiveType
{
    CollectKey,
    OpenDoor,
    ReachLocation
}

[System.Serializable]
public class Objective
{
    public string objectiveName;
    public ObjectiveType type;
    public Transform arrowTarget;
    public Transform[] arrowTargets;
    public DoorInteractable door;
    public Transform[] triggerPoints;
    public int pointsReward;
}