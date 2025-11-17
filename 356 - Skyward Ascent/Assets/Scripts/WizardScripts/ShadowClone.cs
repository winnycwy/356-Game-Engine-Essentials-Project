using UnityEngine;
using Ilumisoft.HealthSystem;

public class ShadowClone : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public float contactDamage = 10f;   // how much damage clone deals to player

    private Transform player;
    private Health health;              // Ilumisoft health reference

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("ShadowClone requires a Health component!");
        }

        // If killed by weapon:
        health.OnHealthEmpty += OnDeath;

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
        // If it hits player, deal damage + die
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
                playerHealth.ApplyDamage(contactDamage);

            Destroy(gameObject);
        }
    }

    private void OnDeath()
    {
        // Clone is killed by weapon or other damage
        Destroy(gameObject);
    }
}