using UnityEngine;

public class camera : MonoBehaviour
{
    private Transform player;
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 5f;
    private bool snapNextFrame = false;

    // --- Camera Pan ---
    private bool isPanning = false;
    private Vector3 panTargetPosition;
    private float panDuration;
    private float panHoldTime;
    private float panTimer;
    private enum PanState { MovingTo, Holding, Returning }
    private PanState panState;

    void LateUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindPlayer();
            if (player == null) return;
        }

        if (isPanning)
        {
            HandlePan();
            if (CameraShake.Instance != null)
                transform.position += CameraShake.Instance.ShakeOffset;
            transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            return;
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

        if (CameraShake.Instance != null)
            transform.position += CameraShake.Instance.ShakeOffset;
        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }

    void HandlePan()
    {
        switch (panState)
        {
            case PanState.MovingTo:
                transform.position = Vector3.Lerp(transform.position, panTargetPosition, smoothSpeed * Time.deltaTime);
                // Close enough — start holding
                if (Vector3.Distance(transform.position, panTargetPosition) < 0.05f)
                {
                    transform.position = panTargetPosition;
                    panTimer = 0f;
                    panState = PanState.Holding;
                }
                break;

            case PanState.Holding:
                panTimer += Time.deltaTime;
                if (panTimer >= panHoldTime)
                    panState = PanState.Returning;
                break;

            case PanState.Returning:
                Vector3 returnTarget = player.position + offset;
                transform.position = Vector3.Lerp(transform.position, returnTarget, smoothSpeed * Time.deltaTime);
                // Close enough — resume normal follow
                if (Vector3.Distance(transform.position, returnTarget) < 0.1f)
                    isPanning = false;
                break;
        }
    }

    /// <summary>
    /// Call this to pan the camera to a world position, hold, then return.
    /// </summary>
    public void PanToPosition(Vector3 targetWorldPos, float holdTime = 2f)
    {
        panTargetPosition = targetWorldPos;
        panHoldTime = holdTime;
        panTimer = 0f;
        panState = PanState.MovingTo;
        isPanning = true;
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