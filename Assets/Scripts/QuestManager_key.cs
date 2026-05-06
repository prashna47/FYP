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

    [Header("Multi Enemy Arrow")]
    public GameObject enemyArrowPrefab;
    public Canvas arrowCanvas;

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

    List<MultiEnemyArrow> activeEnemyArrows = new List<MultiEnemyArrow>();

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
        while (currentObjectiveIndex < objectives.Length && objectives[currentObjectiveIndex].skipObjective)
            currentObjectiveIndex++;

        if (currentObjectiveIndex >= objectives.Length)
        {
            questFinished = true;
            return;
        }

        if (completeRoutine != null) StopCoroutine(completeRoutine);
        completeRoutine = StartCoroutine(StartObjectiveRoutine());
    }

    IEnumerator StartObjectiveRoutine()
    {
        Objective obj = CurrentObjective;

        QuestUI.Instance.HideImmediate();

        yield return StartCoroutine(PlayDialogueSequence(obj.startSequence));

        ResetTimer();
        DisableArrowHard();
        ClearEnemyArrows();

        currentStepIndex = 0;
        UpdateArrowTarget();

        QuestUI.Instance.ShowObjective(obj.objectiveName);
        completingObjective = false;

        UpdateInteractablesForObjective(currentObjectiveIndex);
    }

    void UpdateArrowTarget()
    {
        Objective obj = CurrentObjective;

        if (obj.type == ObjectiveType.DefeatEnemy)
        {
            arrow.enabled = false;
            arrowGroup.gameObject.SetActive(false);
        }

        if (obj.type == ObjectiveType.DefeatEnemy && obj.targetEnemies != null && obj.targetEnemies.Length > 0)
        {
            SpawnEnemyArrows(obj.targetEnemies);
            return;
        }

        if (obj.arrowTargets != null && obj.arrowTargets.Length > currentStepIndex)
            arrow.target = obj.arrowTargets[currentStepIndex];
        else
            arrow.target = obj.arrowTarget;
    }

    void SpawnEnemyArrows(Enemy[] enemies)
    {
        ClearEnemyArrows();

        if (enemyArrowPrefab == null || arrowCanvas == null) return;

        foreach (Enemy e in enemies)
        {
            if (e == null) continue;

            GameObject go = Instantiate(enemyArrowPrefab, arrowCanvas.transform);
            MultiEnemyArrow mea = go.GetComponent<MultiEnemyArrow>();
            if (mea != null)
            {
                mea.SetTarget(e);
                activeEnemyArrows.Add(mea);
            }
        }
    }

    void ClearEnemyArrows()
    {
        foreach (var a in activeEnemyArrows)
        {
            if (a != null) Destroy(a.gameObject);
        }
        activeEnemyArrows.Clear();
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

            case ObjectiveType.DefeatEnemy:
                completed = AllEnemiesDead(obj.targetEnemies);
                break;

            // ✅ NEW
            case ObjectiveType.DrinkPotion:
                if (obj.potion != null) completed = !obj.potion.gameObject.activeSelf;
                break;

            case ObjectiveType.ReachLocation:
            case ObjectiveType.Sleep:
                return;
        }

        if (completed)
        {
            completingObjective = true;
            CompleteObjective(obj.pointsReward);
        }
    }

    bool AllEnemiesDead(Enemy[] enemies)
    {
        if (enemies == null || enemies.Length == 0) return false;

        foreach (Enemy e in enemies)
        {
            if (e != null) return false;
        }
        return true;
    }

    public bool IsCorrectTrigger(int objectiveIndex, int stepIndex)
    {
        return currentObjectiveIndex == objectiveIndex &&
               currentStepIndex == stepIndex;
    }

    public void TriggerReached()
    {
        if (questFinished || completingObjective) return;

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
        ClearEnemyArrows();

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

        NPCQuestController npc = FindObjectOfType<NPCQuestController>();
        if (npc != null)
            npc.OnObjectiveCompleted(currentObjectiveIndex);

        // ✅ ADD THIS LINE — notifies AppearOnObjectiveNPC (and any future listener)
        AppearOnObjectiveNPC.OnQuestObjectiveCompleted?.Invoke(currentObjectiveIndex);

        // ✅ NEW — show name screen after completion dialogues if flagged
        if (justCompleted.showNameScreenAfterComplete && PlayerNameUI.Instance != null)
            yield return PlayerNameUI.Instance.ShowAndWait();

        // Teleport the player if this objective has a QuestTeleport assigned
        if (justCompleted.questTeleport != null)
        {
            justCompleted.questTeleport.Execute();
            yield return new WaitUntil(() => !GameState.IsPlayerFrozen);
        }

        // ✅ NEW — show choice screen if flagged
        if (justCompleted.showChoiceScreen && PlayerChoiceUI.Instance != null)
        {
            yield return PlayerChoiceUI.Instance.ShowAndWait(
                justCompleted.choicePrompt,
                justCompleted.choiceLabelA,
                justCompleted.choiceLabelB
            );

            // If they picked Head Out, fire the choice teleport
            if (PlayerChoiceUI.Instance.ChoseHeadOut && justCompleted.choiceTeleport != null)
            {
                justCompleted.choiceTeleport.Execute();
                yield return new WaitUntil(() => !GameState.IsPlayerFrozen);
            }
        }

        currentObjectiveIndex++;

        if (currentObjectiveIndex >= objectives.Length)
        {
            questFinished = true;
            yield break;
        }

        StartObjective();
    }

    IEnumerator PlayDialogueSequence(DialogueEntry[] sequence)
    {
        if (sequence == null || sequence.Length == 0)
            yield break;

        foreach (var entry in sequence)
        {
            if (entry == null || entry.lines == null || entry.lines.Length == 0)
                continue;

            if (entry.triggerDistortion && ScreenDistortionController.Instance != null)
                ScreenDistortionController.Instance.TriggerDistortion();

            if (entry.speaker == SpeakerType.Player)
                ObjectiveDialogueUI.Instance.ShowDialogue(entry.lines, true);
            else
                ObjectiveDialogueUI.Instance.ShowDialogue(entry.lines, false, entry.npcPortrait, entry.npcName);

            while (!ObjectiveDialogueUI.Instance.IsFinished)
                yield return null;
        }
    }

    void ShowArrow()
    {
        hintShown = true;

        if (CurrentObjective.type != ObjectiveType.DefeatEnemy)
        {
            arrow.enabled = true;
            arrowGroup.gameObject.SetActive(true);

            if (arrowFadeRoutine != null) StopCoroutine(arrowFadeRoutine);
            arrowFadeRoutine = StartCoroutine(FadeArrow(1f));
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

        ClearEnemyArrows();
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

    [Tooltip("Tick to skip this objective entirely")]
    public bool skipObjective;

    public Transform arrowTarget;
    public Transform[] arrowTargets;

    public DoorInteractable door;
    public Transform[] triggerPoints;

    [Tooltip("Drag all enemies here for DefeatEnemy objectives")]
    public Enemy[] targetEnemies;

    // ✅ NEW
    [Tooltip("Drag your potion GameObject here for DrinkPotion objectives")]
    public ProximityPotionInteractable potion;

    [Tooltip("Tick this on the objective after which the name screen should appear")]
    public bool showNameScreenAfterComplete;

    public int pointsReward;

    [Header("Cutscene (Optional)")]
    public CutsceneScript cutscene;

    [Header("Teleport After Completion (Optional)")]
    public QuestTeleport questTeleport;

    [Header("START DIALOGUE")]
    public DialogueEntry[] startSequence;

    [Header("COMPLETION DIALOGUE")]
    public DialogueEntry[] completionSequence;

    [Header("Choice Screen (Optional)")]
    public bool showChoiceScreen;
    public string choicePrompt = "What would you like to do?";
    public string choiceLabelA = "Head Out";
    public string choiceLabelB = "Stay and Explore";
    public QuestTeleport choiceTeleport; // used if player picks Head Out
}

public enum ObjectiveType
{
    CollectKey,
    OpenDoor,
    ReachLocation,
    Sleep,
    DefeatEnemy,
    DrinkPotion  // ✅ NEW
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
    public bool triggerDistortion;
}