using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class StaticNPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptText = "Press [E] to Interact";

    [Header("Quest — objective this completes on first E press")]
    public int objectiveIndex = 0;

    [Header("Choice Screen — shown after objective completes")]
    public string choicePrompt = "What would you like to do?";
    public string choiceLabelA = "Head Out";
    public string choiceLabelB = "Stay and Explore";
    public QuestTeleport headOutTeleport;

    PlayerProximityInteractor playerInside;
    bool objectiveCompleted = false;
    bool isHandlingChoice = false;

    public string Prompt => promptText;

    void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var interactor = other.GetComponent<PlayerProximityInteractor>();
        if (interactor != null)
        {
            playerInside = interactor;
            InteractionLock.NpcInRange = false;
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
        if (isHandlingChoice) return;

        if (!objectiveCompleted)
        {
            // First E press — complete the objective if it's the right one
            if (QuestManager.Instance.CurrentObjectiveIndex != objectiveIndex) return;

            objectiveCompleted = true;
            interactor.Unregister(this);

            // Complete the objective — QuestManager plays completion dialogue,
            // then the choice screen on the objective fires if you have it set up there.
            // After that the player can come back and press E to get the loop below.
            QuestManager.Instance.TriggerReached();
        }
        else
        {
            // Objective already done — show choice screen in a loop
            StartCoroutine(HandleChoiceLoop(interactor));
        }
    }

    IEnumerator HandleChoiceLoop(PlayerProximityInteractor interactor)
    {
        isHandlingChoice = true;

        // Hide E prompt while choice is open
        interactor.Unregister(this);

        yield return PlayerChoiceUI.Instance.ShowAndWait(
            choicePrompt,
            choiceLabelA,
            choiceLabelB
        );

        if (PlayerChoiceUI.Instance.ChoseHeadOut)
        {
            isHandlingChoice = false;

            // ✅ Make sure locks are fully clear before teleport takes over
            PlayerControlLock.MovementLocked = false;
            InteractionLock.DialoguePlaying = false;

            if (headOutTeleport != null)
            {
                headOutTeleport.Execute();
                yield return new WaitUntil(() => !GameState.IsPlayerFrozen);
            }
        }
        else
        {
            // Player picked Stay — re-register so they can press E again
            isHandlingChoice = false;

            if (playerInside != null)
                playerInside.Register(this);
        }
    }
}