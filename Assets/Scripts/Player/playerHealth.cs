using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;
    private int currentHP;

    [Header("UI (optional)")]
    public Slider healthSlider;   // drag a UI Slider here if you want a health bar
    public Text healthText;       // or a Text label — both are optional

    [Header("Feedback")]
    public float invincibilityTime = 0.5f;   // brief iframe after being hit
    private float invincibilityTimer = 0f;

    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (invincibilityTimer > 0f) return;   // still in iframes, ignore hit

        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        invincibilityTimer = invincibilityTime;

        UpdateUI();
        Debug.Log($"Player took {damage} damage — HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }

        if (healthText != null)
            healthText.text = $"{currentHP} / {maxHP}";
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // ── put your death logic here ──────────────────────────────
        // e.g. reload scene:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        //
        // e.g. trigger death animation:
        // GetComponent<Animator>().SetTrigger("Die");
        //
        // e.g. show game over screen:
        // GameManager.Instance.ShowGameOver();
    }
}