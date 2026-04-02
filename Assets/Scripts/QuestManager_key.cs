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
        if (completeRoutine != null) StopCoroutine(completeRoutine);
        completeRoutine = StartCoroutine(StartObjectiveRoutine());
    }

    IEnumerator StartObjectiveRoutine()
    {
        Objective obj = CurrentObjective;

        // Hide any leftover quest UI before start dialogue
        QuestUI.Instance.HideImmediate();

        // Show start dialogue before objective begins
        yield return StartCoroutine(PlayDialogueSequence(obj.startSequence));

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
            case ObjectiveType.Sleep:        // ← add this line
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

        // Only allow event-driven completion for these types
        Objective obj = CurrentObjective;
        if (obj.type != ObjectiveType.ReachLocation && obj.type != ObjectiveType.Sleep)
            return;

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

        yield return null;
        while (QuestUI.Instance.IsAnimating) yield return null;

        StoryProgress.Instance.AddPointsSmooth(points);

        Objective justCompleted = objectives[currentObjectiveIndex];
        yield return StartCoroutine(PlayDialogueSequence(justCompleted.completionSequence));

        if (justCompleted.cutscene != null)
        {
            bool done = false;
            justCompleted.cutscene.onCutsceneFinished = () => done = true;
            justCompleted.cutscene.Play();
            while (!done) yield return null;
        }

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

    IEnumerator PlayDialogueSequence(DialogueEntry[] sequence)
    {
        if (sequence == null || sequence.Length == 0)
            yield break;

        foreach (var entry in sequence)
        {
            if (entry == null || entry.lines == null || entry.lines.Length == 0)
                continue;

            if (entry.speaker == SpeakerType.Player)
            {
                ObjectiveDialogueUI.Instance.ShowDialogue(
                    entry.lines,
                    true
                );
            }
            else
            {
                ObjectiveDialogueUI.Instance.ShowDialogue(
                   entry.lines,
                   false,
                   entry.npcPortrait,
                   entry.npcName
               );
            }

            while (!ObjectiveDialogueUI.Instance.IsFinished)
                yield return null;
        }
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

    void UpdateInteractablesForObjective(int objectiveIndex)
    {
        if (bedInteractable != null)
            bedInteractable.SetInteractionEnabled(objectiveIndex >= 5);
    }
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

    [Header("Cutscene (Optional)")]
    public CutsceneScript cutscene;

    [Header("START DIALOGUE")]
    public DialogueEntry[] startSequence;

    [Header("COMPLETION DIALOGUE")]
    public DialogueEntry[] completionSequence;
}

public enum ObjectiveType
{
    CollectKey,
    OpenDoor,
    ReachLocation,
    Sleep          
}

public enum SpeakerType
{
    Player,
    NPC
}

[System.Serializable]
public class DialogueEntry
{
    public SpeakerType speaker;

    [Header("NPC Settings (only used if speaker = NPC)")]
    public string npcName;
    public Sprite npcPortrait;

    [TextArea(2, 4)]
    public string[] lines;
}

