using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI")]
    public GameObject pauseMenuUI;
    public CanvasGroup canvasGroup;
    public RectTransform menuPanel;
    public GameObject firstSelectedButton;

    private bool isPaused = false;
    private bool arrowActive = false;

    [Header("Camera Zoom")]
    public Camera mainCamera;
    public float zoomInSize = 4f;
    public float zoomSpeed = 8f;

    private float defaultSize;
    private float targetZoom;

    [Header("Pause Buttons")]
    public List<UnityEngine.UI.Button> menuButtons = new List<UnityEngine.UI.Button>();
    public List<TextMeshProUGUI> menuTexts = new List<TextMeshProUGUI>();
    public RectTransform arrowIndicator;

    [Header("Arrow Settings")]
    public Vector2 arrowOffset = new Vector2(-30f, 0f);
    public float arrowBounceDistance = 8f;
    public float arrowBounceSpeed = 3f;

    [Header("Glow Settings")]
    public Color glowColor = new Color(1f, 0.8f, 0.2f, 1f);
    [Range(0f, 1f)] public float glowInner = 0.1f;
    [Range(0f, 1f)] public float glowOuter = 0.3f;
    [Range(0f, 1f)] public float glowPower = 0.5f;
    public float glowFadeDuration = 0.2f;

    private int selectedIndex = 0;
    private Material[] textMaterials;

    [Header("Pause Button")]
    public GameObject pauseButtonUI;

    [Header("Other UI To Hide When Paused")]
    public GameObject[] otherUIElements;
    private bool[] previousUIStates;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip openSound;
    public AudioClip clickSound;

    [Header("Animation")]
    public float fadeDuration = 0.25f;
    public float panelSlideDistance = 40f;

    private Vector2 menuPanelDefaultPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (mainCamera != null)
        {
            defaultSize = mainCamera.orthographicSize;
            targetZoom = defaultSize;
        }

        if (otherUIElements != null)
            previousUIStates = new bool[otherUIElements.Length];

        if (arrowIndicator != null)
            arrowIndicator.gameObject.SetActive(false);

        if (menuPanel != null)
            menuPanelDefaultPos = menuPanel.anchoredPosition;

        SetupTextMaterials();
        AddHoverEvents();
        HidePauseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        HandleZoom();

        if (!isPaused) return;

        HandleInput();
        AnimateArrow();
    }

    // ---------------- ZOOM ----------------

    void HandleZoom()
    {
        if (mainCamera == null) return;

        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetZoom,
            Time.unscaledDeltaTime * zoomSpeed
        );
    }

    // ---------------- INPUT ----------------

    void HandleInput()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Count;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
            moved = true;
        }

        if (moved)
            UpdateSelection();

        if (Input.GetKeyDown(KeyCode.Return))
            ActivateSelected();
    }

    void ActivateSelected()
    {
        if (selectedIndex < 0 || selectedIndex >= menuButtons.Count) return;
        if (menuButtons[selectedIndex] == null) return;

        menuButtons[selectedIndex].onClick.Invoke();
    }

    // ---------------- HOVER ----------------

    void AddHoverEvents()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            int index = i;

            if (menuButtons[i] == null) continue;

            var trigger = menuButtons[i].GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;

            entry.callback.AddListener((data) =>
            {
                if (!isPaused) return;

                selectedIndex = index;
                UpdateSelection();
            });

            trigger.triggers.Add(entry);
        }
    }

    // ---------------- ARROW ----------------

    void AnimateArrow()
    {
        if (!arrowActive) return;

        if (arrowIndicator == null ||
            menuTexts.Count <= selectedIndex ||
            menuTexts[selectedIndex] == null)
            return;

        float bouncedX =
            menuTexts[selectedIndex].rectTransform.position.x +
            arrowOffset.x +
            Mathf.Sin(Time.unscaledTime * arrowBounceSpeed) * arrowBounceDistance;

        float targetY =
            menuTexts[selectedIndex].rectTransform.position.y +
            arrowOffset.y;

        arrowIndicator.position = new Vector3(
            bouncedX,
            targetY,
            arrowIndicator.position.z
        );
    }

    void UpdateSelection()
    {
        StopAllCoroutines();

        for (int i = 0; i < menuTexts.Count; i++)
        {
            if (textMaterials[i] == null) continue;

            bool isSelected = i == selectedIndex;
            StartCoroutine(FadeGlow(textMaterials[i], isSelected ? 1f : 0f));
        }
    }

    // ---------------- GLOW ----------------

    void SetupTextMaterials()
    {
        textMaterials = new Material[menuTexts.Count];

        for (int i = 0; i < menuTexts.Count; i++)
        {
            if (menuTexts[i] == null) continue;

            textMaterials[i] = new Material(menuTexts[i].fontMaterial);
            menuTexts[i].fontMaterial = textMaterials[i];

            SetGlowInstant(textMaterials[i], 0f);
        }
    }

    void SetGlowInstant(Material mat, float power)
    {
        mat.SetColor(TMPro.ShaderUtilities.ID_GlowColor,
            new Color(glowColor.r, glowColor.g, glowColor.b, power));

        mat.SetFloat(TMPro.ShaderUtilities.ID_GlowInner, glowInner);
        mat.SetFloat(TMPro.ShaderUtilities.ID_GlowOuter, glowOuter);
        mat.SetFloat(TMPro.ShaderUtilities.ID_GlowPower, glowPower);

        if (power > 0f)
            mat.EnableKeyword(TMPro.ShaderUtilities.Keyword_Glow);
        else
            mat.DisableKeyword(TMPro.ShaderUtilities.Keyword_Glow);
    }

    IEnumerator FadeGlow(Material mat, float targetPower)
    {
        float startPower = mat.GetColor(TMPro.ShaderUtilities.ID_GlowColor).a;
        float elapsed = 0f;

        if (targetPower > 0f)
            mat.EnableKeyword(TMPro.ShaderUtilities.Keyword_Glow);

        while (elapsed < glowFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float power = Mathf.Lerp(startPower, targetPower, elapsed / glowFadeDuration);

            mat.SetColor(TMPro.ShaderUtilities.ID_GlowColor,
                new Color(glowColor.r, glowColor.g, glowColor.b, power));

            yield return null;
        }

        mat.SetColor(TMPro.ShaderUtilities.ID_GlowColor,
            new Color(glowColor.r, glowColor.g, glowColor.b, targetPower));

        if (targetPower <= 0f)
            mat.DisableKeyword(TMPro.ShaderUtilities.Keyword_Glow);
    }

    // ---------------- FADE ANIMATION ----------------

    IEnumerator FadeInMenu()
    {
        pauseMenuUI.SetActive(true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (menuPanel != null)
            menuPanel.anchoredPosition = menuPanelDefaultPos - new Vector2(0, panelSlideDistance);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);

            canvasGroup.alpha = t;

            if (menuPanel != null)
                menuPanel.anchoredPosition = Vector2.Lerp(
                    menuPanelDefaultPos - new Vector2(0, panelSlideDistance),
                    menuPanelDefaultPos,
                    t
                );

            yield return null;
        }

        canvasGroup.alpha = 1f;

        if (menuPanel != null)
            menuPanel.anchoredPosition = menuPanelDefaultPos;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // Start selection AFTER fade so glow coroutines aren't killed
        selectedIndex = 0;
        UpdateSelection();

        if (arrowIndicator != null)
        {
            arrowActive = true;
            arrowIndicator.gameObject.SetActive(true);
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    IEnumerator FadeOutMenu()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            if (menuPanel != null)
                menuPanel.anchoredPosition = Vector2.Lerp(
                    menuPanelDefaultPos,
                    menuPanelDefaultPos - new Vector2(0, panelSlideDistance),
                    t
                );

            yield return null;
        }

        HidePauseMenu();

        if (menuPanel != null)
            menuPanel.anchoredPosition = menuPanelDefaultPos;
    }

    // ---------------- PAUSE ----------------

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        GameState.IsPaused = true;
        Time.timeScale = 0f;

        if (pauseButtonUI != null)
            pauseButtonUI.SetActive(false);

        SaveAndHideOtherUI();

        if (uiAudioSource && openSound)
            uiAudioSource.PlayOneShot(openSound);

        targetZoom = zoomInSize;

        StopAllCoroutines();
        StartCoroutine(FadeInMenu());
    }

    public void Resume()
    {
        isPaused = false;
        GameState.IsPaused = false;
        Time.timeScale = 1f;

        if (pauseButtonUI != null)
            pauseButtonUI.SetActive(true);

        RestoreOtherUI();

        if (arrowIndicator != null)
        {
            arrowActive = false;
            arrowIndicator.gameObject.SetActive(false);
        }

        targetZoom = defaultSize;

        StopAllCoroutines();
        StartCoroutine(FadeOutMenu());
    }

    void HidePauseMenu()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    // ---------------- UI SAVE ----------------

    void SaveAndHideOtherUI()
    {
        if (otherUIElements == null) return;

        for (int i = 0; i < otherUIElements.Length; i++)
        {
            if (otherUIElements[i] != null)
            {
                previousUIStates[i] = otherUIElements[i].activeSelf;
                otherUIElements[i].SetActive(false);
            }
        }
    }

    void RestoreOtherUI()
    {
        if (otherUIElements == null) return;

        for (int i = 0; i < otherUIElements.Length; i++)
        {
            if (otherUIElements[i] != null)
                otherUIElements[i].SetActive(previousUIStates[i]);
        }
    }

    // ---------------- BUTTONS ----------------

    public void GoToMainMenu()
    {
        isPaused = false;
        GameState.IsPaused = false;
        Time.timeScale = 1f;

        HidePauseMenu();

        if (otherUIElements != null)
        {
            foreach (GameObject ui in otherUIElements)
            {
                if (ui != null)
                    Destroy(ui);
            }
        }

        if (uiAudioSource && clickSound)
            uiAudioSource.PlayOneShot(clickSound);

        SceneManager.LoadScene("MainMenu");
    }

    public void PlayClickSound()
    {
        if (uiAudioSource && clickSound)
            uiAudioSource.PlayOneShot(clickSound);
    }
}