using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 5f;
    public float waitTime = 2f;

    [Header("Health")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Death")]
    public float destroyDelay = 2f;

    private Vector3 targetPosition;
    private float waitTimer;
    private bool isDead = false;

    private Animator animator;
    private EnemyHealthBar healthBar;

    void Start()
    {
        currentHP = maxHP;

        animator = GetComponent<Animator>();
        healthBar = GetComponentInChildren<EnemyHealthBar>();

        if (healthBar != null)
            healthBar.UpdateHealth(currentHP, maxHP);

        SetNewTarget();
    }

    void Update()
    {
        if (isDead) return;

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                SetNewTarget();
                waitTimer = 0f;
            }
        }
    }

    void SetNewTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0f;

        targetPosition = transform.position + randomDirection;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;

        // Update UI
        if (healthBar != null)
            healthBar.UpdateHealth(currentHP, maxHP);

        // Play hit animation
        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (animator != null)
            animator.SetTrigger("Die");

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject, destroyDelay);
    }
}