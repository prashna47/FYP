using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 5;
    private int currentHP;

    [Header("Death Effects")]
    public GameObject smokePrefab;
    public float smokeDelay = 0.1f;
    public float distortionDuration = 2f;

    [TextArea]
    public string[] corruptionLines;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public Canvas uiCanvas;

    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 5f;
    public float waitTime = 2f;

    [Header("Attack")]
    public float attackRange = 3f;
    public float attackCooldown = 3f;
    public int attackDamage = 1;
    public float jumpHeight = 3f;
    public float jumpDuration = 0.6f;
    public float postAttackWanderTime = 3f;

    [Header("Death")]
    public float destroyDelay = 2f;

    private EnemyHealthBar healthBar;
    private Animator animator;
    private Collider col;
    private Transform player;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isPostAttackWander = false;

    private Vector3 spawnPosition;
    private Vector3 targetPosition;
    private float waitTimer;

    private float cooldownTimer = 0f;
    private float postAttackWanderTimer = 0f;
    private float jumpTimer = 0f;
    private Vector3 jumpStartPos;
    private Vector3 jumpEndPos;
    private bool damageDealt = false;

    private static readonly int HashJumpStart = Animator.StringToHash("JumpStart");
    private static readonly int HashJumpUp = Animator.StringToHash("JumpUp");
    private static readonly int HashJumpUpToDown = Animator.StringToHash("JumpUpToDown");
    private static readonly int HashJumpDown = Animator.StringToHash("JumpDown");
    private static readonly int HashJumpLand = Animator.StringToHash("JumpLand");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDie = Animator.StringToHash("Die");

    void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        spawnPosition = transform.position;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        SpawnHealthBar();
        SetNewTarget();
    }

    void Update()
    {
        if (isDead) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isAttacking)
        {
            UpdateJumpAttack();
            return;
        }

        if (isPostAttackWander)
        {
            postAttackWanderTimer += Time.deltaTime;
            if (postAttackWanderTimer >= postAttackWanderTime)
            {
                isPostAttackWander = false;
                postAttackWanderTimer = 0f;
            }
            DoWander();
            return;
        }

        if (player != null && cooldownTimer <= 0f)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= attackRange)
            {
                StartJumpAttack();
                return;
            }
        }

        DoWander();
    }
    IEnumerator DeathSequence()
    {
        // small delay so animation starts first
        yield return new WaitForSeconds(smokeDelay);

        // 🌫 Spawn smoke
        if (smokePrefab != null)
        {
            Instantiate(smokePrefab, transform.position, Quaternion.identity);
        }

        // 🎥 Trigger distortion
        if (ScreenDistortionController.Instance != null)
        {
            ScreenDistortionController.Instance.TriggerDistortion(distortionDuration);
        }

        // 🧠 Delay before dialogue (feels natural)
        yield return new WaitForSeconds(0.5f);

        // 🧠 Trigger corruption dialogue
        if (ObjectiveDialogueUI.Instance != null && corruptionLines != null && corruptionLines.Length > 0)
        {
            ObjectiveDialogueUI.Instance.ShowDialogue(corruptionLines, true);
        }
    }
    void DoWander()
    {
        float dist = Vector3.Distance(transform.position, targetPosition);
        if (dist > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPosition, moveSpeed * Time.deltaTime);

            float dir = targetPosition.x - transform.position.x;
            if (Mathf.Abs(dir) > 0.01f)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * (dir > 0 ? 1f : -1f);
                transform.localScale = s;
            }
        }
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime) { SetNewTarget(); waitTimer = 0f; }
        }
    }

    void StartJumpAttack()
    {
        isAttacking = true;
        damageDealt = false;
        jumpTimer = 0f;
        jumpStartPos = transform.position;
        jumpEndPos = player.position;
        jumpEndPos.y = transform.position.y;

        float dir = jumpEndPos.x - transform.position.x;
        if (Mathf.Abs(dir) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir > 0 ? 1f : -1f);
            transform.localScale = s;
        }

        animator.SetTrigger(HashJumpStart);
    }

    void UpdateJumpAttack()
    {
        jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        Vector3 flatPos = Vector3.Lerp(jumpStartPos, jumpEndPos, t);
        flatPos.y = Mathf.Lerp(jumpStartPos.y, jumpEndPos.y, t)
                          + jumpHeight * Mathf.Sin(t * Mathf.PI);
        transform.position = flatPos;

        if (t < 0.25f) animator.SetTrigger(HashJumpUp);
        else if (t < 0.50f) animator.SetTrigger(HashJumpUpToDown);
        else if (t < 0.90f) animator.SetTrigger(HashJumpDown);
        else animator.SetTrigger(HashJumpLand);

        // guaranteed damage at landing — no distance check
        if (t >= 0.90f && !damageDealt)
        {
            damageDealt = true;
            DealDamageGuaranteed();
        }

        if (t >= 1f)
        {
            transform.position = jumpEndPos;
            isAttacking = false;
            cooldownTimer = attackCooldown;
            isPostAttackWander = true;
            postAttackWanderTimer = 0f;
            SetNewTarget();
        }
    }

    void DealDamageGuaranteed()
    {
        if (player == null) return;
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(attackDamage, transform.position);
    }

    void SetNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPosition = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        if (healthBar != null) healthBar.ShowHit(currentHP, maxHP);
        if (animator != null) animator.SetTrigger(HashHit);
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null) animator.SetTrigger(HashDie);
        if (col != null) col.enabled = false;
        if (healthBar != null) healthBar.PlayDeathAnimation();

        StartCoroutine(DeathSequence());

        Destroy(gameObject, destroyDelay);
    }



    void SpawnHealthBar()
    {
        if (healthBarPrefab == null) return;
        Canvas targetCanvas = uiCanvas != null ? uiCanvas : FindObjectOfType<Canvas>();
        if (targetCanvas == null) return;
        GameObject hb = Instantiate(healthBarPrefab, targetCanvas.transform);
        healthBar = hb.GetComponent<EnemyHealthBar>();
        healthBar.SetTarget(transform);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? spawnPosition : transform.position, wanderRadius);
    }


}
