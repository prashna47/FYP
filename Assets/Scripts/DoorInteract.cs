using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Teleport")]
    public Transform teleportPoint;

    public bool IsOpen { get; private set; }

    [Header("Prompt")]
    public string prompt = "Press [E] to interact";
    public string lockedPrompt = "Door is Locked. You need a key";
    public string unlockedPrompt = "Press [E] to Open Door";

    [Header("Door Lock State")]
    public bool isLocked = true;

    [Header("Fade")]
    public ScreenFade fader;
    public float blackScreenHoldTime = 0.5f;

    public string Prompt => prompt;

    void Reset()
    {
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

        prompt = "Press [E] to interact";
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (interactor == null) return;

        PlayerItemHandler playerItemHandler = interactor.GetComponent<PlayerItemHandler>();
        if (playerItemHandler != null)
        {
            if (isLocked)
            {
                if (playerItemHandler.IsHoldingItem && HasKey(playerItemHandler.carriedItem))
                {
                    UnlockDoor();
                    prompt = unlockedPrompt;
                }
                else
                {
                    prompt = lockedPrompt;
                    Debug.Log("Door is locked! You need a key.");
                }
            }
            else
            {
                StartCoroutine(TeleportWithFade(interactor, teleportPoint));
            }
        }
    }

    private bool HasKey(GameObject item)
    {
        return item != null && item.GetComponent<KeyItem>() != null;
    }

    private void UnlockDoor()
    {
        isLocked = false;
        prompt = "Press [E] to interact";
        Debug.Log("Door unlocked!");
    }

    IEnumerator TeleportWithFade(PlayerProximityInteractor interactor, Transform target)
    {
        IsOpen = true;

        if (fader != null)
            yield return fader.FadeOut();

        yield return new WaitForSeconds(blackScreenHoldTime);

        interactor.ClearAllInteractables();
        var itemHandler = interactor.GetComponent<PlayerItemHandler>();
        if (itemHandler != null) itemHandler.ClearNearbyItem();

        Transform player = interactor.transform;
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        player.position = target.position;

        camera cam = FindObjectOfType<camera>();
        if (cam != null) cam.SnapToTarget();

        yield return null;
        yield return new WaitForEndOfFrame();

        if (cc != null) cc.enabled = true;

        if (fader != null)
            yield return fader.FadeIn();
    }
}