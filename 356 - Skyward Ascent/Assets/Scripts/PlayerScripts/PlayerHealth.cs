using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;  // The green/red fill image
    public TMP_Text healthText;  // The text showing current health
    public Camera mainCamera;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        UpdateHealthBar();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        // Make the health bar face the camera
        if (healthSlider != null && mainCamera != null)
        {
            Vector3 dir = healthSlider.transform.position - mainCamera.transform.position;
            healthSlider.transform.rotation = Quaternion.LookRotation(dir);
        }

        // Test input
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.T))
            Heal(10);
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (fillImage != null)
        {
            float t = currentHealth / maxHealth;
            fillImage.color = Color.Lerp(Color.red, Color.green, t);
        }

        if (healthText != null)
            healthText.text = Mathf.RoundToInt(currentHealth) + " / " + Mathf.RoundToInt(maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    void Die()
    {
        Debug.Log("Player Died!");
    }
}
