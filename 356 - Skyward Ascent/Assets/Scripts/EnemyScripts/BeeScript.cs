using UnityEngine;
using UnityEngine.AI;
using Ilumisoft.HealthSystem;

public class BeeScript : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState = State.Patrol;

    public Transform player;
    public Transform[] waypoints;
    private int waypointIndex = 0;
    private NavMeshAgent agent;

    public float chaseRange = 8f;
    public float loseRange = 12f;

    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    private BeeHealthController healthController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        healthController = GetComponent<BeeHealthController>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (healthController != null)
        {
            healthController.OnBeeDeath += OnDeath;
        }

        GoToNextWaypoint();
    }

    void Update()
    {
        if (healthController != null && !healthController.IsAlive())
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

    public void TakeDamage(float damage)
    {
        Debug.Log($"🐝 Bee taking {damage} damage!");

        if (healthController != null && healthController.IsAlive())
        {
            healthController.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("BeeHealthController not found or bee already dead!");
        }
    }

    void OnDeath()
    {
        Debug.Log("Bee died!");
        agent.isStopped = true;
        // Don't destroy here - BeeHealthController handles destruction
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