using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Character Images")]
    public Image maleCharacter;
    public Image femaleCharacter;

    [Header("Outlines")]
    public Outline maleOutline;
    public Outline femaleOutline;

    [Header("Arrow")]
    public RectTransform selectionArrow;
    public ArrowBounce arrowBounce;

    private int selectedIndex = 0; // 0 = Male, 1 = Female

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleConfirmInput();
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = 1;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = 0;
            UpdateSelection();
        }
    }

    void HandleConfirmInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmSelection();
        }
    }

    void UpdateSelection()
    {
        maleOutline.enabled = selectedIndex == 0;
        femaleOutline.enabled = selectedIndex == 1;

        selectionArrow.gameObject.SetActive(true);

        RectTransform target =
            selectedIndex == 0 ? maleCharacter.rectTransform : femaleCharacter.rectTransform;

        selectionArrow.position =
            target.position + new Vector3(0, target.rect.height / 2 + 30f, 0);

        arrowBounce.ResetBounce();
    }

    // === Mouse Events ===
    public void HoverMale()
    {
        selectedIndex = 0;
        UpdateSelection();
    }

    public void HoverFemale()
    {
        selectedIndex = 1;
        UpdateSelection();
    }

    public void ConfirmSelection()
    {
        GameData.IsMale = selectedIndex == 0;
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        ScreenFade fader = FindObjectOfType<ScreenFade>();

        if (fader != null)
        {
            yield return fader.FadeOut();
            SceneManager.LoadScene("world");
        }
        else
        {
            // Fallback if no fader exists
            SceneManager.LoadScene("world");
        }
    }
}
