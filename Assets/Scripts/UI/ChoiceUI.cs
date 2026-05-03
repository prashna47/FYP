using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerChoiceUI : MonoBehaviour
{
    public static PlayerChoiceUI Instance;

    [Header("UI References")]
    public CanvasGroup panelGroup;
    public TMP_Text promptText;
    public Button choiceAButton; // Head Out
    public Button choiceBButton; // Stay and Explore
    public TMP_Text choiceAText;
    public TMP_Text choiceBText;

    [Header("Timing")]
    public float fadeDuration = 0.25f;
    public float delayBeforeShow = 1.5f;

    bool choiceMade = false;
    bool choseA = false;

    public bool IsChoiceMade => choiceMade;
    public bool ChoseHeadOut => choseA;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }
    }

    void Start()
    {
        if (choiceAButton != null)
            choiceAButton.onClick.AddListener(() => OnChoice(true));
        if (choiceBButton != null)
            choiceBButton.onClick.AddListener(() => OnChoice(false));
    }

    // Called by QuestManager — pass in the label text from Inspector
    public IEnumerator ShowAndWait(string prompt, string labelA, string labelB)
    {
        choiceMade = false;

        if (promptText != null) promptText.text = prompt;
        if (choiceAText != null) choiceAText.text = labelA;
        if (choiceBText != null) choiceBText.text = labelB;

        // Freeze player while choice is open
        PlayerControlLock.MovementLocked = true;
        InteractionLock.DialoguePlaying = true;

        yield return new WaitForSeconds(delayBeforeShow);
        yield return StartCoroutine(Fade(1f));

        while (!choiceMade)
            yield return null;

        yield return StartCoroutine(Fade(0f));

        // Only unfreeze if they stayed — if they head out, 
        // QuestTeleport handles the freeze/unfreeze
        if (!choseA)
        {
            PlayerControlLock.MovementLocked = false;
            InteractionLock.DialoguePlaying = false;
        }
    }

    void OnChoice(bool pickedA)
    {
        choseA = pickedA;
        choiceMade = true;
    }

    IEnumerator Fade(float target)
    {
        float start = panelGroup.alpha;
        float t = 0f;

        panelGroup.interactable = target > 0f;
        panelGroup.blocksRaycasts = target > 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            panelGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        panelGroup.alpha = target;
    }
}