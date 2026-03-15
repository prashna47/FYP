using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string prompt = "Press [E] to Sleep";

    [Header("Objective Index (Go To Bed)")]
    public int bedObjectiveIndex = 5; // Objective 6

    [Header("Teleport Settings")]
    public Transform teleportPoint;        // Assign the teleport target in Inspector
    public float holdBlackTime = 0.5f;     // Hold time while screen is black

    [Header("Screen Fade")]
    public ScreenFade screenFader;         // Assign your existing ScreenFade component here

    private bool isInteractable = false;   // Only allow interaction after Objective 6

    public string Prompt => prompt;

    void Start()
    {
        // Keep the bed active at all times
        gameObject.SetActive(true);

        if (screenFader == null)
            Debug.LogWarning("ScreenFade not assigned on BedInteractable!");
    }

    void Update()
    {
        // Enable interaction only when Objective 6 starts
        if (!isInteractable && QuestManager.Instance != null &&
            QuestManager.Instance.CurrentObjectiveIndex == bedObjectiveIndex)
        {
            isInteractable = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isInteractable) return;

        var interactor = other.GetComponentInParent<PlayerProximityInteractor>();
        if (interactor != null)
            interactor.Register(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isInteractable) return;

        var interactor = other.GetComponentInParent<PlayerProximityInteractor>();
        if (interactor != null)
            interactor.Unregister(this);
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (!isInteractable) return;

        // Clear any existing prompt
        interactor.ClearAllInteractables();

        // Complete the objective
        if (QuestManager.Instance != null &&
            QuestManager.Instance.CurrentObjectiveIndex == bedObjectiveIndex)
        {
            QuestManager.Instance.TriggerReached();
        }

        // Fade + teleport
        if (teleportPoint != null && screenFader != null)
        {
            StartCoroutine(FadeAndTeleport(interactor.transform));
        }
    }

    IEnumerator FadeAndTeleport(Transform player)
    {
        // Fade out
        yield return screenFader.FadeOut();

        // Hold black
        yield return new WaitForSeconds(holdBlackTime);

        // Teleport player
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPoint.position;
        player.rotation = teleportPoint.rotation;

        if (cc != null) cc.enabled = true;

        // Fade in
        yield return screenFader.FadeIn();
    }
}