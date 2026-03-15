using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ProximityBookInteractable : MonoBehaviour, IInteractable
{
    public BookData book;
    public string promptOverride = "Press [E] to Interact";

    bool interactionEnabled = true;
    PlayerProximityInteractor playerInside;

    public string Prompt => string.IsNullOrEmpty(promptOverride) ? "Press [E] to Interact" : promptOverride;

    void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2.0f;
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
        if (!interactionEnabled) return;

        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null)
            playerInside = interactor;

        if (interactionEnabled)
            playerInside.Register(this);
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

        if (book != null)
            BookUI.Instance.Open(book);
    }
}