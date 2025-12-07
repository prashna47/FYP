using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform player;     // Reference to the player
    public Vector3 offset = new Vector3(0, 10, -10); // Adjust height/distance
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }
}
