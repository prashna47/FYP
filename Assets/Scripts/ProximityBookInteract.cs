// ProximityBookInteractable.cs
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ProximityBookInteractable : MonoBehaviour, IInteractable
{
    public BookData book;
    [Tooltip("Optional override for prompt text.")]
    public string promptOverride = "Press [E] to Interact";

    public string Prompt => string.IsNullOrEmpty(promptOverride) ? "Press [E] to Interact" : promptOverride;

    void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2.0f;
    }

    void OnTriggerEnter(Collider other)
    {
        var interactor = other.GetComponentInParent<PlayerProximityInteractor>();
        if (interactor != null) interactor.Register(this);
    }

    void OnTriggerExit(Collider other)
    {
        var interactor = other.GetComponentInParent<PlayerProximityInteractor>();
        if (interactor != null) interactor.Unregister(this);
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (book != null) BookUI.Instance.Open(book);
    }
}
