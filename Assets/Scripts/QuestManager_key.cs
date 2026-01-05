using UnityEngine;
using System.Collections;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    public float hintDelay = 30f;

    [Header("Arrow")]
    public CanvasGroup arrowGroup;
    public ScreenDirectionArrow arrow;

    [Header("Objectives")]
    public Transform keyTarget;
    public Transform doorTarget;
    public DoorInteractable door;

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

        if (currentObjective == 0 && GameState.HasKey)
        {
            completingObjective = true;
            CompleteObjective(StartDoorObjective);
        }
        else if (currentObjective == 1 && door.IsOpen)
        {
            completingObjective = true;
            questFinished = true;
            CompleteObjective(null);
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
            arrow.enabled = false; // 🔴 stops ALL movement updates

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
