using UnityEngine;

public class OrbProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float maxDistance = 20f;

    [Header("Impact")]
    public GameObject impactEffect;
    public float impactYOffset = -0.3f;

    private Vector3 startPosition;
    private Vector3 moveDirection;
    private Vector3 targetPoint;
    private bool hasTarget = false;
    private bool hasDealtDamage = false;
    private bool hasImpacted = false;   // guard against double Impact()

    public void SetTarget(Vector3 target)
    {
        targetPoint = target;
        hasTarget = true;
        startPosition = transform.position;

        Vector3 from = transform.position;
        from.y = target.y;
        Vector3 dir = target - from;

        if (dir != Vector3.zero)
        {
            moveDirection = dir.normalized;
            transform.forward = moveDirection;
        }
    }

    void Update()
    {
        if (GameState.IsPaused) return;
        if (hasImpacted) return;

        transform.position += moveDirection * speed * Time.deltaTime;

        if (hasTarget)
        {
            Vector3 flat = transform.position;
            flat.y = targetPoint.y;
            if (Vector3.Distance(flat, targetPoint) < 0.1f) { Impact(); return; }
        }

        if (Vector3.Distance(transform.position, startPosition) >= maxDistance)
            Impact();
    }

    // ── Solid colliders — existing enemies / walls ────────────────────────────
    void OnCollisionEnter(Collision col)
    {
        if (hasImpacted) return;
        if (col.gameObject.CompareTag("Player")) return;

        // Never handle Mimic here — it uses the trigger path below
        if (col.gameObject.GetComponentInParent<MimicSpace.MimicEnemy>() != null) return;

        Enemy enemy = col.gameObject.GetComponent<Enemy>();
        if (enemy != null) { enemy.TakeDamage(1); Impact(); return; }

        SkeletonEnemy skeleton = col.gameObject.GetComponent<SkeletonEnemy>();
        if (skeleton != null) { skeleton.TakeDamage(1); Impact(); return; }

        Impact();
    }

    // ── Trigger colliders — MimicHitbox ──────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted || hasDealtDamage) return;

        Debug.Log($"[Orb] OnTriggerEnter with: {other.gameObject.name} layer={other.gameObject.layer}");

        MimicSpace.MimicHitboxReceiver recv = other.GetComponent<MimicSpace.MimicHitboxReceiver>();
        if (recv != null)
        {
            Debug.Log("[Orb] Hit MimicHitbox — dealing damage.");
            hasDealtDamage = true;
            recv.owner?.TakeDamage(1);
            Impact();
        }
    }

    void Impact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        if (impactEffect != null)
        {
            Vector3 pos = transform.position;
            pos.y = targetPoint.y + impactYOffset;
            GameObject fx = Instantiate(impactEffect, pos, Quaternion.identity);
            Destroy(fx, 2f);
        }
        Destroy(gameObject);
    }
}