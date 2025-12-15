using UnityEngine;

// Attach this to the PLAYER.
// Requires your existing PlayerProximityInteractor + IInteractable system.
public class PlayerDoorProximityBridge : MonoBehaviour
{
    [Header("Door Detection")]
    public string doorTag = "Door";
    public string teleportChildName = "TeleportPoint";

    [Header("Prompt")]
    public string doorPrompt = "Press [E] to interact";

    private PlayerProximityInteractor interactor;
    private DoorRuntimeInteractable activeDoorInteractable;

    void Awake()
    {
        interactor = GetComponent<PlayerProximityInteractor>();
        if (interactor == null)
            Debug.LogError("PlayerDoorProximityBridge: No PlayerProximityInteractor found on the Player.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (interactor == null) return;
        if (!other.CompareTag(doorTag)) return;

        // Find teleport point on the door (child named TeleportPoint)
        Transform tp = other.transform.Find(teleportChildName);
        if (tp == null)
        {
            Debug.LogWarning($"Door '{other.name}' has no child named '{teleportChildName}'.");
            return;
        }

        // Prevent duplicates if we re-enter quickly
        if (activeDoorInteractable != null)
        {
            interactor.Unregister(activeDoorInteractable);
            Destroy(activeDoorInteractable.gameObject);
            activeDoorInteractable = null;
        }

        // Create a proxy object located at the door so nearest-selection works
        GameObject proxy = new GameObject($"DoorInteractableProxy_{other.name}");
        proxy.transform.position = other.transform.position;
        proxy.transform.rotation = other.transform.rotation;

        activeDoorInteractable = proxy.AddComponent<DoorRuntimeInteractable>();
        activeDoorInteractable.Init(tp, doorPrompt);

        interactor.Register(activeDoorInteractable);
    }

    void OnTriggerExit(Collider other)
    {
        if (interactor == null) return;
        if (!other.CompareTag(doorTag)) return;

        if (activeDoorInteractable != null)
        {
            interactor.Unregister(activeDoorInteractable);
            Destroy(activeDoorInteractable.gameObject);
            activeDoorInteractable = null;
        }
    }

    // Nested interactable class so everything is in ONE file
    private class DoorRuntimeInteractable : MonoBehaviour, IInteractable
    {
        private Transform teleportPoint;
        private string prompt;

        public string Prompt => prompt;

        public void Init(Transform tp, string promptText)
        {
            teleportPoint = tp;
            prompt = promptText;
        }

        public void Interact(PlayerProximityInteractor interactor)
        {
            if (teleportPoint == null) return;

            Transform player = interactor.transform;

            // CharacterController-safe teleport
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = teleportPoint.position;

            if (cc != null) cc.enabled = true;
        }
    }
}
