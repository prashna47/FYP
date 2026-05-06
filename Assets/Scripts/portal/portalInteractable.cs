using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class ProximityPortalInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    public string promptOverride = "Press [E] to Enter Portal";

    [Header("Portal Visual")]
    [SerializeField] private Portal_Controller portal;

    [Header("Teleport")]
    [SerializeField] private Transform teleportTarget;

    [Header("Fade System (USE YOUR QUEST FADE)")]
    [SerializeField] private ScreenFade fader;
    [SerializeField] private float blackScreenHoldTime = 0.5f;

    [Header("Ranges")]
    [SerializeField] private float activationRange = 6f;
    [SerializeField] private float interactRange = 3f;

    [Header("Effects")]
    [SerializeField] private GameObject preTeleportEffect;
    [SerializeField] private GameObject postTeleportEffect;

    [Header("Effect Offset")]
    [SerializeField] private Vector3 effectOffset = new Vector3(0, -0.5f, 0);

    private Transform player;
    private PlayerProximityInteractor interactor;

    private bool portalStarted;
    private bool canInteract;
    private bool busy;

    public string Prompt => string.IsNullOrEmpty(promptOverride)
        ? "Press [E] to Enter Portal"
        : promptOverride;
    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        // 🔥 Portal ON when entering range
        if (dist <= activationRange && !portalStarted)
        {
            portalStarted = true;
            portal?.SetPlayerInRange(true);
        }

        // 🔥 Portal OFF when leaving range
        if (dist > activationRange && portalStarted)
        {

            portalStarted = false;

            portal?.SetPlayerInRange(false);
        }

        // 🔥 Show interaction prompt only when close
        if (dist <= interactRange && !canInteract)
        {
            canInteract = true;
            interactor?.Register(this);
        }
        else if (dist > interactRange && canInteract)
        {
            canInteract = false;
            interactor?.Unregister(this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player = other.transform;
        interactor = other.GetComponent<PlayerProximityInteractor>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        interactor?.Unregister(this);
        player = null;
        interactor = null;
        canInteract = false;
    }

    public void Interact(PlayerProximityInteractor pInteractor)
    {
        if (busy || teleportTarget == null) return;

        StartCoroutine(PortalSequence(pInteractor.transform));
    }

    IEnumerator PortalSequence(Transform playerObj)
    {
        busy = true;

        // 🔥 1. PRE EFFECT
        if (preTeleportEffect != null)
        {
            GameObject fx = Instantiate(preTeleportEffect, playerObj.position + effectOffset, Quaternion.identity);

            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            // 🔥 SHORT DELAY instead of full duration wait
            yield return new WaitForSeconds(0.3f);
        }

        // 🔥 2. FADE OUT (your QuestTeleport system)
        GameState.IsPlayerFrozen = true;

        if (fader != null)
            yield return fader.FadeOut();

        yield return new WaitForSeconds(blackScreenHoldTime);

        // 🔥 3. TELEPORT
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerObj.position = teleportTarget.position;

        var cam = FindObjectOfType<camera>();
        if (cam != null) cam.SnapToTarget();

        yield return null;
        yield return new WaitForEndOfFrame();

        if (cc != null) cc.enabled = true;

        var inter = playerObj.GetComponent<PlayerProximityInteractor>();
        if (inter != null) inter.ClearAllInteractables();

        var item = playerObj.GetComponent<PlayerItemHandler>();
        if (item != null) item.ClearNearbyItem();

        // 🔥 5. POST EFFECT
        if (postTeleportEffect != null)
        {
            GameObject fx = Instantiate(preTeleportEffect, playerObj.position + effectOffset, Quaternion.identity);

            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(fx, ps.main.duration);
            }
            else
            {
                Destroy(fx, 2f);
            }
        }

        // 🔥 4. FADE IN
        if (fader != null)
            yield return fader.FadeIn();

        GameState.IsPlayerFrozen = false;
        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;

     

        busy = false;
    }
}