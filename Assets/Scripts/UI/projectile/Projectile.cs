using UnityEngine;

public class OrbProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 5f;

    [Header("Impact")]
    public GameObject impactEffect;

    private Vector3 targetPosition;
    private bool hasTarget = false;

    public void SetTarget(Vector3 target)
    {
        // Lock the height so the projectile never goes up/down
        target.y = transform.position.y;

        targetPosition = target;
        hasTarget = true;

        Vector3 direction = targetPosition - transform.position;

        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!hasTarget) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Impact();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            return;

        Impact();
    }

    void Impact()
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }
}