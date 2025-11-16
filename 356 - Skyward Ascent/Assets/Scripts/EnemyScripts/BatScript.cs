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
    public float flightHeight = 3f;
    public float groundClearance = 0.5f;
    public float obstacleClearance = 1f;
    public float obstacleCheckDistance = 2f;

    [Header("Movement Settings")]
    public float patrolSpeed = 4f; // Increased from default
    public float chaseSpeed = 6f;  // Increased from default
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

        // Configure NavMeshAgent with higher speeds
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

        // Ensure the bat starts at a safe height
        EnsureSafeHeight();

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

        UpdateMovementSpeed(); // Update speed based on state
        UpdateAnimations();
        UpdateFlappingSound();
        MaintainFlightHeight();

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

    private void UpdateMovementSpeed()
    {
        if (isDead) return;

        // Change speed based on state
        float targetSpeed = currentState == State.Chase ? chaseSpeed : patrolSpeed;

        // Smoothly change speed
        agent.speed = Mathf.Lerp(agent.speed, targetSpeed, Time.deltaTime * 2f);
    }

    private void MaintainFlightHeight()
    {
        if (isDead) return;

        // Get current position
        Vector3 currentPosition = transform.position;

        // Calculate ground height
        float groundHeight = GetGroundHeight(currentPosition);

        // Calculate obstacle height (jumping stones, etc.)
        float obstacleHeight = GetObstacleHeight(currentPosition);

        // Calculate minimum safe height (highest of ground + clearance or obstacle + clearance)
        float minSafeHeight = Mathf.Max(
            groundHeight + groundClearance,
            obstacleHeight + obstacleClearance
        );

        // Ensure we're flying at least at flightHeight OR above obstacles
        float targetHeight = Mathf.Max(flightHeight, minSafeHeight);

        // Smoothly adjust height if needed
        if (Mathf.Abs(currentPosition.y - targetHeight) > 0.1f)
        {
            float newY = Mathf.Lerp(currentPosition.y, targetHeight, Time.deltaTime * 3f);
            transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        }
    }

    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        // Shoot a ray straight down to find the ground
        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y;
        }

        // If no ground detected, assume flat ground at y=0
        return 0f;
    }

    private float GetObstacleHeight(Vector3 position)
    {
        float highestObstacle = 0f;

        // Check for obstacles in multiple directions
        Vector3[] checkDirections = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.forward + Vector3.right,
            Vector3.forward + Vector3.left,
            Vector3.back + Vector3.right,
            Vector3.back + Vector3.left
        };

        foreach (Vector3 direction in checkDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(position, direction, out hit, obstacleCheckDistance))
            {
                // Only consider obstacles that are higher than current position
                if (hit.collider.gameObject != gameObject && hit.point.y > highestObstacle)
                {
                    highestObstacle = hit.point.y;
                }
            }
        }

        return highestObstacle;
    }

    private void EnsureSafeHeight()
    {
        Vector3 currentPosition = transform.position;
        float groundHeight = GetGroundHeight(currentPosition);
        float obstacleHeight = GetObstacleHeight(currentPosition);
        float minSafeHeight = Mathf.Max(
            groundHeight + groundClearance,
            obstacleHeight + obstacleClearance
        );

        // If current position is below safe height, move up
        if (currentPosition.y < minSafeHeight)
        {
            float targetHeight = Mathf.Max(flightHeight, minSafeHeight);
            transform.position = new Vector3(currentPosition.x, targetHeight, currentPosition.z);
        }
    }

    private void UpdateFlappingSound()
    {
        if (flappingAudioSource == null || isDead) return;

        float speed = agent.velocity.magnitude / agent.speed;
        flappingAudioSource.pitch = Mathf.Lerp(0.7f, 1.5f, speed); // Increased pitch range for faster movement

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

        // Ensure waypoint is at safe height
        float groundHeight = GetGroundHeight(waypointPos);
        float obstacleHeight = GetObstacleHeight(waypointPos);
        float minSafeHeight = Mathf.Max(
            groundHeight + groundClearance,
            obstacleHeight + obstacleClearance
        );
        float targetHeight = Mathf.Max(flightHeight, minSafeHeight);

        Vector3 adjustedWaypoint = new Vector3(waypointPos.x, targetHeight, waypointPos.z);

        agent.destination = adjustedWaypoint;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void Chase()
    {
        if (isDead || player == null) return;

        Vector3 playerPosition = player.position;

        // Ensure chase position is at safe height
        float groundHeight = GetGroundHeight(playerPosition);
        float obstacleHeight = GetObstacleHeight(playerPosition);
        float minSafeHeight = Mathf.Max(
            groundHeight + groundClearance,
            obstacleHeight + obstacleClearance
        );
        float targetHeight = Mathf.Max(flightHeight, minSafeHeight);

        Vector3 chasePosition = new Vector3(playerPosition.x, targetHeight, playerPosition.z);

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

        // Draw ground detection ray
        Vector3 currentPos = transform.position;
        float groundHeight = GetGroundHeight(currentPos);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(currentPos, new Vector3(currentPos.x, groundHeight, currentPos.z));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(currentPos.x, groundHeight, currentPos.z), 0.2f);

        // Draw obstacle detection rays
        Gizmos.color = Color.blue;
        Vector3[] checkDirections = {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            Vector3.forward + Vector3.right, Vector3.forward + Vector3.left,
            Vector3.back + Vector3.right, Vector3.back + Vector3.left
        };
        foreach (Vector3 direction in checkDirections)
        {
            Gizmos.DrawRay(currentPos, direction * obstacleCheckDistance);
        }

        if (flappingAudioSource != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, flappingMaxDistance);
        }
    }
}