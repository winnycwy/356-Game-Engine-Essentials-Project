using UnityEngine;
using Ilumisoft.HealthSystem;
using UnityEngine.UI;

public class BeeHealthController : MonoBehaviour
{
    [Header("Health References")]
    public Health health;

    [Header("Health Bar UI")]
    public Slider healthBarSlider;
    public GameObject healthBarCanvas;

    [Header("Death Effects")]
    public ParticleSystem deathParticles;
    public AudioClip deathSound;

    // Events for other scripts to listen to
    public System.Action OnBeeDeath;
    public System.Action<float> OnBeeHealthChanged;

    void Start()
    {
        // Get Health component if not assigned
        if (health == null)
            health = GetComponent<Health>();

        // Setup health system
        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged;
            health.OnHealthEmpty += OnDeath;
        }

        // Setup health bar
        if (healthBarSlider == null && healthBarCanvas != null)
            healthBarSlider = healthBarCanvas.GetComponentInChildren<Slider>();

        UpdateHealthBar();
    }

    private void OnHealthChanged(float changeAmount)
    {
        UpdateHealthBar();
        OnBeeHealthChanged?.Invoke(changeAmount);
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null && health != null)
        {
            healthBarSlider.value = health.CurrentHealth / health.MaxHealth;

            // Hide health bar when full health
            if (healthBarCanvas != null)
            {
                healthBarCanvas.SetActive(health.CurrentHealth < health.MaxHealth);
            }
        }
    }

    private void OnDeath()
    {
        Debug.Log("Bee died!");

        // Play death effects
        if (deathParticles != null)
            Instantiate(deathParticles, transform.position, Quaternion.identity);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Notify other scripts that bee died
        OnBeeDeath?.Invoke();

        // Destroy the bee
        Destroy(gameObject, 0.1f);
    }

    // Public method for other scripts to damage the bee
    public void TakeDamage(float damage)
    {
        if (health != null && health.IsAlive)
        {
            health.ApplyDamage(damage);
        }
    }

    public bool IsAlive()
    {
        return health != null && health.IsAlive;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
            health.OnHealthEmpty -= OnDeath;
        }
    }
}