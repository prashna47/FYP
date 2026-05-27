using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Navigation")]
    public List<Button> menuButtons = new List<Button>();
    public List<TextMeshProUGUI> menuTexts = new List<TextMeshProUGUI>();
    public RectTransform arrowIndicator;

    [Header("Arrow Settings")]
    public Vector2 arrowOffset = new Vector2(-30f, 0f);
    [Range(0f, 50f)] public float arrowBounceDistance = 8f;
    [Range(0.1f, 5f)] public float arrowBounceSpeed = 3f;

    [Header("Glow Settings")]
    public Color glowColor = new Color(1f, 0.8f, 0.2f, 1f);
    [Range(0f, 1f)] public float glowInner = 0.1f;
    [Range(0f, 1f)] public float glowOuter = 0.3f;
    [Range(0f, 1f)] public float glowPower = 0.5f;
    public float glowFadeDuration = 0.2f;

    private int selectedIndex = 0;
    private Material[] textMaterials;

    void Start()
    {
        SetupTextMaterials();
        UpdateSelection();

        for (int i = 0; i < menuButtons.Count; i++)
        {
            int index = i;
            AddHoverEvents(menuButtons[i], index);
        }
    }

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
        mat.SetColor(ShaderUtilities.ID_GlowColor, new Color(glowColor.r, glowColor.g, glowColor.b, power));
        mat.SetFloat(ShaderUtilities.ID_GlowInner, glowInner);
        mat.SetFloat(ShaderUtilities.ID_GlowOuter, glowOuter);
        mat.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);

        if (power > 0f)
            mat.EnableKeyword(ShaderUtilities.Keyword_Glow);
        else
            mat.DisableKeyword(ShaderUtilities.Keyword_Glow);
    }

    void AddHoverEvents(Button btn, int index)
    {
        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                   ?? btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((_) => {
            selectedIndex = index;
            UpdateSelection();
        });
        trigger.triggers.Add(enterEntry);
    }

    void Update()
    {
        HandleKeyboardInput();
        AnimateArrow();
    }

    void HandleKeyboardInput()
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

        if (moved) UpdateSelection();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            menuButtons[selectedIndex].onClick.Invoke();
        }
    }

    void AnimateArrow()
    {
        if (arrowIndicator == null || menuTexts.Count <= selectedIndex || menuTexts[selectedIndex] == null) return;

        float bouncedX = menuTexts[selectedIndex].rectTransform.position.x
                       + arrowOffset.x
                       + Mathf.Sin(Time.time * arrowBounceSpeed) * arrowBounceDistance;

        float targetY = menuTexts[selectedIndex].rectTransform.position.y + arrowOffset.y;

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

    IEnumerator FadeGlow(Material mat, float targetPower)
    {
        float startPower = mat.GetColor(ShaderUtilities.ID_GlowColor).a;
        float elapsed = 0f;

        if (targetPower > 0f)
            mat.EnableKeyword(ShaderUtilities.Keyword_Glow);

        while (elapsed < glowFadeDuration)
        {
            elapsed += Time.deltaTime;
            float power = Mathf.Lerp(startPower, targetPower, elapsed / glowFadeDuration);
            mat.SetColor(ShaderUtilities.ID_GlowColor, new Color(glowColor.r, glowColor.g, glowColor.b, power));
            mat.SetFloat(ShaderUtilities.ID_GlowInner, glowInner);
            mat.SetFloat(ShaderUtilities.ID_GlowOuter, glowOuter);
            mat.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
            yield return null;
        }

        mat.SetColor(ShaderUtilities.ID_GlowColor, new Color(glowColor.r, glowColor.g, glowColor.b, targetPower));

        if (targetPower <= 0f)
            mat.DisableKeyword(ShaderUtilities.Keyword_Glow);
    }

    void OnDestroy()
    {
        foreach (var mat in textMaterials)
        {
            if (mat != null) Destroy(mat);
        }
    }

    public void PlayGame()
    {
        ScreenFade fader = FindObjectOfType<ScreenFade>();
        if (fader != null)
            fader.FadeToScene("CharacterSelection");
        else
            SceneManager.LoadScene("CharacterSelection");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!");
    }
}