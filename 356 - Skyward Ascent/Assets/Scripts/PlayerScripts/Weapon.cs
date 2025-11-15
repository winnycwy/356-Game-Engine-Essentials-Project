using UnityEngine;
using System.Collections;
using Ilumisoft.HealthSystem;


public class Weapon : MonoBehaviour
{
    public Collider weaponCollider;
    public int damage = 10;
    public float activeTime = 0.3f;

    private bool canDamage = false;

    private void Awake()
    {
        weaponCollider.enabled = false;
    }

    public void EnableDamage()
    {
        StartCoroutine(EnableColliderTemporarily());
    }

    private IEnumerator EnableColliderTemporarily()
    {
        canDamage = true;
        weaponCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        weaponCollider.enabled = false;
        canDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;

        Debug.Log("Weapon collided with: " + other.name);

        // Only damage enemies
        if (!other.CompareTag("enemy"))
        {
            Debug.Log("Hit object is NOT an enemy, ignoring.");
            return;
        }

        // Get the Ilumisoft Health component
        Health enemyHealth = other.GetComponent<Health>();

        if (enemyHealth != null)
        {
            Debug.Log("Applying " + damage + " damage to ENEMY: " + other.name);

            float before = enemyHealth.CurrentHealth;

            enemyHealth.ApplyDamage(damage);

            // Check if the health dropped to 0
            if (before > 0 && enemyHealth.CurrentHealth <= 0)
            {
                Debug.Log(other.name + " has died (health reached 0 from weapon hit)");
            }
        }
        else
        {
            Debug.Log("Enemy has no Health component (unexpected).");
        }
    }
}