using UnityEngine;
using System.Collections;

public class AttackTutorialManager : MonoBehaviour
{
    [Header("Trigger Settings")]
    public int unlockAtObjectiveIndex = 2;

    [Header("References")]
    PlayerAttack playerAttack;
    public CanvasGroup tutorialUI;

    [Header("Animation Settings")]
    public float fadeInSpeed = 2f;
    public float flickerSpeed = 2f;
    public float fadeOutSpeed = 3f;

    bool tutorialActive = false;
    bool attackUnlocked = false;
    bool isFadingIn = false;

    // ✅ NEW
    bool canDismiss = false;
    bool isFadingOut = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerAttack = player.GetComponent<PlayerAttack>();
        }
        else
        {
            Debug.LogWarning("No GameObject with tag 'Player' found!");
        }

        if (playerAttack != null)
            playerAttack.canAttack = false;

        tutorialUI.alpha = 0f;
        tutorialUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (QuestManager.Instance == null) return;

        if (!attackUnlocked &&
            QuestManager.Instance.CurrentObjectiveIndex >= unlockAtObjectiveIndex)
        {
            UnlockAttack();
        }

        if (!tutorialActive) return;

        if (!isFadingIn && !isFadingOut)
        {
            FlickerUI();
        }

        // ✅ ONLY allow click after fully shown
        if (canDismiss &&
            !isFadingOut &&
            (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1")))
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
        canDismiss = false; // 🔒 LOCK INPUT

        tutorialUI.gameObject.SetActive(true);
        tutorialUI.alpha = 0f;

        while (tutorialUI.alpha < 1f)
        {
            tutorialUI.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }

        tutorialUI.alpha = 1f;

        isFadingIn = false;
        canDismiss = true; // 🔓 UNLOCK INPUT ONLY AFTER FULLY VISIBLE
    }

    void FlickerUI()
    {
        float alpha = Mathf.PingPong(Time.time * flickerSpeed, 1f);
        tutorialUI.alpha = alpha;
    }

    IEnumerator FadeOutAndDisable()
    {
        isFadingOut = true;
        tutorialActive = false;
        canDismiss = false; // 🔒 prevent spam

        while (tutorialUI.alpha > 0f)
        {
            tutorialUI.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        tutorialUI.alpha = 0f;
        tutorialUI.gameObject.SetActive(false);

        isFadingOut = false;
    }
}