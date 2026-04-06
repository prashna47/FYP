using UnityEngine;
using System.Collections;

public class AttackTutorialManager : MonoBehaviour
{
    [Header("Trigger Settings")]
    public int unlockAtObjectiveIndex = 2;

    [Header("References")]
    public PlayerAttack playerAttack;     // 👈 drag your PlayerAttack here
    public CanvasGroup tutorialUI;

    [Header("Animation Settings")]
    public float fadeInSpeed = 2f;
    public float flickerSpeed = 2f;
    public float fadeOutSpeed = 3f;

    bool tutorialActive = false;
    bool attackUnlocked = false;
    bool isFadingIn = false;

    void Start()
    {
        // Lock attack at start
        if (playerAttack != null)
            playerAttack.canAttack = false;

        tutorialUI.alpha = 0f;
        tutorialUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (QuestManager.Instance == null) return;

        // 🔓 Unlock attack at objective
        if (!attackUnlocked &&
            QuestManager.Instance.CurrentObjectiveIndex >= unlockAtObjectiveIndex)
        {
            UnlockAttack();
        }

        if (!tutorialActive) return;

        // 🎯 After fade-in → flicker
        if (!isFadingIn)
        {
            FlickerUI();
        }

        // 🖱 Mouse OR 🎮 Controller input
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
        {
            StartCoroutine(FadeOutAndDisable());
        }
    }

    void UnlockAttack()
    {
        attackUnlocked = true;

        if (playerAttack != null)
            playerAttack.canAttack = true;

        StartCoroutine(FadeInThenFlicker());
    }

    IEnumerator FadeInThenFlicker()
    {
        tutorialActive = true;
        isFadingIn = true;

        tutorialUI.gameObject.SetActive(true);
        tutorialUI.alpha = 0f;

        // 🌟 Fade IN
        while (tutorialUI.alpha < 1f)
        {
            tutorialUI.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }

        tutorialUI.alpha = 1f;
        isFadingIn = false;
    }

    void FlickerUI()
    {
        float alpha = Mathf.PingPong(Time.time * flickerSpeed, 1f);
        tutorialUI.alpha = alpha;
    }

    IEnumerator FadeOutAndDisable()
    {
        tutorialActive = false;

        // 🌙 Fade OUT smoothly
        while (tutorialUI.alpha > 0f)
        {
            tutorialUI.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        tutorialUI.alpha = 0f;
        tutorialUI.gameObject.SetActive(false);
    }
}