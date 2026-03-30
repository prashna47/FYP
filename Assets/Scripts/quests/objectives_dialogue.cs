using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveDialogueUI : MonoBehaviour
{
    public static ObjectiveDialogueUI Instance;

    [Header("UI")]
    public CanvasGroup dialogBoxGroup;
    public TMP_Text dialogText;

    [Header("UI Blocking")]
    public GameObject otherUIRoot;
    public CanvasGroup promptGroup; // drag PlayerProximityInteractor's promptGroup here

    [Header("Player Animation")]
    public Animator playerAnimator;

    [Header("Timing")]
    public float fadeDuration = 0.25f;
    public float switchDelay = 0.5f;

    [Header("Typewriter")]
    public float charsPerSecond = 45f;

    [Header("Skip Control")]
    public float minSkipDelay = 1f;

    public bool IsFinished { get; private set; } = true;

    float lineStartTime;
    bool advanceLock;
    bool dialogOpen;
    int lineIndex;
    bool isTyping;
    string currentLineFull = "";
    string[] currentLines;

    Coroutine dialogFadeRoutine;
    Coroutine typingRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitGroup(dialogBoxGroup, visible: false);
        if (dialogText) dialogText.text = "";
    }

    void Update()
    {
        if (!dialogOpen) return;

        UpdateAdvanceLock();

        if (Time.time - lineStartTime < minSkipDelay) return;

        bool advancePressed = AdvancePressed();
        if (!advancePressed) return;

        advanceLock = true;

        if (isTyping)
            FinishCurrentLineInstant();
        else
            NextLine();
    }

    public void ShowDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        IsFinished = false;
        currentLines = lines;
        lineIndex = 0;
        dialogOpen = true;

        StartCoroutine(OpenThenType());
    }

    IEnumerator OpenThenType()
    {
        // Hide all prompts and other UI
        if (otherUIRoot) otherUIRoot.SetActive(false);
        if (promptGroup)
        {
            promptGroup.alpha = 0f;
            promptGroup.blocksRaycasts = false;
        }

        // Lock player
        PlayerControlLock.MovementLocked = true;
        InteractionLock.DialoguePlaying = true;

        // Set animator to idle
        if (playerAnimator) playerAnimator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(switchDelay);
        FadeIn(dialogBoxGroup, ref dialogFadeRoutine);
        yield return new WaitForSeconds(fadeDuration);
        StartTypingLine(currentLines[lineIndex]);
    }

    void NextLine()
    {
        lineIndex++;

        if (currentLines == null || lineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        StartTypingLine(currentLines[lineIndex]);
    }

    void EndDialogue()
    {
        dialogOpen = false;

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        isTyping = false;

        FadeOut(dialogBoxGroup, ref dialogFadeRoutine);

        StartCoroutine(MarkFinishedAfterFade());
    }

    IEnumerator MarkFinishedAfterFade()
    {
        yield return new WaitForSeconds(fadeDuration);

        // Restore UI
        if (otherUIRoot) otherUIRoot.SetActive(true);
        if (promptGroup) promptGroup.blocksRaycasts = true;

        // Unlock player
        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;

        IsFinished = true;
    }

    void StartTypingLine(string line)
    {
        if (!dialogText) return;
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        currentLineFull = line ?? "";
        lineStartTime = Time.time;
        advanceLock = true;

        typingRoutine = StartCoroutine(TypeLine(currentLineFull));
    }

    IEnumerator TypeLine(string fullLine)
    {
        isTyping = true;
        dialogText.text = "";

        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        foreach (char c in fullLine)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
    }

    void FinishCurrentLineInstant()
    {
        if (!dialogText) return;
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = null;
        dialogText.text = currentLineFull;
        isTyping = false;
    }

    bool AdvancePressed()
    {
        if (advanceLock) return false;
        return Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetMouseButtonDown(0);
    }

    void UpdateAdvanceLock()
    {
        if (!advanceLock) return;
        bool stillHeld = Input.GetKey(KeyCode.Return)
            || Input.GetKey(KeyCode.KeypadEnter)
            || Input.GetMouseButton(0);
        if (!stillHeld) advanceLock = false;
    }

    static void InitGroup(CanvasGroup g, bool visible)
    {
        if (!g) return;
        g.alpha = visible ? 1f : 0f;
        g.interactable = visible;
        g.blocksRaycasts = visible;
    }

    void FadeIn(CanvasGroup g, ref Coroutine routine)
    {
        if (!g) return;
        if (routine != null) StopCoroutine(routine);
        g.gameObject.SetActive(true);
        routine = StartCoroutine(FadeCanvasGroup(g, g.alpha, 1f, fadeDuration));
    }

    void FadeOut(CanvasGroup g, ref Coroutine routine)
    {
        if (!g) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeCanvasGroup(g, g.alpha, 0f, fadeDuration));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup g, float from, float to, float duration)
    {
        g.interactable = to > from;
        g.blocksRaycasts = to > from;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        g.alpha = to;
    }
}