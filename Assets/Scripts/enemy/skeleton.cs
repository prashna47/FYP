using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkeletonEnemy : MonoBehaviour
{
    // -------------------------------------------------------------------------
    //  Inspector
    // -------------------------------------------------------------------------

    [Header("Health")]
    public int maxHP = 8;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public Canvas uiCanvas;

    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float stoppingDist = 2.0f;

    [Header("Wander")]
    public float wanderRadius = 5f;
    public float wanderWaitTime = 2f;
    public float chaseRange = 6f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 2.0f;
    public int attackDamage = 1;
    public float attackWindup = 0.35f;
    public float attackDuration = 0.8f;

    [Header("Hit Stun")]
    public float hitStunDuration = 0.4f; // match your Hit clip length

    [Header("Death")]
    public float destroyDelay = 2f;

    [Header("Death Effects")]
    public GameObject smokePrefab;
    public float smokeDelay = 0.1f;
    public float distortionDuration = 2f;
    [TextArea]
    public string[] corruptionLines;

    // -------------------------------------------------------------------------
    //  Private state
    // -------------------------------------------------------------------------

    private int currentHP;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isHit = false;   // frozen during hit stun

    private Animator animator;
    private Collider col;
    private Transform player;
    private EnemyHealthBar healthBar;

    private float cooldownTimer = 0f;
    private Vector2 lastDir = Vector2.down;

    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private float wanderWaitTimer = 0f;

    // -------------------------------------------------------------------------
    //  Animator hashes — change strings to match your Animator parameter names
    // -------------------------------------------------------------------------
    private static readonly int HashMoveX = Animator.StringToHash("Horizontal");
    private static readonly int HashMoveY = Animator.StringToHash("Vertical");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDie = Animator.StringToHash("Die");

    // =========================================================================
    //  Unity lifecycle
    // =========================================================================

    void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        spawnPosition = transform.position;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        SpawnHealthBar();
        SetNewWanderTarget();
    }

    void Update()
    {
        // All movement and logic stops when dead, attacking, or in hit stun
        if (isDead || isAttacking || isHit) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (player == null) { DoWander(); return; }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange && cooldownTimer <= 0f)
        {
            SetAnimatorMovement(lastDir, moving: false);
            StartCoroutine(DoAttack());
        }
        else if (dist <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            DoWander();
        }
    }

    // =========================================================================
    //  Wander
    // =========================================================================

    void DoWander()
    {
        float dist = Vector3.Distance(transform.position, wanderTarget);

        if (dist > 0.2f)
        {
            Vector3 dir3 = (wanderTarget - transform.position).normalized;
            transform.position = Vector3.MoveTowards(
                transform.position, wanderTarget, moveSpeed * 0.6f * Time.deltaTime);

            Vector2 dir8 = SnapTo8Dir(new Vector2(dir3.x, dir3.z));
            lastDir = dir8;
            SetAnimatorMovement(dir8, moving: true);
        }
        else
        {
            SetAnimatorMovement(lastDir, moving: false);
            wanderWaitTimer += Time.deltaTime;
            if (wanderWaitTimer >= wanderWaitTime)
            {
                SetNewWanderTarget();
                wanderWaitTimer = 0f;
            }
        }
    }

    void SetNewWanderTarget()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPosition + new Vector3(rand.x, 0f, rand.y);
        wanderTarget.y = transform.position.y;
    }

    // =========================================================================
    //  Chase
    // =========================================================================

    void ChasePlayer()
    {
        Vector3 playerPos = player.position;
        playerPos.y = transform.position.y;

        Vector3 toPlayer = playerPos - transform.position;
        float dist = toPlayer.magnitude;

        if (dist <= stoppingDist)
        {
            SetAnimatorMovement(lastDir, moving: false);
            return;
        }

        Vector3 targetPos = playerPos - toPlayer.normalized * stoppingDist;
        Vector3 dir3 = (targetPos - transform.position).normalized;

        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, moveSpeed * Time.deltaTime);

        Vector2 dir8 = SnapTo8Dir(new Vector2(dir3.x, dir3.z));
        lastDir = dir8;
        SetAnimatorMovement(dir8, moving: true);
    }

    // =========================================================================
    //  Attack
    // =========================================================================

    IEnumerator DoAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        if (player != null)
        {
            Vector2 toPlayer = new Vector2(
                player.position.x - transform.position.x,
                player.position.z - transform.position.z);
            lastDir = SnapTo8Dir(toPlayer);
        }

        animator.SetFloat(HashMoveX, lastDir.x);
        animator.SetFloat(HashMoveY, lastDir.y);
        animator.SetFloat(HashSpeed, 0f);

        yield return null; // let blend tree settle before firing trigger

        animator.SetTrigger(HashAttack);

        yield return new WaitForSeconds(attackWindup);
        TryDealDamage();

        yield return new WaitForSeconds(attackDuration - attackWindup);

        isAttacking = false;
    }

    void TryDealDamage()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange * 1.5f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(attackDamage, transform.position);
        }
    }

    // =========================================================================
    //  Damage / Death
    // =========================================================================

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        if (healthBar != null) healthBar.ShowHit(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitStun());
        }
    }

    IEnumerator HitStun()
    {
        isHit = true;

        // Stop all movement params before playing hit
        animator.SetFloat(HashSpeed, 0f);
        animator.SetFloat(HashMoveX, lastDir.x);
        animator.SetFloat(HashMoveY, lastDir.y);
        animator.SetTrigger(HashHit);

        yield return new WaitForSeconds(hitStunDuration);

        isHit = false;
    }

    void Die()
    {
        animator.SetBool("IsDead", true);

        if (isDead) return;
        isDead = true;

        // Stop everything cleanly before triggering death
        StopAllCoroutines();
        isAttacking = false;
        isHit = false;

        // Zero out movement so no blend tree state bleeds into death
        animator.SetFloat(HashSpeed, 0f);
        animator.SetFloat(HashMoveX, 0f);
        animator.SetFloat(HashMoveY, 0f);

        // Clear any pending triggers that could interrupt Die
        animator.ResetTrigger(HashAttack);
        animator.ResetTrigger(HashHit);

        animator.SetTrigger(HashDie);

        if (col != null) col.enabled = false;
        if (healthBar != null) healthBar.PlayDeathAnimation();

        StartCoroutine(DeathSequence());
    }
    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(smokeDelay);

        if (smokePrefab != null)
            Instantiate(smokePrefab, transform.position, Quaternion.identity);

        if (ScreenDistortionController.Instance != null)
            ScreenDistortionController.Instance.TriggerDistortion(distortionDuration);

        yield return new WaitForSeconds(0.5f);

        if (ObjectiveDialogueUI.Instance != null &&
            corruptionLines != null && corruptionLines.Length > 0)
            ObjectiveDialogueUI.Instance.ShowDialogue(corruptionLines, true);

        // Wait for death clip to finish then kill the animator so nothing plays after
        yield return new WaitForSeconds(destroyDelay - smokeDelay - 0.5f);
        if (animator != null) animator.enabled = false;

        Destroy(gameObject, 0.1f);
    }
    // =========================================================================
    //  Helpers
    // =========================================================================

    Vector2 SnapTo8Dir(Vector2 dir)
    {
        if (dir == Vector2.zero) return lastDir;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snapped = Mathf.Round(angle / 45f) * 45f;
        float rad = snapped * Mathf.Deg2Rad;
        return new Vector2(
            Mathf.Round(Mathf.Cos(rad)),
            Mathf.Round(Mathf.Sin(rad)));
    }

    void SetAnimatorMovement(Vector2 dir, bool moving)
    {
        if (isDead) return;
        animator.SetFloat(HashMoveX, dir.x);
        animator.SetFloat(HashMoveY, dir.y);
        animator.SetFloat(HashSpeed, moving ? 1f : 0f);
    }

    // =========================================================================
    //  Health bar
    // =========================================================================

    void SpawnHealthBar()
    {
        if (healthBarPrefab == null) return;

        Canvas targetCanvas = uiCanvas;

        if (targetCanvas == null)
        {
            foreach (Canvas c in FindObjectsOfType<Canvas>())
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay ||
                    c.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    targetCanvas = c;
                    break;
                }
            }
        }

        if (targetCanvas == null)
        {
            Debug.LogWarning("[SkeletonEnemy] No Screen Space canvas found. Assign uiCanvas in Inspector.");
            return;
        }

        GameObject hb = Instantiate(healthBarPrefab, targetCanvas.transform);
        healthBar = hb.GetComponent<EnemyHealthBar>();
        if (healthBar != null)
            healthBar.SetTarget(transform);
        else
            Debug.LogWarning("[SkeletonEnemy] healthBarPrefab missing EnemyHealthBar component.");
    }

    // =========================================================================
    //  Gizmos
    // =========================================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? spawnPosition : transform.position, wanderRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}