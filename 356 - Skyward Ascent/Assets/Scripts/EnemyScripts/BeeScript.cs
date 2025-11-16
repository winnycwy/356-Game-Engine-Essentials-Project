using UnityEngine;
using UnityEngine.AI;
using Ilumisoft.HealthSystem;
using System.Collections;

public class BeeScript : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState = State.Patrol;

    [Header("AI Settings")]
    public Transform player;
    public Transform[] waypoints;

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

    private NavMeshAgent agent;
    private int waypointIndex = 0;
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
            audioSource.spatialBlend = 1f;
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
        if (isDead) return; // Only check isDead, not health.IsAlive

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
        if (changeAmount < 0 && !isDead)
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

        if (beeRenderer != null)
        {
            beeRenderer.material.color = hurtColor;
        }

        yield return new WaitForSeconds(hurtFlashDuration);

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
        Debug.Log("🐝 Bee died! Starting death sequence...");

        // ✅ IMMEDIATELY: Stop AI and disable components
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // ✅ DON'T disable renderer yet - let particles play on visible bee
        // beeRenderer.enabled = false;

        // ✅ PLAY DEATH EFFECTS FIRST
        TriggerDeathEffects();

        // ✅ THEN start destruction sequence
        StartCoroutine(DeathSequence());
    }

    private void TriggerDeathEffects()
    {
        Debug.Log("🐝 Playing death effects");

        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
            Debug.Log("🐝 Death sound played");
        }

        // Play death particles
        if (deathParticles != null)
        {
            deathParticles.Play();
            Debug.Log("🐝 Death particles played");

            // Optional: Make particles independent so they continue after bee is destroyed
            deathParticles.transform.SetParent(null); // Detach from bee
        }
        else
        {
            Debug.LogWarning("🐝 No death particles assigned!");
        }

        // Spawn death effect prefab
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Debug.Log("🐝 Death effect prefab instantiated");
        }
    }

    // ✅ NEW: Separate death sequence coroutine
    private IEnumerator DeathSequence()
    {
        Debug.Log("🐝 Death sequence started");

        // Wait a moment for effects to be visible
        yield return new WaitForSeconds(0.5f);

        // Now hide the bee but keep effects playing
        if (beeRenderer != null)
        {
            beeRenderer.enabled = false;
            Debug.Log("🐝 Bee renderer disabled");
        }

        // Wait for remaining effects
        yield return new WaitForSeconds(deathDestroyDelay - 0.5f);

        // Finally destroy the bee
        Debug.Log("🐝 Destroying bee GameObject");
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        Debug.Log($"🐝 Bee taking {damage} damage!");

        if (health != null && health.IsAlive)
        {
            health.ApplyDamage(damage);
        }
    }

    void ChangeState(State newState)
    {
        if (isDead) return;
        currentState = newState;

        if (newState == State.Patrol)
            GoToNextWaypoint();
    }

    void Patrol()
    {
        if (isDead || waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (isDead || waypoints.Length == 0) return;
        agent.destination = waypoints[waypointIndex].position;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void Chase()
    {
        if (isDead || player == null) return;
        agent.SetDestination(player.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        TryDamagePlayer(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (isDead) return;
        TryDamagePlayer(other);
    }

    void TryDamagePlayer(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(attackDamage);
                lastAttackTime = Time.time;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}