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
    public float attackRange = 2f;

    [Header("Animation Settings")]
    public Animator animator;
    public float animationTransitionTime = 0.1f;

    [Header("Sound Effects")]
    public AudioClip hurtSound;
    public AudioClip buzzingSound; // New buzzing sound
    public float buzzingVolume = 0.3f;
    public float buzzingMaxDistance = 15f;
    public float buzzingMinDistance = 3f;

    [Header("Hurt Effects")]
    public float hurtFlashDuration = 0.2f;
    public Color hurtColor = Color.white;

    [Header("Death Effects")]
    public float deathDestroyDelay = 2f;

    private NavMeshAgent agent;
    private int waypointIndex = 0;
    private float lastAttackTime = 0f;
    private Health health;
    private AudioSource audioSource;
    private AudioSource buzzingAudioSource; // Separate audio source for buzzing
    private Renderer beeRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    private bool isDead = false;
    private bool isAttacking = false;

    // Use string names that match your Animator parameters
    private string moveSpeedParam = "MoveSpeed";
    private string attackParam = "Attack";
    private string damageParam = "Damage";
    private string isDeadParam = "isDead";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        beeRenderer = GetComponentInChildren<Renderer>();
        animator = GetComponentInChildren<Animator>();

        // Setup buzzing audio source
        SetupBuzzingSound();

        // Get or add main AudioSource if missing
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

    private void SetupBuzzingSound()
    {
        // Create a separate AudioSource for buzzing sound
        buzzingAudioSource = gameObject.AddComponent<AudioSource>();
        buzzingAudioSource.spatialBlend = 1f; // 3D sound
        buzzingAudioSource.volume = buzzingVolume;
        buzzingAudioSource.maxDistance = buzzingMaxDistance;
        buzzingAudioSource.minDistance = buzzingMinDistance;
        buzzingAudioSource.loop = true; // Loop the buzzing sound
        buzzingAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Assign and play buzzing sound
        if (buzzingSound != null)
        {
            buzzingAudioSource.clip = buzzingSound;
            buzzingAudioSource.Play();
            Debug.Log("🐝 Buzzing sound started");
        }
        else
        {
            Debug.LogWarning("🐝 Buzzing sound clip is not assigned!");
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateAnimations();
        UpdateBuzzingSound(); // Update buzzing sound based on movement

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distanceToPlayer < chaseRange)
                    ChangeState(State.Chase);
                break;

            case State.Chase:
                Chase(); // Always chase, attack doesn't stop movement

                // Check for attack range
                if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    StartAttack();
                }

                if (distanceToPlayer > loseRange)
                    ChangeState(State.Patrol);
                break;
        }
    }

    private void UpdateBuzzingSound()
    {
        if (buzzingAudioSource == null || isDead) return;

        // Adjust buzzing pitch based on movement speed for more dynamic sound
        float speed = agent.velocity.magnitude / agent.speed;
        buzzingAudioSource.pitch = Mathf.Lerp(0.8f, 1.2f, speed);

        // Adjust volume based on state (louder when chasing)
        float targetVolume = currentState == State.Chase ? buzzingVolume * 1.2f : buzzingVolume;
        buzzingAudioSource.volume = Mathf.Lerp(buzzingAudioSource.volume, targetVolume, Time.deltaTime * 2f);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Always update movement speed - bee keeps moving even during attack
        float speed = agent.velocity.magnitude / agent.speed;

        if (HasParameter(animator, moveSpeedParam))
        {
            animator.SetFloat(moveSpeedParam, speed);
        }
    }

    // Helper method to check if parameter exists
    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private void StartAttack()
    {
        if (isDead) return;

        isAttacking = true;

        // Trigger attack animation
        if (animator != null && HasParameter(animator, attackParam))
        {
            animator.SetTrigger(attackParam);
        }

        lastAttackTime = Time.time;

        // Apply damage immediately (no need to wait for animation)
        ApplyAttackDamage();

        Debug.Log("🐝 Starting attack while moving!");
    }

    private void ApplyAttackDamage()
    {
        if (isDead) return;

        // Check if player is in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.ApplyDamage(attackDamage);
                Debug.Log($"🐝 Bee attacked player for {attackDamage} damage! Player health: {playerHealth.CurrentHealth}");
            }
        }
    }

    // Animation Event: Called at the end of attack animation
    public void OnAttackEnd()
    {
        isAttacking = false;

        // Reset attack trigger
        if (animator != null && HasParameter(animator, attackParam))
        {
            animator.ResetTrigger(attackParam);
        }

        Debug.Log("🐝 Attack animation ended");
    }

    private void OnHealthChanged(float changeAmount)
    {
        // Trigger hurt effects when taking damage
        if (changeAmount < 0 && !isDead)
        {
            TriggerHurtEffects();

            // Trigger damage animation
            if (animator != null && HasParameter(animator, damageParam))
            {
                animator.SetTrigger(damageParam);
            }
        }
    }

    private void TriggerHurtEffects()
    {
        // Play hurt sound
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
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

        // Stop buzzing sound when bee dies
        if (buzzingAudioSource != null)
        {
            buzzingAudioSource.Stop();
            Debug.Log("🐝 Buzzing sound stopped");
        }

        // Trigger death animation
        if (animator != null && HasParameter(animator, isDeadParam))
        {
            animator.SetBool(isDeadParam, true);
        }

        // Stop AI and disable components
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // Start destruction sequence
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log("🐝 Death sequence started");

        // Wait for death animation to play
        yield return new WaitForSeconds(1.5f);

        // Now hide the bee
        if (beeRenderer != null)
        {
            beeRenderer.enabled = false;
        }

        // Wait for remaining effects
        yield return new WaitForSeconds(deathDestroyDelay - 1.5f);

        // Finally destroy the bee
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

    void OnDrawGizmosSelected()
    {
        // Chase range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Lose range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        // Attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Buzzing sound range
        if (buzzingAudioSource != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, buzzingMaxDistance);
        }
    }
}