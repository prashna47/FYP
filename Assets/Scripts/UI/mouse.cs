using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // Make cursor visible
        Cursor.visible = true;

        // Ensure cursor is not locked to the center
        Cursor.lockState = CursorLockMode.None;
    }
}