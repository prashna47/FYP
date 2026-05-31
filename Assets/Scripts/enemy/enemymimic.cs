using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MimicSpace
{
    [RequireComponent(typeof(Mimic))]
    public class MimicEnemy : MonoBehaviour
    {
        // ── Movement ──────────────────────────────────────────────────────────
        [Header("Body Height")]
        [Range(0.5f, 5f)] public float height = 0.8f;
        public float velocityLerpCoef = 4f;

        [Header("Movement")]
        public float moveSpeed = 2.5f;
        public float chaseSpeed = 4f;

        [Header("Respawn Grace Period")]
        [Tooltip("Seconds after respawn before the mimic can move and deal damage")]
        public float respawnGraceDuration = 3f;
        private float respawnGracePeriod = 3f; // customizable in inspector
        private bool inGracePeriod = false;

        [Header("Death Collapse")]
        public float collapseSpeed = 3f; // how fast it sinks to ground

        [Header("Objective / Quest")]
        public int objectiveIndex;
        public int stepIndex = 0;
        public int respawnStepIndex = 1; // the step index to check on second death

        [Header("Camera Pan on Respawn")]
        public Transform cameraPanTarget;
        public float holdTime = 2f;

        private int deathCount = 0;

        [Header("Wander")]
        public float wanderRadius = 6f;
        public float wanderWaitTime = 2f;

        [Header("Detection")]
        public float chaseRange = 8f;
        public float attackRange = 2.5f;
        [Tooltip("How long the Mimic keeps chasing after losing the player or being hit from afar")]
        public float alertDuration = 5f;

        // ── Health ────────────────────────────────────────────────────────────
        [Header("Health")]
        public int maxHP = 10;

        [Header("Death")]
        public float destroyDelay = 2f;
        public GameObject smokePrefab;

        // ── Hitbox ────────────────────────────────────────────────────────────
        [Header("Hitbox")]
        [Tooltip("Layer number of the 'MimicHitbox' layer (Tags and Layers window)")]
        public int mimicHitboxLayer = 7;

        public enum HitboxShape { Sphere, Box, Capsule }
        public HitboxShape hitboxShape = HitboxShape.Sphere;

        public float hitboxRadius = 1f;
        public Vector3 hitboxBoxSize = new Vector3(2f, 1.5f, 2f);
        public float hitboxCapsuleHeight = 2f;
        public Vector3 hitboxOffset = Vector3.zero;
        private float originalHeight;

        // ── Leg attack ────────────────────────────────────────────────────────
        [Header("Leg Attack")]
        public int attackDamage = 1;
        public float damageCooldown = 1f;
        public float legTipRadius = 0.35f;

        // ── UI ────────────────────────────────────────────────────────────────
        [Header("Health Bar UI")]
        public MimicHealthBar healthBar;

        // ── Private ───────────────────────────────────────────────────────────
        private Mimic myMimic;
        private Transform player;
        private Vector3 velocity = Vector3.zero;

        private Vector3 spawnPosition;
        private Vector3 wanderTarget;
        private float wanderWaitTimer = 0f;

        private float damageTimer = 0f;
        private float alertTimer = 0f;

        private int currentHP = 0;
        private bool isDead = false;

        private enum AIState { Wander, Chase }
        private AIState state = AIState.Wander;

        private Dictionary<Leg, GameObject> legTriggers = new Dictionary<Leg, GameObject>();
        private LayerMask legRaycastMask;


        [Header("Respawn Trigger")]
        [Tooltip("The objective index whose START triggers this mimic to respawn")]
        public int respawnOnObjectiveIndex = -1;

        // remove: public int respawnStepIndex
        // remove: public float respawnDelay


        // =====================================================================

        void Start()
        {
            myMimic = GetComponent<Mimic>();
            currentHP = maxHP;
            originalHeight = height;
            spawnPosition = transform.position;
            SetNewWanderTarget();

            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;

            legRaycastMask = ~(1 << mimicHitboxLayer);
            myMimic.legRaycastMask = legRaycastMask;

            CreateHitbox();

            // Subscribe to quest objective start
            QuestManager.OnObjectiveStarted += OnObjectiveStarted;
        }

        void OnDestroy()
        {
            QuestManager.OnObjectiveStarted -= OnObjectiveStarted;
        }

        void OnObjectiveStarted(int index)
        {
            if (index == respawnOnObjectiveIndex && isDead)
                StartCoroutine(Respawn());
        }

        void Update()
        {
            if (isDead) return;

            if (damageTimer > 0f) damageTimer -= Time.deltaTime;
            if (alertTimer > 0f) alertTimer -= Time.deltaTime;

            SyncLegTriggers();

            if (!inGracePeriod)  // ← add this check
                UpdateAI();

            ApplyHeight();
        }
        void RetractLegs()
        {
            // Disable all active leg triggers first
            foreach (var kv in legTriggers)
                if (kv.Value != null) kv.Value.SetActive(false);
            legTriggers.Clear();

            // Destroy all active Leg components so they visually disappear
            Leg[] activeLegs = GetComponentsInChildren<Leg>();
            foreach (Leg leg in activeLegs)
                Destroy(leg.gameObject);
        }


        void CreateHitbox()
        {
            GameObject hb = new GameObject("MimicHitbox");
            hb.transform.SetParent(transform);
            hb.transform.localPosition = hitboxOffset;
            hb.layer = mimicHitboxLayer;

            // ── Solid collider (isTrigger = FALSE) — orb bounces off this ─────
            switch (hitboxShape)
            {
                case HitboxShape.Sphere:
                    var sc = hb.AddComponent<SphereCollider>();
                    sc.isTrigger = false;   // SOLID
                    sc.radius = hitboxRadius;
                    break;

                case HitboxShape.Box:
                    var bc = hb.AddComponent<BoxCollider>();
                    bc.isTrigger = false;   // SOLID
                    bc.size = hitboxBoxSize;
                    break;

                case HitboxShape.Capsule:
                    var cc = hb.AddComponent<CapsuleCollider>();
                    cc.isTrigger = false;   // SOLID
                    cc.radius = hitboxRadius;
                    cc.height = hitboxCapsuleHeight;
                    break;
            }

            // ── Receiver script — uses OnCollisionEnter to match the orb ──────
            MimicHitboxReceiver recv = hb.AddComponent<MimicHitboxReceiver>();
            recv.owner = this;
        }

        // =====================================================================
        //  AI
        // =====================================================================

        void UpdateAI()
        {
            if (player == null) { DoWander(); return; }

            float dist = HorizontalDist(transform.position, player.position);
            bool playerInRange = dist <= chaseRange;
            bool alerted = alertTimer > 0f;

            if (state == AIState.Wander && (playerInRange || alerted))
            {
                state = AIState.Chase;
                healthBar?.Show();
            }

            if (state == AIState.Chase && !playerInRange && !alerted)
            {
                state = AIState.Wander;
                healthBar?.Hide();
            }

            if (state == AIState.Chase) DoChase(dist);
            else DoWander();
        }

        void DoWander()
        {
            Vector3 flat = new Vector3(wanderTarget.x, transform.position.y, wanderTarget.z);
            float dist = Vector3.Distance(transform.position, flat);

            if (dist > 0.3f)
            {
                SetVelocity((flat - transform.position).normalized * moveSpeed);
            }
            else
            {
                SetVelocity(Vector3.zero);
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
            wanderTarget = spawnPosition + new Vector3(rand.x, 0, rand.y);
        }

        void DoChase(float dist)
        {
            if (dist <= attackRange) { SetVelocity(Vector3.zero); return; }
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            SetVelocity(dir.normalized * chaseSpeed);
        }

        // =====================================================================
        //  Movement helpers
        // =====================================================================

        void SetVelocity(Vector3 target)
        {
            velocity = Vector3.Lerp(velocity, target, velocityLerpCoef * Time.deltaTime);
            myMimic.velocity = velocity;
            transform.position += velocity * Time.deltaTime;
        }

        void ApplyHeight()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5f, -Vector3.up,
                                out RaycastHit hit, 20f, legRaycastMask))
            {
                Vector3 dest = new Vector3(transform.position.x, hit.point.y + height, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, dest, velocityLerpCoef * Time.deltaTime);
            }
        }

        // =====================================================================
        //  Health
        // =====================================================================

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
            alertTimer = alertDuration;

            if (state == AIState.Wander)
            {
                state = AIState.Chase;
                healthBar?.Show();
            }

            healthBar?.ShowHit(currentHP, maxHP);
            Debug.Log($"[Mimic] TakeDamage({damage}) → HP now {currentHP}/{maxHP}");

            if (currentHP <= 0)
                StartCoroutine(Die());
        }

        // ...inside Die():
        IEnumerator Die()
        {
            isDead = true;
            deathCount++;
            SetVelocity(Vector3.zero);
            healthBar?.PlayDeathAnimation();

            // Retract legs immediately on death
            RetractLegs();

            // Then sink the body to the ground
            yield return StartCoroutine(CollapseToGround());

            // Stop Mimic from spawning new legs
            myMimic.enabled = false;

            if (deathCount == 1)
            {
                if (QuestManager.Instance != null)
                    QuestManager.Instance.CompleteMimicObjective();
            }
            else
            {
                if (QuestManager.Instance != null)
                    QuestManager.Instance.CompleteMimicObjective();

                yield return new WaitForSeconds(destroyDelay);
                Destroy(gameObject);
            }
        }

        IEnumerator CollapseToGround()
        {
            float startHeight = height;

            // Optionally spawn smoke at the start of collapse
            if (smokePrefab != null)
                Instantiate(smokePrefab, transform.position, Quaternion.identity);

            while (height > 0.01f)
            {
                height = Mathf.Lerp(height, 0f, collapseSpeed * Time.deltaTime);
                yield return null;
            }

            height = 0f;
        }

        IEnumerator Respawn()
        {
            isDead = false;
            currentHP = maxHP;
            damageTimer = 0f;
            alertTimer = 0f;
            wanderWaitTimer = 0f;
            velocity = Vector3.zero;
            myMimic.velocity = Vector3.zero;
            height = originalHeight;

            // Re-enable Mimic so it naturally starts growing legs again
            myMimic.enabled = true;

            state = AIState.Wander;
            spawnPosition = transform.position;
            SetNewWanderTarget();

            healthBar?.ResetBar();

            if (smokePrefab != null)
                Instantiate(smokePrefab, transform.position, Quaternion.identity);

            if (cameraPanTarget != null)
            {
                camera cam = Camera.main?.GetComponent<camera>();
                if (cam != null)
                    cam.PanToPosition(cameraPanTarget.position, holdTime);
            }

            // Grace period — mimic sits still and can't deal damage
            inGracePeriod = true;
            yield return new WaitForSeconds(respawnGraceDuration);
            inGracePeriod = false;
        }

        //  Leg tip triggers — hurt player on contact

        void SyncLegTriggers()
        {
            Leg[] activeLegs = GetComponentsInChildren<Leg>();

            List<Leg> toRemove = new List<Leg>();
            foreach (var kv in legTriggers)
                if (kv.Key == null || kv.Value == null) toRemove.Add(kv.Key);
            foreach (var leg in toRemove) legTriggers.Remove(leg);

            foreach (Leg leg in activeLegs)
            {
                if (!legTriggers.ContainsKey(leg))
                {
                    GameObject tipGO = new GameObject("LegTipTrigger");
                    tipGO.transform.SetParent(leg.transform);
                    // Use Default layer (not MimicHitbox) so this trigger collides
                    // with the Player collider normally. Leg raycasts already exclude
                    // MimicHitbox layer, and Default-vs-Default raycasts are fine
                    // because this is a trigger (no physical blocking of leg rays).
                    tipGO.layer = 0; // Default

                    SphereCollider sc = tipGO.AddComponent<SphereCollider>();
                    sc.isTrigger = true;
                    sc.radius = legTipRadius;

                    // Kinematic Rigidbody required for OnTriggerEnter to fire
                    Rigidbody rb = tipGO.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;

                    LegTipDamager dmg = tipGO.AddComponent<LegTipDamager>();
                    dmg.owner = this;

                    legTriggers[leg] = tipGO;
                }

                if (legTriggers[leg] != null)
                    legTriggers[leg].transform.position = leg.footPosition;
            }
        }

        public void TryDealDamageToPlayer(Collider other)
        {
            if (isDead) return;
            if (inGracePeriod) return;
            if (damageTimer > 0f) return;
            if (!other.CompareTag("Player")) return;

            PlayerHealth ph = other.GetComponent<PlayerHealth>()
                           ?? other.GetComponentInParent<PlayerHealth>();
            if (ph == null)
            {
                return;
            }

            Debug.Log("[Mimic] Leg touched player — dealing damage.");
            ph.TakeDamage(attackDamage, transform.position);
            damageTimer = damageCooldown;
        }

        // =====================================================================
        //  Helpers
        // =====================================================================

        float HorizontalDist(Vector3 a, Vector3 b)
        {
            a.y = 0; b.y = 0;
            return Vector3.Distance(a, b);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 origin = Application.isPlaying ? spawnPosition : transform.position;
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(origin, wanderRadius);
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, chaseRange);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    // ── Receives orb hits via solid collision (matches how skeleton works) ───
    public class MimicHitboxReceiver : MonoBehaviour
    {
        [HideInInspector] public MimicEnemy owner;

        // OnCollisionEnter fires because the hitbox is a solid (non-trigger) collider,
        // exactly like the skeleton's CapsuleCollider. The orb calls Impact() after this.
        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("Player")) return;

            OrbProjectile orb = col.gameObject.GetComponent<OrbProjectile>();
            if (orb != null)
            {
                owner?.TakeDamage(1);
                // Don't call Impact() here — the orb's own OnCollisionEnter does it
            }
        }
    }

    // ── Hurts player on leg contact ───────────────────────────────────────────
    public class LegTipDamager : MonoBehaviour
    {
        [HideInInspector] public MimicEnemy owner;

        void OnTriggerEnter(Collider other)
        {
            owner?.TryDealDamageToPlayer(other);
        }
    }
}