using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject orbPrefab;
    public Transform firePoint;
    public Camera mainCamera;

    [Header("Attack Control")]
    public bool canAttack = false;
    public float fireRate = 0.4f;
    private float nextFireTime = 0f;

    [Header("Controller Settings")]
    public string controllerFireButton = "Fire1";

    [Header("Aim Settings")]
    [Tooltip("Set this to a Layer that only contains your ground/floor mesh.")]
    public LayerMask groundLayerMask;

    [Tooltip("Y position of your ground plane — used as fallback if raycast misses.")]
    public float groundY = 0f;

    void Update()
    {
        if (!canAttack) return;
        if (InteractionLock.DialoguePlaying) return;

        bool mouseInput = Input.GetMouseButtonDown(0);
        bool controllerInput = Input.GetButtonDown(controllerFireButton);

        if ((mouseInput || controllerInput) && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 targetPoint = GetGroundClickPoint();

        Vector3 spawnPos = firePoint.position;
        spawnPos.y -= 0.3f;    // ← tweak this value in Play mode until it lines up

        GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
        OrbProjectile projectile = orb.GetComponent<OrbProjectile>();
        projectile.SetTarget(targetPoint);
    }

    Vector3 GetGroundClickPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 1️⃣ Try to hit the ground layer specifically
        if (Physics.Raycast(ray, out hit, 300f, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        // 2️⃣ Fallback: intersect ray with a flat mathematical plane at groundY
        //    This works even if the ground has no collider in that spot
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // 3️⃣ Last resort (should never reach here with a top-down camera)
        return ray.origin + ray.direction * 50f;
    }
}