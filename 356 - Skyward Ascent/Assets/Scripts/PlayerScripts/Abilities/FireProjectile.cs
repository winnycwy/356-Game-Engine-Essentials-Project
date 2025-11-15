using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Fire Projectile Settings")]
    public int damage = 20;
    public float lifetime = 3f;
    public GameObject impactEffect;
    public LayerMask enemyLayer;

    [Header("Visual Effects")]
    public ParticleSystem trailParticles;
    public Light fireLight;

    private void Start()
    {
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);

        // Start visual effects
        if (trailParticles != null)
            trailParticles.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't collide with player, triggers, or other projectiles
        if (other.CompareTag("Player") || other.CompareTag("Projectile") || other.isTrigger)
            return;

        // Check if hit an enemy (using tags instead of EnemyHealth component)
        if (other.CompareTag("Enemy") || other.CompareTag("Spriteling"))
        {
            HandleEnemyHit(other.gameObject);
        }

        // Always destroy when hitting environment
        HandleImpact();
    }

    private void HandleEnemyHit(GameObject enemy)
    {
        Debug.Log($"Fire projectile hit enemy: {enemy.name}");

        // Temporary enemy damage system - will be replaced with EnemyHealth later
        // For now, we'll use a simple tag-based system or custom interface

        // Method 1: Try to find any health component (flexible approach)
        MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Check if the script has a TakeDamage method using reflection
            var takeDamageMethod = script.GetType().GetMethod("TakeDamage");
            if (takeDamageMethod != null)
            {
                takeDamageMethod.Invoke(script, new object[] { damage });
                break;
            }
        }

        // Method 2: Simple destroy for testing (uncomment if you want quick testing)
        // Destroy(enemy); // Be careful with this - only for testing!

        HandleImpact();
    }

    private void HandleImpact()
    {
        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Stop trail particles
        if (trailParticles != null)
        {
            trailParticles.Stop();
            trailParticles.transform.parent = null;
            Destroy(trailParticles.gameObject, 2f);
        }

        // Disable light and renderer for a better effect
        if (fireLight != null)
            fireLight.enabled = false;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // Destroy the projectile
        Destroy(gameObject, 0.1f); // Small delay to let impact effect play
    }

    // Optional: Add visual effects over time
    private void Update()
    {
        // Make the projectile grow slightly as it travels
        transform.localScale *= 1.001f;

        // Flicker light effect
        if (fireLight != null)
        {
            fireLight.intensity = Mathf.PingPong(Time.time * 10f, 2f) + 3f;
        }
    }
}