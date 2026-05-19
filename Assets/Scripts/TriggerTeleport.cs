using System.Collections;
using UnityEngine;

public class TriggerTeleport : MonoBehaviour
{
    [Header("Teleport Target")]
    public Transform teleportPoint;

    [Header("Quest Requirement")]
    public int requiredObjectiveIndex = 0;
    public bool requireObjectiveCompletion = true;

    [Header("Fade")]
    public ScreenFade fader;
    public float blackScreenHoldTime = 0.5f;

    [Header("Settings")]
    public bool oneTimeUse = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeUse) return;
        if (!other.CompareTag("Player")) return;

        if (QuestManager.Instance != null)
        {
            if (!QuestManager.Instance.IsObjectiveCompleted(requiredObjectiveIndex))
                return;
        }

        hasTriggered = true;
        StartCoroutine(TeleportRoutine(other.gameObject));
    }
    private IEnumerator TeleportRoutine(GameObject playerObj)
    {
        // Freeze player
        GameState.IsPlayerFrozen = true;

        // Fade out
        if (fader != null)
            yield return fader.FadeOut();

        yield return new WaitForSeconds(blackScreenHoldTime);

        // --- TELEPORT ---
        if (teleportPoint != null)
        {
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObj.transform.position = teleportPoint.position;

            // Snap camera (same pattern as your system)
            var cam = FindObjectOfType<camera>();
            if (cam != null) cam.SnapToTarget();

            yield return null;
            yield return new WaitForEndOfFrame();

            if (cc != null) cc.enabled = true;

            // cleanup interactables (safe for your system)
            var interactor = playerObj.GetComponent<PlayerProximityInteractor>();
            if (interactor != null) interactor.ClearAllInteractables();

            var itemHandler = playerObj.GetComponent<PlayerItemHandler>();
            if (itemHandler != null) itemHandler.ClearNearbyItem();
        }

        // Fade in
        if (fader != null)
            yield return fader.FadeIn();

        // Unfreeze player
        GameState.IsPlayerFrozen = false;
        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;
    }
}