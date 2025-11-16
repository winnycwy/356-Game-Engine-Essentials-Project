using UnityEngine;
using System.Collections.Generic;
using Ilumisoft.HealthSystem; // Add this using directive

public class FireRainDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerHit = 20f;
    public float hitInterval = 0.5f; // Time between damage ticks
    public string[] enemyTags = { "Enemy", "Spriteling", "Bee" }; // Added "Bee" tag

    [Header("Visual Feedback")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnParticleCollision(GameObject other)
    {
        // Check if collided object is an enemy
        bool isEnemy = false;
        foreach (string enemyTag in enemyTags)
        {
            if (other.CompareTag(enemyTag))
            {
                isEnemy = true;
                break;
            }
        }

        if (isEnemy)
        {
            ProcessEnemyHit(other);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Additional trigger-based detection for larger enemies
        bool isEnemy = false;
        foreach (string enemyTag in enemyTags)
        {
            if (other.CompareTag(enemyTag))
            {
                isEnemy = true;
                break;
            }
        }

        if (isEnemy)
        {
            ProcessEnemyHit(other.gameObject);
        }
    }

    private void ProcessEnemyHit(GameObject enemy)
    {
        // Check if enough time has passed since last hit
        float currentTime = Time.time;
        if (lastHitTime.ContainsKey(enemy))
        {
            if (currentTime - lastHitTime[enemy] < hitInterval)
                return;

            lastHitTime[enemy] = currentTime;
        }
        else
        {
            lastHitTime.Add(enemy, currentTime);
        }

        // Apply damage to enemy using Ilumisoft Health System
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null && enemyHealth.IsAlive)
        {
            enemyHealth.ApplyDamage(damagePerHit);

            // Show hit effect
            if (hitEffect != null)
            {
                ParticleSystem effect = Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
                Destroy(effect.gameObject, 2f);
            }

            // Play hit sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            Debug.Log($"Fire rain hit {enemy.name} for {damagePerHit} damage!");

            // Special handling for Bee
            BeeScript bee = enemy.GetComponent<BeeScript>();
            if (bee != null)
            {
                // Call the existing TakeDamage method for additional bee-specific logic
                bee.TakeDamage(damagePerHit);
            }
        }

        // Note: SpritelingAI will be implemented later
        // SpritelingAI spriteling = enemy.GetComponent<SpritelingAI>();
        // if (spriteling != null)
        // {
        //     spriteling.OnFireHit(); // Optional: Add fire-specific reaction
        // }
    }

    void OnDestroy()
    {
        // Clean up
        lastHitTime.Clear();
    }

    public class ParticleCollisionDebugger : MonoBehaviour
    {
        void OnParticleCollision(GameObject other)
        {
            Debug.Log($"🔥 Particle collided with: {other.name} (Tag: {other.tag})");

            // Visual feedback - change color temporarily
            Renderer renderer = other.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.red;
                Invoke(nameof(ResetColor), 0.5f);
            }
        }

        void ResetColor()
        {
            // This would need to store original colors in a real implementation
        }
    }

}
