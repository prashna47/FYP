using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string prompt = "Press [E] to Sleep";

    [Header("Teleport")]
    public Transform teleportPoint;
    public float holdBlackTime = 0.5f;

    [Header("Fade")]
    public ScreenFade fader;

    bool interactionEnabled = false;
    PlayerProximityInteractor playerInside;

    public string Prompt => prompt;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;

        if (playerInside != null)
        {
            if (interactionEnabled)
                playerInside.Register(this);
            else
                playerInside.Unregister(this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null)
        {
            playerInside = interactor;
            if (interactionEnabled)
                playerInside.Register(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null)
        {
            interactor.Unregister(this);
            playerInside = null;
        }
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (!interactionEnabled || interactor == null) return;

        StartCoroutine(SleepWithFade(interactor));
    }

    IEnumerator SleepWithFade(PlayerProximityInteractor interactor)
    {
        if (!interactionEnabled || interactor == null) yield break;

        // 1️⃣ Immediately unregister to prevent repeated prompts
        interactor.Unregister(this);
        playerInside = null;

        // Fade out first
        if (fader != null)
            yield return fader.FadeOut();

        yield return new WaitForSeconds(holdBlackTime);

        // Clear interactables/items
        interactor.ClearAllInteractables();
        var itemHandler = interactor.GetComponent<PlayerItemHandler>();
        if (itemHandler != null)
            itemHandler.ClearNearbyItem();

        // Teleport player while screen is black
        Transform player = interactor.transform;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPoint.position;

        // Snap camera if using custom camera script
        var cam = FindObjectOfType<camera>();
        if (cam != null)
            cam.SnapToTarget();

        yield return null;
        yield return new WaitForEndOfFrame();

        if (cc != null) cc.enabled = true;

        // Fade back in
        if (fader != null)
            yield return fader.FadeIn();

        // Trigger quest progression
        if (QuestManager.Instance != null)
            QuestManager.Instance.TriggerReached();
    }
}