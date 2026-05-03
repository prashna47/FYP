using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;
    private int currentHP;

    [Header("UI")]
    public PlayerHealthBar healthBar;

    [Header("Hit Effects")]
    public HitEffects hitEffects;

    void Start()
    {
        currentHP = maxHP;
        if (healthBar != null) healthBar.UpdateHealth(currentHP, maxHP);
    }

    // Enemies that pass their position (recommended)
    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        if (currentHP <= 0) return;
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        if (healthBar != null) healthBar.UpdateHealth(currentHP, maxHP);
        if (hitEffects != null) hitEffects.PlayHitEffects(attackerPosition);
        if (currentHP <= 0) Die();
    }

    // Fallback overload so old TakeDamage(int) calls still compile
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    void Die()
    {
        Debug.Log("Player died.");
    }
    public void HealToFull()
    {
        currentHP = maxHP;
        if (healthBar != null) healthBar.UpdateHealth(currentHP, maxHP);
    }
}