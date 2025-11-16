using UnityEngine;
using System.Collections.Generic;
using Ilumisoft.HealthSystem;

public class FireRainAreaDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 40f; // 20 damage every 0.5 seconds
    public float damageInterval = 0.5f;
    public float damageRadius = 4f;

    [Header("Visual Feedback")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();
    private AudioSource audioSource;
    private SphereCollider damageCollider;
    private List<GameObject> enemiesInRange = new List<GameObject>();

    void Start()
    {
        Debug.Log("🔥 FireRainAreaDamage Started!");

        // Create damage collider
        damageCollider = gameObject.AddComponent<SphereCollider>();
        damageCollider.isTrigger = true;
        damageCollider.radius = damageRadius;

        // Add rigidbody for trigger events (required for OnTriggerStay)
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Debug.Log($"🔥 Damage area created with radius: {damageRadius}");
    }

    void Update()
    {
        // Damage all enemies in range at intervals
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            GameObject enemy = enemiesInRange[i];
            if (enemy == null)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }

            ProcessDamage(enemy);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsEnemy(other.gameObject))
        {
            Debug.Log($"🔥 Enemy entered damage area: {other.name}");
            if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsEnemy(other.gameObject))
        {
            Debug.Log($"🔥 Enemy left damage area: {other.name}");
            enemiesInRange.Remove(other.gameObject);
            lastDamageTime.Remove(other.gameObject);
        }
    }

    private bool IsEnemy(GameObject obj)
    {
        return obj.CompareTag("enemy");
    }

    private void ProcessDamage(GameObject enemy)
    {
        float currentTime = Time.time;

        // Check damage interval
        if (lastDamageTime.ContainsKey(enemy))
        {
            if (currentTime - lastDamageTime[enemy] < damageInterval)
                return;

            lastDamageTime[enemy] = currentTime;
        }
        else
        {
            lastDamageTime.Add(enemy, currentTime);
        }

        // Apply damage
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            if (enemyHealth.IsAlive)
            {
                float damage = damagePerSecond * damageInterval;

                Debug.Log($"💥 Applying {damage} damage to {enemy.name}");
                Debug.Log($"📊 Before - Health: {enemyHealth.CurrentHealth}/{enemyHealth.MaxHealth}");

                enemyHealth.ApplyDamage(damage);

                Debug.Log($"📊 After - Health: {enemyHealth.CurrentHealth}/{enemyHealth.MaxHealth}");
                Debug.Log($"❤️ IsAlive: {enemyHealth.IsAlive}");

                // Visual/Audio feedback
                ShowHitEffect(enemy.transform.position);
                PlayHitSound();
            }
            else
            {
                Debug.Log("💀 Enemy is already dead, removing from list");
                enemiesInRange.Remove(enemy);
                lastDamageTime.Remove(enemy);
            }
        }
        else
        {
            Debug.LogError($"❌ No Health component on {enemy.name}!");

            // Try BeeScript as fallback
            BeeScript bee = enemy.GetComponent<BeeScript>();
            if (bee != null)
            {
                Debug.Log("🐝 Using BeeScript.TakeDamage()");
                bee.TakeDamage(damagePerSecond * damageInterval);
            }
        }
    }

    private void ShowHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }
    }

    private void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    void OnDestroy()
    {
        enemiesInRange.Clear();
        lastDamageTime.Clear();
        Debug.Log("🔥 FireRainAreaDamage destroyed");
    }

    void OnDrawGizmosSelected()
    {
        // Visualize damage radius in Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}