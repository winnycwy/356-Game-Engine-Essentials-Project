using Ilumisoft.HealthSystem;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Player Hurt Effects")]
    public AudioClip playerHurtSound; 
    public Image screenHurtOverlay;
    public float hurtFlashDuration = 0.3f;
    public float hurtMaxAlpha = 0.4f;

    [Header("Death Settings")]
    public GameObject deathScreen;
    public float deathScreenDelay = 2f;
    public AudioClip deathSound;
    public ParticleSystem deathParticles;

    [Header("Camera Effects")]
    public Camera mainCamera;
    public float deathCameraFov = 60f;

    [Header("Respawn Settings")]
    public Transform[] respawnPoints; // Assign your island start points here
    public int currentRespawnPoint = 0;
    public int deathIslandIndex = 0;

    private Health playerHealth;
    private Animator animator;
    private PlayerController playerController;
    private AudioSource audioSource;
    private float originalCameraFov;
    private bool isDead = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();

        // Save original position for respawn
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Get main camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            originalCameraFov = mainCamera.fieldOfView;

        // Hide death screen initially
        if (deathScreen != null)
            deathScreen.SetActive(false);

        if (screenHurtOverlay != null)
        {
            screenHurtOverlay.color = new Color(1, 0, 0, 0); // Start invisible
        }

        // Subscribe to health events - CORRECTED VERSION
        if (playerHealth != null)
        {
            // Use += and -= for C# events (not AddListener/RemoveListener)
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnHealthEmpty += HandleDeath;
        }
    }


    // Alternative method if Ilumisoft uses single parameter
    private void HandleHealthChanged(float currentHealth)
    {
        Debug.Log($"Health changed: {currentHealth}");



        if (currentHealth < playerHealth.MaxHealth)
            TriggerHurtEffect();
        {
            PlayerHurtEffect hurtEffect = GetComponent<PlayerHurtEffect>();
            if (hurtEffect != null)
            {
                hurtEffect.TriggerHurtEffect();
            }
        }

        // Trigger hurt animation if health decreased and not dead
        if (animator != null && playerHealth != null && currentHealth < playerHealth.MaxHealth && !isDead)
        {
            animator.SetTrigger("Hit");
            Debug.Log("Hurt animation triggered");

            // Optional: Add a cooldown so you don't spam hurt animation
            StartCoroutine(HurtAnimationCooldown());
        }
    }

    private void TriggerHurtEffect()
    {
        // Play hurt sound
        if (playerHurtSound != null && audioSource != null && !isDead)
        {
            audioSource.PlayOneShot(playerHurtSound);
        }

        // Trigger UI hurt effect
        if (screenHurtOverlay != null && !isDead)
        {
            StartCoroutine(UIHurtFlash());
        }

        // Trigger material flash if you have it
        PlayerHurtEffect hurtEffect = GetComponent<PlayerHurtEffect>();
        if (hurtEffect != null && !isDead)
        {
            hurtEffect.TriggerHurtEffect();
        }
    }

    private IEnumerator UIHurtFlash()
    {
        // Fade in quickly
        float elapsed = 0f;
        while (elapsed < hurtFlashDuration / 2)
        {
            float alpha = Mathf.Lerp(0, hurtMaxAlpha, elapsed / (hurtFlashDuration / 2));
            screenHurtOverlay.color = new Color(1, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out slowly
        elapsed = 0f;
        while (elapsed < hurtFlashDuration / 2)
        {
            float alpha = Mathf.Lerp(hurtMaxAlpha, 0, elapsed / (hurtFlashDuration / 2));
            screenHurtOverlay.color = new Color(1, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure completely invisible
        screenHurtOverlay.color = new Color(1, 0, 0, 0);
    }

    private IEnumerator HurtAnimationCooldown()
    {
        // Prevent hurt animation from spamming
        yield return new WaitForSeconds(0.5f);
    }

    // Keep this method for the respawn zones to call
    public void SetCurrentIsland(int islandIndex)
    {
        if (!isDead && islandIndex >= 0 && islandIndex < respawnPoints.Length)
        {
            deathIslandIndex = islandIndex;
            Debug.Log($"Player now on Island {deathIslandIndex + 1}");
        }
    }

    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"Player died on Island {deathIslandIndex + 1}, will respawn there");

        StopAllCoroutines();
        if (screenHurtOverlay != null)
        {
            screenHurtOverlay.color = new Color(1, 0, 0, 0); // Ensure no red overlay
        }

        // 1️⃣ DISABLE PHYSICS FIRST - This is crucial!
        DisablePhysics();

        // COMPLETELY DISABLE ALL MOVEMENT
        DisableAllMovement();

        // Fallback: Force show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2️⃣ Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            // Apply root motion so animation controls position
            animator.applyRootMotion = true;
        }

        // 3️⃣ Play death particles
        if (deathParticles != null)
        {
            deathParticles.Play();
        }

        // 4️⃣ Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 5️⃣ Camera effect
        if (mainCamera != null)
        {
            StartCoroutine(DeathCameraEffect());
        }

        // 6️⃣ Show death screen after delay
        Invoke(nameof(ShowDeathScreen), deathScreenDelay);
    }

    private void DisablePhysics()
    {
        Debug.Log("Disabling physics components...");

        // Disable Character Controller (if using)
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("CharacterController disabled");
        }

        // Disable or freeze Rigidbody (if using)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // This stops physics interactions
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Rigidbody set to kinematic");
        }

        // Disable Capsule Collider (if using)
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
            Debug.Log("CapsuleCollider disabled");
        }

        // Disable any other colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        Debug.Log($"Disabled {colliders.Length} colliders");
    }

    private IEnumerator DeathCameraEffect()
    {
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = Mathf.Lerp(originalCameraFov, deathCameraFov, elapsed / duration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ShowDeathScreen()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);

            // Add fade-in effect if needed
            CanvasGroup canvasGroup = deathScreen.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInDeathScreen(canvasGroup));
            }
        }
    }

    private IEnumerator FadeInDeathScreen(CanvasGroup canvasGroup)
    {
        float duration = 1f;
        float elapsed = 0f;

        canvasGroup.alpha = 0f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }


    private void DisableAllMovement()
    {
        // Method 1: Use your PlayerController
        if (playerController != null)
        {
            playerController.DisableMovement();
        }

        // Method 2: Disable components directly
        ThirdPersonController tpc = GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = false;

        StarterAssetsInputs inputs = GetComponent<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.move = Vector2.zero;
            inputs.sprint = false;
            inputs.jump = false;
            inputs.cursorInputForLook = false;
        }

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        // Method 3: Disable Rigidbody/CharacterController if exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Force cursor unlock
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("All movement systems disabled");
    }

    public void Respawn()
    {
        if (!isDead) return;

        if (screenHurtOverlay != null)
        {
            screenHurtOverlay.color = new Color(1, 0, 0, 0);
        }

        Debug.Log("=== STARTING RESPAWN PROCESS ===");
        Debug.Log($"Respawning at Island {deathIslandIndex + 1}"); // Added island info


        // 1️⃣ Reset healing items FIRST
        if (HealingItemManager.Instance != null)
        {
            HealingItemManager.Instance.ResetAllHealingItems();
            Debug.Log("Healing items reset requested");
        }
        else
        {
            Debug.LogError("HealingItemManager instance is null!");
        }

        // 1️⃣ Reset animator FIRST - before moving player
        if (animator != null)
        {
            Debug.Log("Resetting animator...");

            // Reset all triggers
            animator.ResetTrigger("Die");
            animator.ResetTrigger("Hit");

            // Reset all bools
            animator.SetBool("IsDead", false);

            // Stop root motion
            animator.applyRootMotion = false;

            // Force back to idle state
            animator.Play("Idle", 0, 0f); // Layer 0, time 0 (start of animation)

            // Re-enable animator if it was disabled
            animator.enabled = true;

            Debug.Log("Animator reset to Idle state");

            // In Respawn method, after animator reset:
            ForceAnimationReset();
        }
        else
        {
            Debug.LogError("Animator is null during respawn!");
        }

        // 2️⃣ Move player to respawn position
        // 2️⃣ Move player to respawn position - USE deathIslandIndex
        if (respawnPoints != null && respawnPoints.Length > deathIslandIndex)
        {
            transform.position = respawnPoints[deathIslandIndex].position;
            transform.rotation = respawnPoints[deathIslandIndex].rotation;
            Debug.Log($"Player moved to Island {deathIslandIndex + 1} respawn point");
        }
        else if (respawnPoints != null && respawnPoints.Length > 0)
        {
            // Fallback to first respawn point
            transform.position = respawnPoints[0].position;
            transform.rotation = respawnPoints[0].rotation;
            Debug.Log($"Player moved to fallback respawn point (Island 1)");
        }
        else
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            Debug.Log("Player moved to original position");
        }

        // 3️⃣ Re-enable physics
        EnablePhysics();
        Debug.Log("Physics re-enabled");

        // 4️⃣ Reset health
        if (playerHealth != null)
        {
            try
            {
                playerHealth.CurrentHealth = playerHealth.MaxHealth;
                Debug.Log($"Health reset: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Health reset failed: " + e.Message);
            }
        }

        // 5️⃣ Re-enable movement with extra safety
        StartCoroutine(EnableMovementDelayed());

        // 6️⃣ Hide death screen
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
            Debug.Log("Death screen hidden");
        }

        isDead = false;
        Debug.Log("=== RESPAWN COMPLETED ===");
    }

    private void ForceAnimationReset()
    {
        if (animator != null)
        {
            // Completely reset the animator
            animator.Rebind();
            animator.Update(0f);

            // Force specific state
            animator.Play("Idle");

            Debug.Log("Animator forcefully reset");
        }
    }

    private IEnumerator EnableMovementDelayed()
    {
        // Wait one frame to ensure everything is reset
        yield return null;

        Debug.Log("Enabling movement systems...");

        // Method 1: Use your PlayerController
        if (playerController != null)
        {
            playerController.EnableMovement();
            Debug.Log("PlayerController.EnableMovement() called");
        }
        else
        {
            Debug.LogError("PlayerController is null!");
        }

        // Method 2: Direct component enabling (safety net)
        ThirdPersonController tpc = GetComponent<ThirdPersonController>();
        if (tpc != null)
        {
            tpc.enabled = true;
            Debug.Log("ThirdPersonController enabled directly");
        }

        StarterAssetsInputs inputs = GetComponent<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.enabled = true;
            inputs.cursorInputForLook = true;
            inputs.move = Vector2.zero; // Reset input
            Debug.Log("StarterAssetsInputs enabled");
        }

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
            Debug.Log("PlayerInput enabled");
        }

        // Force cursor lock
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Cursor locked and hidden");

        Debug.Log("All movement systems enabled");
    }

    private void EnablePhysics()
    {
        Debug.Log("Re-enabling physics components...");

        // Wait one frame to ensure position is set
        StartCoroutine(EnablePhysicsCoroutine());
    }

    private IEnumerator EnablePhysicsCoroutine()
    {
        // Wait for end of frame to ensure position is properly set
        yield return new WaitForEndOfFrame();

        // Re-enable Character Controller
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
            Debug.Log("CharacterController re-enabled");
        }

        // Re-enable Rigidbody - CAREFUL with this!
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Reset velocity and position first
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Only set to non-kinematic if you need physics
            // For character controllers, often better to keep it kinematic
            rb.isKinematic = true; // Keep it kinematic to prevent falling
            Debug.Log("Rigidbody reset and kept kinematic");
        }

        // Re-enable Capsule Collider
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = true;
            Debug.Log("CapsuleCollider re-enabled");
        }

        // Re-enable all colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
        Debug.Log($"Re-enabled {colliders.Length} colliders");
    }

    private void EnableAllMovement()
    {
        // Re-enable all components
        ThirdPersonController tpc = GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = true;

        StarterAssetsInputs inputs = GetComponent<StarterAssetsInputs>();
        if (inputs != null) inputs.cursorInputForLook = true;

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        // Use your PlayerController
        if (playerController != null)
        {
            playerController.EnableMovement();
        }

        Debug.Log("All movement systems re-enabled");
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events - CORRECTED VERSION
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnHealthEmpty -= HandleDeath;
        }
    }
}