using UnityEngine;
using Ilumisoft.HealthSystem;

public class HealthTest : MonoBehaviour
{
    public Health playerHealth;    // Assign your player's Health component

    public float damageAmount = 10f;
    public float healAmount = 10f;

    private void Update()
    {
        if (playerHealth == null) return;

        // Press 'H' to take damage
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerHealth.ApplyDamage(damageAmount);
        }

        // Press 'J' to heal
        if (Input.GetKeyDown(KeyCode.J))
        {
            playerHealth.AddHealth(healAmount);
        }
    }
}
