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

        health = GetComponent<Health>();
        animator = GetComponent<Animator>();

        if (health == null)
            Debug.LogError("ShadowClone requires a Health component!");

        if (animator != null)
            animator.SetTrigger("Run");

        health.OnHealthEmpty += OnDeath;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        // --- ROTATE TOWARD PLAYER ---
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                10f * Time.deltaTime
            );
        }

        // --- MOVE FORWARD IN LOOK DIRECTION ---
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
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
        Destroy(gameObject);
    }
}