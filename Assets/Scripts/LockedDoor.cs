using UnityEngine;
using UnityEngine.UI;

public class DoorProximityMessage : MonoBehaviour
{
    public Text messageText;   // Assign in Inspector

    private void Start()
    {
        messageText.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DoorLocked"))
        {
            messageText.text = "Door is Locked";
            messageText.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            messageText.enabled = false;
        }
    }
}
