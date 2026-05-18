using UnityEngine;

public class OrbProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float maxDistance = 20f;

    [Header("Impact")]
    public GameObject impactEffect;

    [Header("Impact")]
    public float impactYOffset = -0.3f;    // ← tweak this in Inspector until it looks right

    private Vector3 startPosition;
    private Vector3 moveDirection;
    private Vector3 targetPoint;        // ← store the actual target
    private bool hasTarget = false;

    public void SetTarget(Vector3 target)
    {
        targetPoint = target;
        hasTarget = true;
        startPosition = transform.position;

        // Calculate direction from THIS object's actual world center, not firePoint
        Vector3 from = transform.position;
        from.y = target.y;    // ← flatten both to same Y before calculating direction

        Vector3 direction = target - from;

        if (direction != Vector3.zero)
        {
            moveDirection = direction.normalized;
            transform.forward = moveDirection;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (GameState.IsPaused) return;

        transform.position += moveDirection * speed * Time.deltaTime;

        // Stop exactly at the target point
        if (hasTarget)
        {
            Vector3 flat = transform.position;
            flat.y = targetPoint.y;

            if (Vector3.Distance(flat, targetPoint) < 0.1f)
            {
                Impact();
                return;
            }
        }

        // Fallback: max distance cap
        if (Vector3.Distance(transform.position, startPosition) >= maxDistance)
        {
            Impact();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        // Existing enemy
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);
            Impact();
            return;
        }

        // 🔥 ADD THIS
        SkeletonEnemy skeleton = collision.gameObject.GetComponent<SkeletonEnemy>();
        if (skeleton != null)
        {
            skeleton.TakeDamage(1);
            Impact();
            return;
        }

        Impact();
    }

    void Impact()
    {
        if (impactEffect != null)
        {
            Vector3 impactPos = transform.position;
            impactPos.y = targetPoint.y + impactYOffset;   // ← apply offset

            GameObject effect = Instantiate(impactEffect, impactPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }
}