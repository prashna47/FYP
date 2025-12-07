using UnityEngine;
using TMPro;

public class PlayerItemHandler : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text interactText;          // Shared UI text

    [Header("Prompts")]
    [Tooltip("Text shown when you can pick up an item.")]
    public string pickupPrompt = "Press [F] to pick up";

    [Tooltip("Text shown when you are holding an item and can drop it.")]
    public string dropPrompt = "Press [G] to drop";

    [Header("Item Handling")]
    public Transform dropPoint;            // Where item will appear when dropped
    public KeyCode pickupKey = KeyCode.F;
    public KeyCode dropKey = KeyCode.G;

    private bool nearItem = false;
    private GameObject currentItem;        // The item we are near
    private GameObject carriedItem;        // The item currently held (if any)

    public bool IsHoldingItem => carriedItem != null;

    private void Start()
    {
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // PICK UP
        if (nearItem && carriedItem == null && Input.GetKeyDown(pickupKey))
        {
            PickUpItem();
        }

        // DROP
        if (carriedItem != null && Input.GetKeyDown(dropKey))
        {
            DropItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickupItem"))
        {
            currentItem = other.gameObject;
            nearItem = true;
            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PickupItem"))
        {
            nearItem = false;
            currentItem = null;
            UpdateUI();
        }
    }

    private void PickUpItem()
    {
        carriedItem = currentItem;

        // Hide / move item
        if (carriedItem != null)
        {
            carriedItem.SetActive(false);
            Debug.Log("Picked up: " + carriedItem.name);
        }

        UpdateUI();
    }

    private void DropItem()
    {
        if (carriedItem == null) return;

        // Place item at drop point
        if (dropPoint != null)
        {
            carriedItem.transform.position = dropPoint.position;
        }

        carriedItem.SetActive(true);
        Debug.Log("Dropped: " + carriedItem.name);

        carriedItem = null;
        UpdateUI();
    }

    /// <summary>
    /// Central place to control what the interactText shows.
    /// </summary>
    private void UpdateUI()
    {
        if (interactText == null) return;

        if (carriedItem != null)
        {
            // Holding an item: show drop prompt
            interactText.text = dropPrompt;
            interactText.gameObject.SetActive(true);
        }
        else if (nearItem && currentItem != null)
        {
            // Near an item and hands are empty: show pickup prompt
            interactText.text = pickupPrompt;
            interactText.gameObject.SetActive(true);
        }
        else
        {
            // Nothing relevant: hide text
            interactText.gameObject.SetActive(false);
        }
    }

    // Optional external API if you ever want to call these directly:
    public void PickUp(GameObject item)
    {
        currentItem = item;
        nearItem = true;
        PickUpItem();
    }

    public void Drop(Vector3 dropPosition)
    {
        if (carriedItem == null) return;

        carriedItem.transform.position = dropPosition;
        carriedItem.SetActive(true);
        carriedItem = null;
        UpdateUI();
    }
}
