using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Teleport")]
    public Transform teleportPoint; // assign in Inspector (child TeleportPoint)

    [Header("Prompt")]
    public string prompt = "Press [E] to interact";

    [Header("Fade")]
    public ScreenFader fader; // assign in Inspector (same ScreenFader used everywhere)

    public string Prompt => prompt;

    void Reset()
    {
        // Make sure the door collider is a trigger for proximity detection
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var interactor = other.GetComponentInParent<PlayerProximityInteractor>();
        if (interactor != null)
            interactor.Register(this);
    }

    void OnTriggerExit(Collider other)
    {
        var interactor = other.GetComponentInParent<PlayerProximityInteractor>();
        if (interactor != null)
            interactor.Unregister(this);
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (interactor == null || teleportPoint == null) return;
        StartCoroutine(TeleportWithFade(interactor, teleportPoint));
    }

    IEnumerator TeleportWithFade(PlayerProximityInteractor interactor, Transform target)
    {
        if (fader != null) yield return fader.FadeOut();

        // Clear stuck interactables & pickup proximity (teleport skips trigger exits)
        interactor.ClearAllInteractables();

        var itemHandler = interactor.GetComponent<PlayerItemHandler>();
        if (itemHandler != null) itemHandler.ClearNearbyItem();

        Transform player = interactor.transform;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = target.position;

        if (cc != null) cc.enabled = true;

        // Confirm arrival
        const float epsilon = 0.02f;
        float timeout = 1.0f;
        float t = 0f;
        yield return null;

        while (Vector3.Distance(player.position, target.position) > epsilon && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (fader != null) yield return fader.FadeIn();
    }
}
