using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string prompt = "Press [E] to Listen";

    [Header("Audio")]
    public AudioSource audioSource;
    public bool playOnce = true;

    private bool hasPlayed = false;

    public string Prompt => prompt;

    void Reset()
    {
        // Ensure trigger collider
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Interact(PlayerProximityInteractor interactor)
    {
        if (audioSource == null) return;

        if (playOnce && hasPlayed)
            return;

        audioSource.Play();
        hasPlayed = true;
    }
}
