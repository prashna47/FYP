using System.Collections;
using TMPro;
using UnityEngine;

public class NPCDialogueTypewriterFade : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup interactPromptGroup; // "Press E"
    public CanvasGroup dialogBoxGroup;      // dialog panel
    public TMP_Text dialogText;

    [Header("Skip Control")]
    public float minSkipDelay = 1f;

    float lineStartTime;
    bool advanceLock;

    [Header("Timing")]
    public float switchDelay = 1f;

    [Header("UI Blocking")]
    public GameObject otherUIRoot;

    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string[] lines;

    [Header("Player Animation")]
    public Animator playerAnimator;

    [Header("Settings")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;

    [Header("Fade")]
    public float fadeDuration = 0.25f;

    [Header("Typewriter")]
    [Tooltip("Characters revealed per second.")]
    public float charsPerSecond = 45f;

    bool playerInRange;
    bool dialogOpen;

    int lineIndex;
    bool isTyping;
    string currentLineFull = "";

    Coroutine promptFadeRoutine;
    Coroutine dialogFadeRoutine;
    Coroutine typingRoutine;

    void Start()
    {
        InitGroup(interactPromptGroup, visible: false);
        InitGroup(dialogBoxGroup, visible: false);

        if (dialogText) dialogText.text = "";
    }

    void Update()
    {
        if (!playerInRange && !dialogOpen) return;

        // Open dialog
        if (playerInRange && !dialogOpen && Input.GetKeyDown(interactKey))
        {
            StartDialogue();
            return;
        }

        if (!dialogOpen) return;

        UpdateAdvanceLock();

        if (!dialogOpen)
            return;

        // Do not allow skipping until minimum time has passed
        if (Time.time - lineStartTime < minSkipDelay)
            return;

        bool advancePressed = AdvancePressed();
        if (!advancePressed)
            return;

        // Lock immediately to prevent holding
        advanceLock = true;

        if (isTyping)
        {
            FinishCurrentLineInstant(); // ONLY skip typing
        }
        else
        {
            NextLine(); // requires a fresh press
        }

    }
    bool AdvancePressed()
    {
        if (advanceLock) return false;

        return Input.GetKeyDown(KeyCode.Return)      // main Enter
            || Input.GetKeyDown(KeyCode.KeypadEnter) // numpad Enter
            || Input.GetMouseButtonDown(0);          // left click
    }

    void UpdateAdvanceLock()
    {
        if (!advanceLock) return;

        bool stillHeld =
            Input.GetKey(KeyCode.Return) ||
            Input.GetKey(KeyCode.KeypadEnter) ||
            Input.GetMouseButton(0);

        if (!stillHeld)
            advanceLock = false;
    }



    void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;

        dialogOpen = true;
        lineIndex = 0;

        if (playerAnimator)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }


        // Lock player & other UI
        PlayerControlLock.MovementLocked = true;
        if (otherUIRoot) otherUIRoot.SetActive(false);

        // Stop any running fades
        if (promptFadeRoutine != null) StopCoroutine(promptFadeRoutine);
        if (dialogFadeRoutine != null) StopCoroutine(dialogFadeRoutine);

        // Fade OUT prompt, then wait, then fade IN dialog
        FadeOut(interactPromptGroup, ref promptFadeRoutine);
        StartCoroutine(DelayedDialogIn());
    }

    IEnumerator DelayedDialogIn()
    {
        yield return new WaitForSeconds(switchDelay);

        FadeIn(dialogBoxGroup, ref dialogFadeRoutine);
        StartTypingLine(lines[lineIndex]);
    }



    void NextLine()
    {
        lineIndex++;

        if (lines == null || lineIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        StartTypingLine(lines[lineIndex]);
    }

    void EndDialogue()
    {
        dialogOpen = false;

        // Stop typing
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        isTyping = false;

        // Stop fades
        if (promptFadeRoutine != null) StopCoroutine(promptFadeRoutine);
        if (dialogFadeRoutine != null) StopCoroutine(dialogFadeRoutine);

        // Fade OUT dialog, then wait, then show prompt
        FadeOut(dialogBoxGroup, ref dialogFadeRoutine);
        StartCoroutine(UnlockAfterDialogFade());

        StartCoroutine(DelayedPromptIn());

        // Unlock player & UI
        //PlayerControlLock.MovementLocked = false;
        //if (otherUIRoot) otherUIRoot.SetActive(true);

        

    }
    IEnumerator UnlockAfterDialogFade()
    {
        // Wait until the dialog finished fading out
        yield return new WaitForSeconds(fadeDuration);

        PlayerControlLock.MovementLocked = false;
        if (otherUIRoot) otherUIRoot.SetActive(true);
    }

    IEnumerator DelayedPromptIn()
    {
        yield return new WaitForSeconds(switchDelay);

        if (playerInRange)
            FadeIn(interactPromptGroup, ref promptFadeRoutine);
    }



    void StartTypingLine(string line)
    {
        if (!dialogText) return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        currentLineFull = line ?? "";
        lineStartTime = Time.time;   // record when line starts
        advanceLock = true;          // lock input initially

        typingRoutine = StartCoroutine(TypeLine(currentLineFull));
    }

    IEnumerator TypeLine(string fullLine)
    {
        isTyping = true;

        dialogText.text = "";
        int total = fullLine.Length;

        if (total == 0)
        {
            isTyping = false;
            yield break;
        }

        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        for (int i = 0; i < total; i++)
        {
            dialogText.text += fullLine[i];
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
    }

    void FinishCurrentLineInstant()
    {
        if (!dialogText) return;

        // Stop coroutine and show full line
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = null;

        dialogText.text = currentLineFull;
        isTyping = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;
        InteractionLock.NpcInRange = true;

        if (!dialogOpen)
            FadeIn(interactPromptGroup, ref promptFadeRoutine);
    }


    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        InteractionLock.NpcInRange = false;

        FadeOut(interactPromptGroup, ref promptFadeRoutine);

        if (dialogOpen)
            EndDialogue();
    }


    // ---------- Fade helpers ----------

    static void InitGroup(CanvasGroup g, bool visible)
    {
        if (!g) return;
        g.gameObject.SetActive(true); // keep active for smooth fades
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
        bool becomingVisible = to > from;

        // For dialog box, you typically want it interactable while visible
        g.interactable = becomingVisible;
        g.blocksRaycasts = becomingVisible;

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
