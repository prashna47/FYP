using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI")]
    public GameObject pauseMenuUI;
    public CanvasGroup canvasGroup;
    public GameObject firstSelectedButton;

    [Header("Pause Button")]
    public GameObject pauseButtonUI;

    [Header("Other UI To Hide When Paused")]
    public GameObject[] otherUIElements;

    // ✅ Stores each element's active state before we hid it
    private bool[] previousUIStates;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip openSound;
    public AudioClip clickSound;

    [Header("Camera")]
    public Camera mainCamera;
    public float zoomInSize = 4f;
    private float defaultSize;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (mainCamera != null)
            defaultSize = mainCamera.orthographicSize;

        // ✅ Initialize the state array to match the UI array size
        if (otherUIElements != null)
            previousUIStates = new bool[otherUIElements.Length];

        HidePauseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

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

        pauseMenuUI.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (pauseButtonUI != null)
            pauseButtonUI.SetActive(false);

        // ✅ Save states then hide
        SaveAndHideOtherUI();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }

        if (uiAudioSource && openSound)
            uiAudioSource.PlayOneShot(openSound);

        if (mainCamera != null)
            mainCamera.orthographicSize = zoomInSize;
    }

    public void Resume()
    {
        isPaused = false;
        GameState.IsPaused = false;
        Time.timeScale = 1f;

        HidePauseMenu();

        if (pauseButtonUI != null)
            pauseButtonUI.SetActive(true);

        // ✅ Restore each element to what it was before pause
        RestoreOtherUI();

        if (mainCamera != null)
            mainCamera.orthographicSize = defaultSize;
    }

    private void HidePauseMenu()
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

    // ✅ Snapshot each element's current state, then disable it
    private void SaveAndHideOtherUI()
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

    // ✅ Restore each element to exactly what it was before pause
    private void RestoreOtherUI()
    {
        if (otherUIElements == null) return;
        for (int i = 0; i < otherUIElements.Length; i++)
        {
            if (otherUIElements[i] != null)
                otherUIElements[i].SetActive(previousUIStates[i]);
        }
    }

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