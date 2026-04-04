using UnityEngine;

public class camera : MonoBehaviour
{
    private Transform player;
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 5f;
    private bool snapNextFrame = false;

    void LateUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindPlayer();
            if (player == null) return;
        }

        Vector3 desiredPosition = player.position + offset;

        if (snapNextFrame)
        {
            transform.position = desiredPosition;
            snapNextFrame = false;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }

        // Apply shake on top — won't fight the follow logic
        if (CameraShake.Instance != null)
            transform.position += CameraShake.Instance.ShakeOffset;

        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }

    void FindPlayer()
    {
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
    }

    public void SnapToTarget()
    {
        snapNextFrame = true;
    }
}