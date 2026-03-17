using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject orbPrefab;
    public Transform firePoint;
    public Camera mainCamera;
    public bool canAttack = false;

    public float fireRate = 0.4f;
    private float nextFireTime = 0f;

    void Update()
    {
        if (!canAttack) return; // ❌ block attack outside zone

        if (Input.GetMouseButtonDown(0) && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetPoint = hit.point;

            GameObject orb = Instantiate(orbPrefab, firePoint.position, Quaternion.identity);

            OrbProjectile projectile = orb.GetComponent<OrbProjectile>();

            projectile.SetTarget(targetPoint);
        }
    }
}