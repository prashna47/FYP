using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerNameUI : MonoBehaviour
{
    public static PlayerNameUI Instance;

    [Header("UI References")]
    public CanvasGroup panelGroup;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public TMP_Text errorText;

    [Header("Settings")]
    public float fadeDuration = 0.25f;
    public int minNameLength = 2;
    public int maxNameLength = 16;

    bool confirmed = false;

    public bool IsConfirmed => confirmed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Start hidden
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }

        if (errorText != null)
            errorText.text = "";
    }

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    // Called by QuestManager — waits until player confirms
    public IEnumerator ShowAndWait()
    {
        confirmed = false;

        if (nameInputField != null)
            nameInputField.text = "";

        if (errorText != null)
            errorText.text = "";

        // Freeze player while name screen is open
        PlayerControlLock.MovementLocked = true;
        InteractionLock.DialoguePlaying = true;

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Fade(1f));

        // Focus the input field so player can type immediately
        if (nameInputField != null)
            nameInputField.Select();

        // Wait until player confirms
        while (!confirmed)
            yield return null;

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Fade(0f));

        yield return StartCoroutine(Fade(0f));

        // Unfreeze player
        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;
    }

    void OnConfirmClicked()
    {
        if (nameInputField == null) return;

        string entered = nameInputField.text.Trim();

        if (entered.Length < minNameLength)
        {
            if (errorText != null)
                errorText.text = $"Name must be at least {minNameLength} characters.";
            return;
        }

        if (entered.Length > maxNameLength)
        {
            if (errorText != null)
                errorText.text = $"Name must be {maxNameLength} characters or fewer.";
            return;
        }

        // Save the name globally
        GameData.PlayerName = entered;

        confirmed = true;
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