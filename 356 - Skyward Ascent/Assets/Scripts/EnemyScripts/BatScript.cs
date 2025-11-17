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

    [Header("Flight Settings")]
    public float heightAbovePlayer = 0.5f; // Directly above player's head

    [Header("Movement Settings")]
    public float patrolSpeed = 4f;
    public float chaseSpeed = 6f;
    public float acceleration = 12f;

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

        // Configure NavMeshAgent
        agent.speed = patrolSpeed;
        agent.acceleration = acceleration;

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

        GoToNextWaypoint();
    }

    private void SetupFlappingSound()
    {
        flappingAudioSource = gameObject.AddComponent<AudioSource>();
        flappingAudioSource.spatialBlend = 1f;
        flappingAudioSource.volume = flappingVolume;
        flappingAudioSource.maxDistance = flappingMaxDistance;
        flappingAudioSource.minDistance = flappingMinDistance;
        flappingAudioSource.loop = true;
        flappingAudioSource.rolloffMode = AudioRolloffMode.Linear;

        if (flappingSound != null)
        {
            flappingAudioSource.clip = flappingSound;
            flappingAudioSource.Play();
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateMovementSpeed();
        UpdateAnimations();
        UpdateFlappingSound();
        MaintainFlightHeight(); // This now directly matches player height

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distanceToPlayer < chaseRange)
                    ChangeState(State.Chase);
                break;

            case State.Chase:
                Chase();

                if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    StartAttack();
                }

                if (distanceToPlayer > loseRange)
                    ChangeState(State.Patrol);
                break;
        }
    }

    private void MaintainFlightHeight()
    {
        if (isDead || player == null || agent == null) return;

        // Let NavMeshAgent handle movement, only adjust destination height
        Vector3 currentDestination = agent.destination;
        float targetHeight = player.position.y + heightAbovePlayer;

        // Only update if height difference is significant
        if (Mathf.Abs(currentDestination.y - targetHeight) > 0.1f)
        {
            agent.destination = new Vector3(currentDestination.x, targetHeight, currentDestination.z);
        }
    }

    private void UpdateMovementSpeed()
    {
        if (isDead) return;

        float targetSpeed = currentState == State.Chase ? chaseSpeed : patrolSpeed;
        agent.speed = Mathf.Lerp(agent.speed, targetSpeed, Time.deltaTime * 2f);
    }

    private void UpdateFlappingSound()
    {
        if (flappingAudioSource == null || isDead) return;

        float speed = agent.velocity.magnitude / agent.speed;
        flappingAudioSource.pitch = Mathf.Lerp(0.7f, 1.5f, speed);

        float targetVolume = currentState == State.Chase ? flappingVolume * 1.5f : flappingVolume;
        flappingAudioSource.volume = Mathf.Lerp(flappingAudioSource.volume, targetVolume, Time.deltaTime * 2f);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude / agent.speed;

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

        if (animator != null && HasParameter(animator, attackParam))
        {
            animator.SetTrigger(attackParam);
        }

        lastAttackTime = Time.time;
        ApplyAttackDamage();
    }

    private void ApplyAttackDamage()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.ApplyDamage(attackDamage);
            }
        }
    }

    public void OnAttackEnd()
    {
        if (animator != null && HasParameter(animator, attackParam))
        {
            animator.ResetTrigger(attackParam);
        }
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

        if (flappingAudioSource != null)
        {
            flappingAudioSource.Stop();
        }

        if (animator != null && HasParameter(animator, isDeadParam))
        {
            animator.SetBool(isDeadParam, true);
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

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

        Vector3 waypointPos = waypoints[waypointIndex].position;

        // Use player's current height for waypoints too
        float waypointHeight = player != null ? player.position.y + heightAbovePlayer : waypointPos.y;
        Vector3 adjustedWaypoint = new Vector3(waypointPos.x, waypointHeight, waypointPos.z);

        agent.destination = adjustedWaypoint;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void Chase()
    {
        if (isDead || player == null) return;

        Vector3 playerPosition = player.position;

        // Chase at player's current height
        Vector3 chasePosition = new Vector3(playerPosition.x, playerPosition.y + heightAbovePlayer, playerPosition.z);

        agent.destination = chasePosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw height relationship
        if (player != null)
        {
            Gizmos.color = Color.green;
            Vector3 batHeightPos = new Vector3(transform.position.x, player.position.y + heightAbovePlayer, transform.position.z);
            Gizmos.DrawWireSphere(batHeightPos, 0.3f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, 0.3f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(player.position, batHeightPos);
        }

        if (flappingAudioSource != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, flappingMaxDistance);
        }
    }
}