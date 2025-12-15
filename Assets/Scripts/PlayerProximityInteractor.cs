// PlayerProximityInteractor.cs
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PlayerProximityInteractor : MonoBehaviour
{
    [Header("UI Prompt")]
    public CanvasGroup promptGroup;
    public TMP_Text promptText;

    [Header("Item Handling")]
    public PlayerItemHandler itemHandler;

    [Header("Options")]
    public float switchHysteresis = 0.25f; // reduce flicker when two are close

    private readonly List<IInteractable> inRange = new();
    private IInteractable current;
    private Transform self;

    void Awake()
    {
        self = transform;
        if (promptGroup) { promptGroup.alpha = 0f; promptGroup.blocksRaycasts = false; }
    }

    void Update()
    {
        if (BookUI.IsOpen) { SetPrompt(null); return; }

        for (int i = inRange.Count - 1; i >= 0; i--)
        {
            var comp = inRange[i] as Component;
            if (comp == null || !comp)
            {
                if (current == inRange[i]) current = null;
                inRange.RemoveAt(i);
            }
        }

        // pick nearest
        IInteractable nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < inRange.Count; i++)
        {
            var inter = inRange[i] as Component;
            if (inter == null) continue;
            float d = Vector3.Distance(self.position, inter.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = inRange[i]; }
        }

        // small hysteresis to avoid jitter
        if (current != null && nearest != null && current != nearest)
        {
            var curComp = current as Component;
            var nearComp = nearest as Component;
            float dCur = Vector3.Distance(self.position, curComp.transform.position);
            if (dCur <= nearestDist + switchHysteresis) nearest = current;
        }

        current = nearest;
        SetPrompt(current);

         if (current != null && Input.GetKeyDown(KeyCode.E) && !InteractionLock.NpcInRange)
            {
            current.Interact(this);
        }
    }

    void SetPrompt(IInteractable interactable)
    {
        bool holdingItem = itemHandler != null && itemHandler.IsHoldingItem;

        // We want to show the prompt if:
        // - there's something to interact with, OR
        // - the player is holding an item (so we can show "Press G to Drop")
        bool show = interactable != null || holdingItem;

        if (!promptGroup) return;

        promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, show ? 1f : 0f, Time.deltaTime * 10f);
        promptGroup.blocksRaycasts = show;

        if (promptText == null) return;

        if (holdingItem && interactable == null)
        {
            // Not near anything, but holding an item -> show Drop prompt
            promptText.text = "Press [G] to Drop";
        }
        else if (interactable != null)
        {
            // Near an interactable -> show its own prompt
            promptText.text = interactable.Prompt;
        }
    }


    // Called by proximity interactables on trigger enter/exit
    public void Register(IInteractable interactable)
    {
        if (!inRange.Contains(interactable)) inRange.Add(interactable);
    }

    public void Unregister(IInteractable interactable)
    {
        inRange.Remove(interactable);
        if (current == interactable) current = null;
    }
    public void ClearAllInteractables()
    {
        inRange.Clear();
        current = null;

        // Hide the E prompt immediately
        if (promptGroup)
        {
            promptGroup.alpha = 0f;
            promptGroup.blocksRaycasts = false;
        }

        if (promptText)
            promptText.text = "";
    }

}
