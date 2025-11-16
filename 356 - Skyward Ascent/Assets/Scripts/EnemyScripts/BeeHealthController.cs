using UnityEngine;
using Ilumisoft.HealthSystem;
using UnityEngine.UI;
using System.Collections;

public class BeeHealthController : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public float healthBarWidth = 1f;
    public float healthBarHeight = 0.1f;
    public float healthBarOffsetY = 0.3f;

    private Health health;
    private Image healthBarFill;
    private GameObject healthBarCanvas;
    private RectTransform fillTransform;
    private Image healthBarBackground;

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
        // Create health bar canvas
        healthBarCanvas = new GameObject("HealthBarCanvas");
        healthBarCanvas.transform.SetParent(transform);
        healthBarCanvas.transform.localPosition = new Vector3(0, healthBarOffsetY, 0);
        healthBarCanvas.transform.localRotation = Quaternion.identity;

        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = healthBarCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(healthBarWidth * 100, healthBarHeight * 100);

        // Create background (DARK color - this shows when health decreases)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarCanvas.transform);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = Vector3.one;

        healthBarBackground = background.AddComponent<Image>();
        healthBarBackground.color = new Color(0.3f, 0, 0, 0.8f); // DARK red background

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(healthBarWidth * 100, healthBarHeight * 100);

        // Create fill (RED color - this shrinks as health decreases)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = Vector3.one; // Start full

        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(1, 0, 0, 0.8f); // BRIGHT red fill

        fillTransform = fill.GetComponent<RectTransform>();
        fillTransform.anchorMin = new Vector2(0, 0);
        fillTransform.anchorMax = new Vector2(1, 1);
        fillTransform.offsetMin = Vector2.zero;
        fillTransform.offsetMax = Vector2.zero;

        // Hide initially
        healthBarCanvas.SetActive(false);
    }

    private void OnHealthChanged(float changeAmount)
    {
        UpdateHealthBar();

        // Show health bar when taking damage
        if (changeAmount < 0 && healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null && health != null)
        {
            float healthPercent = health.CurrentHealth / health.MaxHealth;

            // ✅ FIX: Scale the RED fill based on health
            // When health is full: scale = 1 (full red bar)
            // When health is low: scale approaches 0 (less red, more dark background shows)
            healthBarFill.transform.localScale = new Vector3(healthPercent, 1, 1);

            Debug.Log($"🐝 Health: {health.CurrentHealth}/{health.MaxHealth} ({healthPercent * 100}%) - Fill scale: {healthPercent}");
        }
    }

    private void OnDeath()
    {
        Debug.Log("🐝 Bee died!");
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return null;
        OnBeeDeath?.Invoke();
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