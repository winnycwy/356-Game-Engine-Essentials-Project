using UnityEngine;
using Ilumisoft.HealthSystem;
using UnityEngine.UI;
using System.Collections;

public class BeeHealthController : MonoBehaviour
{
    [Header("Health Bar")]
    public GameObject healthBarPrefab;

    private Health health;
    private Image healthBarFill;
    private GameObject healthBarCanvas;
    private RectTransform fillTransform;
    private float originalFillWidth;

    // Events for other scripts to listen to
    public System.Action OnBeeDeath;

    void Start()
    {
        health = GetComponent<Health>();
        CreateHealthBar();

        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged;
            health.OnHealthEmpty += OnDeath;
        }
    }

    private void CreateHealthBar()
    {
        // Auto-create health bar above the bee
        healthBarCanvas = new GameObject("HealthBarCanvas");
        healthBarCanvas.transform.SetParent(transform);
        healthBarCanvas.transform.localPosition = new Vector3(0, 1.5f, 0);

        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarCanvas.transform);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.red;

        // Set background size
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(100, 10); // Width: 100, Height: 10

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform); // Make fill a child of background
        fill.transform.localPosition = Vector3.zero;

        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = Color.green;

        // Set fill size and store original width
        fillTransform = fill.GetComponent<RectTransform>();
        fillTransform.sizeDelta = new Vector2(100, 10); // Start full width
        fillTransform.anchorMin = new Vector2(0, 0); // Anchor to left
        fillTransform.anchorMax = new Vector2(0, 1); // Anchor to left
        fillTransform.pivot = new Vector2(0, 0.5f); // Pivot on left center

        originalFillWidth = 100f; // Store the full width

        // Hide initially (only show when damaged)
        healthBarCanvas.SetActive(false);
    }

    private void OnHealthChanged(float changeAmount)
    {
        UpdateHealthBar();

        if (changeAmount < 0 && healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null && fillTransform != null && health != null)
        {
            float healthPercent = health.CurrentHealth / health.MaxHealth;

            // Update fill width based on health percentage
            float newWidth = originalFillWidth * healthPercent;
            fillTransform.sizeDelta = new Vector2(newWidth, fillTransform.sizeDelta.y);

            Debug.Log($"🐝 Health: {health.CurrentHealth}/{health.MaxHealth} ({healthPercent * 100}%) - Fill width: {newWidth}");
        }
    }

    private void OnDeath()
    {
        Debug.Log("🐝 Bee died! Starting death sequence...");

        // Start coroutine for death effects
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Optional: Play death animation or effects here
        Debug.Log("🐝 Playing death effects...");

        // Wait a frame to ensure everything is processed
        yield return null;

        // Notify other scripts that bee died
        OnBeeDeath?.Invoke();

        // ✅ Destroy the entire bee GameObject
        Debug.Log("🐝 Destroying bee...");
        Destroy(gameObject);
    }

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
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
            health.OnHealthEmpty -= OnDeath;
        }
    }
}