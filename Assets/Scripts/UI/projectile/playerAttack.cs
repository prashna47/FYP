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
    public string controllerFireButton = "Fire1"; // default Unity input (RT / X / Square)

    void Update()
    {
        if (!canAttack) return;
        if (InteractionLock.DialoguePlaying) return;

        // 🖱 Mouse (Left Click)
        bool mouseInput = Input.GetMouseButtonDown(0);

        // 🎮 Controller (RT / Fire1)
        bool controllerInput = Input.GetButtonDown(controllerFireButton);

        if ((mouseInput || controllerInput) && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 50f;
        }

        GameObject orb = Instantiate(orbPrefab, firePoint.position, Quaternion.identity);
        OrbProjectile projectile = orb.GetComponent<OrbProjectile>();
        projectile.SetTarget(targetPoint);
    }
}