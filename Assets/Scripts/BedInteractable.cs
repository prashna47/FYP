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
    public ScreenFade screenFader;

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
        if (!interactionEnabled) return;

        StartCoroutine(SleepSequence(interactor));
    }

    IEnumerator SleepSequence(PlayerProximityInteractor interactor)
    {
        if (screenFader != null)
            yield return screenFader.FadeOut();

        yield return new WaitForSeconds(holdBlackTime);

        Transform player = interactor.transform;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPoint.position;

        if (cc != null) cc.enabled = true;

        if (screenFader != null)
            yield return screenFader.FadeIn();

        if (QuestManager.Instance != null)
            QuestManager.Instance.TriggerReached();
    }
}