using UnityEngine;
using Ilumisoft.HealthSystem;

public class ShadowClone : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public float contactDamage = 10f;

    private Transform player;
    private Health health;
    private Animator animator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        if (health != null)
        {
            health.OnHealthEmpty += OnDeath;
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // If it hits player, deal damage + disappear
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.ApplyDamage(contactDamage);

            // Just destroy, no animation needed for clones
            Destroy(gameObject);
        }
    }

    private void OnDeath()
    {
        // Clone dies to player attacks - just destroy
        Destroy(gameObject);
    }
}