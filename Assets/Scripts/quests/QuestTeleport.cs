using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to any GameObject. In QuestManager, drag this into the
/// Objective's "questTeleport" slot. After the NPC interaction finishes
/// for that objective, the player will fade to black and teleport.
/// </summary>
public class QuestTeleport : MonoBehaviour
{
    [Header("Teleport")]
    public Transform teleportPoint;

    [Header("Fade")]
    public ScreenFade fader;
    public float blackScreenHoldTime = 0.5f;

    /// <summary>
    /// Called by QuestManager after NPC interaction completes.
    /// </summary>
    public void Execute()
    {
        StartCoroutine(TeleportWithFade());
    }

    IEnumerator TeleportWithFade()
    {
        // Freeze player during teleport
        GameState.IsPlayerFrozen = true;

        // Fade to black
        if (fader != null)
            yield return fader.FadeOut();

        yield return new WaitForSeconds(blackScreenHoldTime);

        // --- Do the teleport ---
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && teleportPoint != null)
        {
            // Disable CharacterController before moving (same pattern as DoorInteractable)
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObj.transform.position = teleportPoint.position;

            // Snap camera so it doesn't lerp across the map
            var cam = FindObjectOfType<camera>();
            if (cam != null) cam.SnapToTarget();

            // Wait a frame for physics to settle
            yield return null;
            yield return new WaitForEndOfFrame();

            if (cc != null) cc.enabled = true;

            // Clear any leftover interactable registrations
            var interactor = playerObj.GetComponent<PlayerProximityInteractor>();
            if (interactor != null) interactor.ClearAllInteractables();

            var itemHandler = playerObj.GetComponent<PlayerItemHandler>();
            if (itemHandler != null) itemHandler.ClearNearbyItem();
        }

        // Fade back in
        if (fader != null)
            yield return fader.FadeIn();

        // Unfreeze player
        GameState.IsPlayerFrozen = false;
    }
}