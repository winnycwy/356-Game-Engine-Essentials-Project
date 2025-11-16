using UnityEngine;
using UnityEngine.AI;
using Ilumisoft.HealthSystem;
using System.Collections;


public class BeeScript : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;
    private int waypointIndex = 0;
    private NavMeshAgent agent;

    [Header("Combat Settings")]
    public float chaseRange = 8f;
    public float loseRange = 12f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    [Header("Hurt Effects")]
    public AudioClip hurtSound;
    public ParticleSystem hurtParticles;
    public float hurtFlashDuration = 0.2f;
    public Color hurtColor = Color.white;

    [Header("Death Effects")] 
    public AudioClip deathSound;
    public ParticleSystem deathParticles;
    public GameObject deathEffectPrefab;
    public float deathDestroyDelay = 2f;

    private float lastAttackTime = 0f;
    private Health health;
    private AudioSource audioSource;
    private Renderer beeRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        beeRenderer = GetComponentInChildren<Renderer>();

        // Get or add AudioSource if missing
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = 0.5f;
        }

        // Store original color for hurt flash
        if (beeRenderer != null)
        {
            originalColor = beeRenderer.material.color;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged;
            health.OnHealthEmpty += OnDeath;
        }

        GoToNextWaypoint();
    }

    void Update()
    {
        if (isDead || health != null && !health.IsAlive)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distance < chaseRange)
                    ChangeState(State.Chase);
                break;

            case State.Chase:
                Chase();
                if (distance > loseRange)
                    ChangeState(State.Patrol);
                break;
        }
    }

    private void OnHealthChanged(float changeAmount)
    {
        // Trigger hurt effects when taking damage
        if (changeAmount < 0) // Negative change = damage taken
        {
            TriggerHurtEffects();
        }
    }

    private void TriggerHurtEffects()
    {
        // Play hurt sound
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // Play hurt particles
        if (hurtParticles != null)
        {
            hurtParticles.Play();
        }

        // Visual hurt flash
        if (beeRenderer != null && !isFlashing)
        {
            StartCoroutine(HurtFlash());
        }
    }

    private IEnumerator HurtFlash()
    {
        isFlashing = true;

        // Flash to hurt color
        if (beeRenderer != null)
        {
            beeRenderer.material.color = hurtColor;
        }

        // Wait for flash duration
        yield return new WaitForSeconds(hurtFlashDuration);

        // Return to original color
        if (beeRenderer != null)
        {
            beeRenderer.material.color = originalColor;
        }

        isFlashing = false;
    }

    void OnDeath()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("🐝 Bee died! Playing death effects...");

        // Stop movement immediately
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable collider to prevent further interactions
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // Disable renderer to make bee invisible
        if (beeRenderer != null)
            beeRenderer.enabled = false;

        // ✅ PLAY DEATH EFFECTS
        TriggerDeathEffects();

        // Destroy after delay (let effects play out)
        Destroy(gameObject, deathDestroyDelay);
    }

    // ✅ ADD THIS: Death effects method
    private void TriggerDeathEffects()
    {
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Play death particles
        if (deathParticles != null)
        {
            deathParticles.Play();
        }

        // Spawn death effect prefab (explosion, etc.)
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Optional: Shake or fall animation
        StartCoroutine(DeathAnimation());
    }

    // ✅ ADD THIS: Death animation coroutine
    private IEnumerator DeathAnimation()
    {
        // Make bee fall down or shake
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < deathDestroyDelay)
        {
            // Sink into ground
            transform.position = startPosition - Vector3.up * (elapsed / deathDestroyDelay);

            // Optional: Add slight shake
            transform.position += Random.insideUnitSphere * 0.1f;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Bee taking {damage} damage!");

        if (health != null && health.IsAlive)
        {
            health.ApplyDamage(damage);
        }
        else
        {
            Debug.LogError("BeeHealthController not found or bee already dead!");
        }
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        if (newState == State.Patrol)
            GoToNextWaypoint();
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.destination = waypoints[waypointIndex].position;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void Chase()
    {
        if (player != null)
            agent.SetDestination(player.position);
    }

    void OnTriggerEnter(Collider other)
    {
        TryDamagePlayer(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other);
    }

    void TryDamagePlayer(Collider other)
    {
        // Check if it's the player and we can attack again
        if (other.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            // Try getting the health component on the player
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(attackDamage);
                lastAttackTime = Time.time; // Reset attack timer
                                            // Debug.Log("Bee attacked player for " + attackDamage);
            }
        }
    }

    // Optional debug gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}