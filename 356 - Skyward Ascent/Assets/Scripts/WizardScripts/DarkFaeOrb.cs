using UnityEngine;
using Ilumisoft.HealthSystem;

public class DarkFaeOrb : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float turnSpeed = 4f;

    [Header("Health")]
    public Health health; // Ilumisoft Health component

    [Header("Combat")]
    public float contactDamage = 10f; // Damage to player on contact

    [Header("Targeting")]
    private Transform target;

    void Start()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (health != null)
            health.OnHealthEmpty += OnDeath;

        // Auto-destroy after 10s if not killed
        Destroy(gameObject, 10f);

        // Find player as target (optional)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            turnSpeed * Time.deltaTime
        );

        // Forward movement
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    /// <summary>
    /// Called externally by FaeLightAbility when orb is inside light range
    /// </summary>
    public void TakeFaeLightDamage(float amount = 1f)
    {
        if (health != null)
            health.ApplyDamage(amount);
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