using UnityEngine;

public class OrbProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float maxDistance = 20f; // destroy after travelling this far

    [Header("Impact")]
    public GameObject impactEffect;

    private Vector3 startPosition;
    private Vector3 moveDirection;

    public void SetTarget(Vector3 target)
    {
        target.y = transform.position.y;

        Vector3 direction = target - transform.position;
        if (direction != Vector3.zero)
        {
            moveDirection = direction.normalized;
            transform.forward = moveDirection;
        }
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, startPosition) >= maxDistance)
        {
            Impact();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);
        }

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