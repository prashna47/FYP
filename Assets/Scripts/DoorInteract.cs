using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Teleport")]
    public Transform teleportPoint; // assign in Inspector (child TeleportPoint)

    [Header("Prompt")]
    public string prompt = "Press [E] to interact";  // Default prompt for unlocked door
    public string lockedPrompt = "Door is Locked. You need a key";  // Prompt for locked door
    public string unlockedPrompt = "Press [E] to Open Door";  // New prompt for when door is unlocked

    [Header("Door Lock State")]
    public bool isLocked = true;  // By default, doors are locked. You can set this in the Inspector.

    [Header("Fade")]
    public ScreenFader fader; // assign in Inspector (same ScreenFader used everywhere)

    private bool isUnlocked = false;

    public string Prompt => prompt;  // Always show the default prompt for interaction

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

        // When the player exits the trigger zone, reset the prompt back to "Press [E] to interact"
        prompt = "Press [E] to interact";  // Reset prompt when leaving the interaction zone
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (interactor == null) return;

        PlayerItemHandler playerItemHandler = interactor.GetComponent<PlayerItemHandler>();  // Check if player has the key
        if (playerItemHandler != null)
        {
            if (isLocked)
            {
                // When pressing [E], check if the player is holding the key
                if (playerItemHandler.IsHoldingItem && HasKey(playerItemHandler.carriedItem))
                {
                    UnlockDoor();  // Unlock the door
                    prompt = unlockedPrompt;  // Show "Door Unlocked" when the player uses the key
                }
                else
                {
                    // Player doesn't have the key, show the locked prompt
                    prompt = lockedPrompt;
                    Debug.Log("Door is locked! You need a key.");
                }
            }
            else
            {
                // If the door is already unlocked, teleport the player
                StartCoroutine(TeleportWithFade(interactor, teleportPoint));
            }
        }
    }

    // This function checks if the carried item is the key
    private bool HasKey(GameObject item)
    {
        // Check if the item has the KeyItem script attached
        return item != null && item.GetComponent<KeyItem>() != null;  // Identifies key by its KeyItem component
    }

    private void UnlockDoor()
    {
        // Unlock the door and update prompt
        isLocked = false;
        prompt = "Press [E] to interact";  // Change the prompt back to the usual interaction prompt after unlocking
        Debug.Log("Door is unlocked!");
    }

    IEnumerator TeleportWithFade(PlayerProximityInteractor interactor, Transform target)
    {
        // Start fading out the screen to black
        if (fader != null)
            yield return fader.FadeOut();

        // Clear stuck interactables & pickup proximity (teleport skips trigger exits)
        interactor.ClearAllInteractables();

        var itemHandler = interactor.GetComponent<PlayerItemHandler>();
        if (itemHandler != null) itemHandler.ClearNearbyItem();

        Transform player = interactor.transform;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;  // Disable the character controller to prevent movement during teleportation

        player.position = target.position; // Move the player to the target position

        if (cc != null)
            cc.enabled = true;  // Re-enable the character controller after the teleportation

        // Now wait a short moment to let the teleportation complete
        const float epsilon = 0.02f;
        float timeout = 1.0f;
        float t = 0f;
        yield return null;

        while (Vector3.Distance(player.position, target.position) > epsilon && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Once the player is at the target position, fade the screen back in
        if (fader != null)
            yield return fader.FadeIn();
    }
}
