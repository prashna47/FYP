using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveDialogueUI : MonoBehaviour
{
    public static ObjectiveDialogueUI Instance;

    [Header("UI")]
    public CanvasGroup dialogBoxGroup;
    public TMP_Text dialogText;
    public float nextAllowedClickTime = 0f;

    [Header("Name UI")]
    public TMP_Text nameText;

    [Header("UI Blocking")]
    public GameObject otherUIRoot;
    public CanvasGroup promptGroup;

    [Header("Player Animation")]
    Animator playerAnimator;

    [Header("Character Portrait")]
    public UnityEngine.UI.Image portraitImage;
    public Sprite malePortrait;
    public Sprite femalePortrait;

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

        InitGroup(dialogBoxGroup, false);
        if (dialogText) dialogText.text = "";
    }

    void Update()
    {
        if (!dialogOpen) return;

        UpdateAdvanceLock();

        // ❌ HARD COOLDOWN (prevents spam completely)
        if (Time.time < nextAllowedClickTime) return;

        if (!AdvancePressed()) return;

        advanceLock = true;

        // Set next allowed click time (Genshin-style delay)
        nextAllowedClickTime = Time.time + minSkipDelay;

        if (isTyping)
            FinishCurrentLineInstant();
        else
            NextLine();
    }
    public void ShowDialogue(string[] lines, bool isPlayer, Sprite npcPortrait = null, string npcName = "")
    {
        StopAllCoroutines();
        if (lines == null || lines.Length == 0) return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        isTyping = false;

        if (dialogText != null)
            dialogText.text = "";

        // ✅ SET PORTRAIT (ONLY PLACE THIS HAPPENS)
        if (portraitImage != null)
        {
            if (isPlayer)
            {
                portraitImage.sprite = GameData.IsMale ? malePortrait : femalePortrait;
            }
            else
            {
                portraitImage.sprite = npcPortrait;
            }
        }

        // ✅ SET NAME
        if (nameText != null)
        {
            if (isPlayer)
                nameText.text = string.IsNullOrEmpty(GameData.PlayerName) ? "You" : GameData.PlayerName;
            else
                nameText.text = string.IsNullOrEmpty(npcName) ? "NPC" : npcName;
        }

        IsFinished = false;
        currentLines = lines;
        lineIndex = 0;
        dialogOpen = true;

        StartCoroutine(OpenThenType());
    }

    void GetPlayerAnimator()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerAnimator = player.GetComponent<Animator>();
    }

    IEnumerator OpenThenType()
    {
        if (otherUIRoot) otherUIRoot.SetActive(false);

        if (promptGroup)
        {
            promptGroup.alpha = 0f;
            promptGroup.blocksRaycasts = false;
        }

        PlayerControlLock.MovementLocked = true;
        InteractionLock.DialoguePlaying = true;

        GetPlayerAnimator();

        if (playerAnimator != null)
            playerAnimator.SetFloat("Speed", 0f);

        // ❌ IMPORTANT: DO NOT TOUCH portrait OR name here

        yield return new WaitForSeconds(switchDelay);

        if (dialogText != null)
            dialogText.text = "";

        FadeIn(dialogBoxGroup, ref dialogFadeRoutine);

        yield return new WaitForSeconds(fadeDuration);

        if (currentLines == null || lineIndex < 0 || lineIndex >= currentLines.Length)
        {
            EndDialogue();
            yield break;
        }

        StartTypingLine(currentLines[lineIndex]);
    }

    void NextLine()
    {
        if (currentLines == null) return;

        lineIndex++;

        if (lineIndex >= currentLines.Length)
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

        if (otherUIRoot) otherUIRoot.SetActive(true);
        if (promptGroup) promptGroup.blocksRaycasts = true;

        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;

        IsFinished = true;
    }
    void StartTypingLine(string line)
    {
        if (!dialogText) return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        currentLineFull = line ?? "";
        lineStartTime = Time.time;

        // 🔥 LOCK input briefly when new line starts
        nextAllowedClickTime = Time.time + minSkipDelay;

        advanceLock = true;

        typingRoutine = StartCoroutine(TypeLine(currentLineFull));
    }

    IEnumerator TypeLine(string fullLine)
    {
        isTyping = true;
        dialogText.text = "";

        // ✅ Replace {name} with whatever the player typed
        fullLine = fullLine.Replace("{name}", string.IsNullOrEmpty(GameData.PlayerName) ? "You" : GameData.PlayerName);
        currentLineFull = fullLine;

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

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
        dialogText.text = currentLineFull;
        isTyping = false;
    }

    bool AdvancePressed()
    {
        if (advanceLock) return false;

        return Input.GetKey(KeyCode.Space) 
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)

            || Input.GetMouseButtonDown(0);
    }

    void UpdateAdvanceLock()
    {
        if (!advanceLock) return;

        bool stillHeld =
             Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.Return) ||
            Input.GetKey(KeyCode.KeypadEnter) ||
            Input.GetMouseButton(0);

        if (!stillHeld)
            advanceLock = false;
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

        if (routine != null)
            StopCoroutine(routine);

        g.gameObject.SetActive(true);
        routine = StartCoroutine(FadeCanvasGroup(g, g.alpha, 1f, fadeDuration));
    }

    void FadeOut(CanvasGroup g, ref Coroutine routine)
    {
        if (!g) return;

        if (routine != null)
            StopCoroutine(routine);

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