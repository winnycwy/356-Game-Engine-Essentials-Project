using UnityEngine;
using Ilumisoft.HealthSystem;

public class PlayerDeathHandler : MonoBehaviour
{
    private Health playerHealth;
    private Animator animator;  // Optional: for death animation
    private CharacterController controller; // Optional: to disable movement

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (playerHealth != null)
        {
            playerHealth.OnHealthEmpty += HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player Died!");

        // 1️⃣ Disable movement
        if (controller != null)
        {
            controller.enabled = false;
        }

        // 2️⃣ Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Make sure you have a "Die" trigger in Animator
        }

        // 3️⃣ Optional: Destroy or respawn
        // Destroy(gameObject, 3f);
        // OR SceneManager.LoadScene("YourSceneName"); // requires using UnityEngine.SceneManagement;
    }
}
