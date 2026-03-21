using UnityEngine;
using UnityEngine.Analytics;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 5;
    private int currentHP;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    // Drag the dedicated UI Canvas (Screen Space Overlay, Sort Order 10+) here
    public Canvas uiCanvas;

    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 5f;
    public float waitTime = 2f;

    [Header("Death")]
    public float destroyDelay = 2f;

    private EnemyHealthBar healthBar;
    private Animator animator;
    private bool isDead = false;
    private Vector3 targetPosition;
    private float waitTimer;

    void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();

        SpawnHealthBar();
        SetNewTarget();
    }

    void SpawnHealthBar()
    {
        if (healthBarPrefab == null) return;

        // Use the dedicated overlay canvas to bypass your pixelation shader
        Canvas targetCanvas = uiCanvas != null ? uiCanvas : FindObjectOfType<Canvas>();
        if (targetCanvas == null) return;

        GameObject hb = Instantiate(healthBarPrefab, targetCanvas.transform);
        healthBar = hb.GetComponent<EnemyHealthBar>();
        healthBar.SetTarget(transform);
    }

    void Update()
    {
        if (isDead) return;

        float dist = Vector3.Distance(transform.position, targetPosition);
        if (dist > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime) { SetNewTarget(); waitTimer = 0f; }
        }
    }

    void SetNewTarget()
    {
        Vector3 dir = Random.insideUnitSphere * wanderRadius;
        dir.y = 0f;
        targetPosition = transform.position + dir;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);

        if (healthBar != null)
            healthBar.ShowHit(currentHP, maxHP);

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        if (animator != null)
            animator.SetTrigger("Die");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Tell the health bar to animate its own death
        if (healthBar != null)
            healthBar.PlayDeathAnimation();

        Destroy(gameObject, destroyDelay);
    }
}
