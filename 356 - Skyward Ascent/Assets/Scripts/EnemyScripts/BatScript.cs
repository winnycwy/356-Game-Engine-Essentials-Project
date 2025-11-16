using UnityEngine;
using UnityEngine.AI;
using Ilumisoft.HealthSystem;
using System.Collections;

public class BatScript : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState = State.Patrol;

    [Header("AI Settings")]
    public Transform player;
    public Transform[] waypoints;

    [Header("Combat Settings")]
    public float chaseRange = 10f;
    public float loseRange = 15f;
    public float attackDamage = 15f;
    public float attackCooldown = 2f;
    public float attackRange = 3f;

    [Header("Animation Settings")]
    public Animator animator;
    public float animationTransitionTime = 0.1f;

    [Header("Sound Effects")]
    public AudioClip hurtSound;
    public AudioClip flappingSound;
    public float flappingVolume = 0.4f;
    public float flappingMaxDistance = 20f;
    public float flappingMinDistance = 4f;

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
    private AudioSource flappingAudioSource;
    private Renderer batRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    private bool isDead = false;
    private bool isAttacking = false;

    // Animation parameter names
    private string moveSpeedParam = "MoveSpeed";
    private string attackParam = "Attack";
    private string damageParam = "Damage";
    private string isDeadParam = "isDead";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        batRenderer = GetComponentInChildren<Renderer>();
        animator = GetComponentInChildren<Animator>();

        // Debug NavMeshAgent
        if (agent != null)
        {
            Debug.Log($"🦇 NavMeshAgent: enabled={agent.enabled}, isOnNavMesh={agent.isOnNavMesh}");
        }
        else
        {
            Debug.LogError("🦇 NavMeshAgent component missing!");
        }

        // Setup flapping audio source
        SetupFlappingSound();

        // Get or add main AudioSource if missing
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.5f;
        }

        // Store original color for hurt flash
        if (batRenderer != null)
        {
            originalColor = batRenderer.material.color;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged;
            health.OnHealthEmpty += OnDeath;
        }

        // Debug waypoints
        if (waypoints != null && waypoints.Length > 0)
        {
            Debug.Log($"🦇 Waypoints assigned: {waypoints.Length}");
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                    Debug.Log($"🦇 Waypoint {i}: {waypoints[i].name} at {waypoints[i].position}");
                else
                    Debug.LogError($"🦇 Waypoint {i} is null!");
            }
        }
        else
        {
            Debug.LogError("🦇 No waypoints assigned!");
        }

        GoToNextWaypoint();
    }

    private void SetupFlappingSound()
    {
        // Create a separate AudioSource for flapping sound
        flappingAudioSource = gameObject.AddComponent<AudioSource>();
        flappingAudioSource.spatialBlend = 1f;
        flappingAudioSource.volume = flappingVolume;
        flappingAudioSource.maxDistance = flappingMaxDistance;
        flappingAudioSource.minDistance = flappingMinDistance;
        flappingAudioSource.loop = true;
        flappingAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // Assign and play flapping sound
        if (flappingSound != null)
        {
            flappingAudioSource.clip = flappingSound;
            flappingAudioSource.Play();
            Debug.Log("🦇 Flapping sound started");
        }
        else
        {
            Debug.LogWarning("🦇 Flapping sound clip is not assigned!");
        }
    }

    void Update()
    {
        if (isDead)
        {
            Debug.Log("🦇 Bat is dead, not updating");
            return;
        }

        UpdateAnimations();
        UpdateFlappingSound();

        if (player == null)
        {
            Debug.LogError("🦇 Player reference is null!");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        Debug.Log($"🦇 State: {currentState}, Distance to player: {distanceToPlayer}");

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distanceToPlayer < chaseRange)
                {
                    Debug.Log($"🦇 Switching to Chase (distance {distanceToPlayer} < {chaseRange})");
                    ChangeState(State.Chase);
                }
                break;

            case State.Chase:
                Chase();

                // Check for attack range
                if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    Debug.Log($"🦇 Starting attack (distance {distanceToPlayer} <= {attackRange})");
                    StartAttack();
                }

                if (distanceToPlayer > loseRange)
                {
                    Debug.Log($"🦇 Switching to Patrol (distance {distanceToPlayer} > {loseRange})");
                    ChangeState(State.Patrol);
                }
                break;
        }
    }

    private void UpdateFlappingSound()
    {
        if (flappingAudioSource == null || isDead) return;

        // Adjust flapping pitch based on movement speed
        float speed = agent.velocity.magnitude / agent.speed;
        flappingAudioSource.pitch = Mathf.Lerp(0.7f, 1.3f, speed);

        // Adjust volume based on state (louder when chasing)
        float targetVolume = currentState == State.Chase ? flappingVolume * 1.3f : flappingVolume;
        flappingAudioSource.volume = Mathf.Lerp(flappingAudioSource.volume, targetVolume, Time.deltaTime * 2f);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Update movement speed
        float speed = agent.velocity.magnitude / agent.speed;
        Debug.Log($"🦇 Movement speed: {speed}");

        if (HasParameter(animator, moveSpeedParam))
        {
            animator.SetFloat(moveSpeedParam, speed);
        }
    }

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
        ApplyAttackDamage();
        Debug.Log("🦇 Starting attack!");
    }

    private void ApplyAttackDamage()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        Debug.Log($"🦇 Attack check: distance={distanceToPlayer}, attackRange={attackRange}, inRange={distanceToPlayer <= attackRange}");

        if (distanceToPlayer <= attackRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log($"🦇 Player health found: {playerHealth.CurrentHealth}");
                if (playerHealth.IsAlive)
                {
                    playerHealth.ApplyDamage(attackDamage);
                    Debug.Log($"🦇 Bat attacked player for {attackDamage} damage! Player health now: {playerHealth.CurrentHealth}");
                }
                else
                {
                    Debug.Log("🦇 Player is already dead");
                }
            }
            else
            {
                Debug.LogError("🦇 Player Health component not found!");
            }
        }
        else
        {
            Debug.Log($"🦇 Player out of attack range: {distanceToPlayer} > {attackRange}");
        }
    }

    // Animation Event: Called at the end of attack animation
    public void OnAttackEnd()
    {
        isAttacking = false;

        if (animator != null && HasParameter(animator, attackParam))
        {
            animator.ResetTrigger(attackParam);
        }

        Debug.Log("🦇 Attack animation ended");
    }

    private void OnHealthChanged(float changeAmount)
    {
        if (changeAmount < 0 && !isDead)
        {
            TriggerHurtEffects();

            if (animator != null && HasParameter(animator, damageParam))
            {
                animator.SetTrigger(damageParam);
            }
        }
    }

    private void TriggerHurtEffects()
    {
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (batRenderer != null && !isFlashing)
        {
            StartCoroutine(HurtFlash());
        }
    }

    private IEnumerator HurtFlash()
    {
        isFlashing = true;

        if (batRenderer != null)
        {
            batRenderer.material.color = hurtColor;
        }

        yield return new WaitForSeconds(hurtFlashDuration);

        if (batRenderer != null)
        {
            batRenderer.material.color = originalColor;
        }

        isFlashing = false;
    }

    void OnDeath()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("🦇 Bat died!");

        // Stop flapping sound
        if (flappingAudioSource != null)
        {
            flappingAudioSource.Stop();
        }

        // Trigger death animation
        if (animator != null && HasParameter(animator, isDeadParam))
        {
            animator.SetBool(isDeadParam, true);
        }

        // Stop AI
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1.5f);

        if (batRenderer != null)
        {
            batRenderer.enabled = false;
        }

        yield return new WaitForSeconds(deathDestroyDelay - 1.5f);
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        Debug.Log($"🦇 Bat taking {damage} damage!");

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
        if (isDead || waypoints.Length == 0)
        {
            Debug.Log("🦇 No waypoints or dead");
            return;
        }

        // Add debug logs to see what's happening
        Debug.Log($"🦇 Patrol: pathPending={agent.pathPending}, remainingDistance={agent.remainingDistance}");

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Debug.Log("🦇 Going to next waypoint");
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (isDead || waypoints.Length == 0)
        {
            Debug.Log("🦇 Cannot go to waypoint - dead or no waypoints");
            return;
        }

        Debug.Log($"🦇 Moving to waypoint {waypointIndex} at position {waypoints[waypointIndex].position}");
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

        // Flapping sound range
        if (flappingAudioSource != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, flappingMaxDistance);
        }
    }
}