using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 5f;

    private bool snapNextFrame = false;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset;

        if (snapNextFrame)
        {
            // Instantly move camera
            transform.position = desiredPosition;
            snapNextFrame = false;
        }
        else
        {
            // Smooth follow
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }

    // Call this when teleporting
    public void SnapToTarget()
    {
        snapNextFrame = true;
    }
}