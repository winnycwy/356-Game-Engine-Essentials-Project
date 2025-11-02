using UnityEngine;

public class PlayerCinematicController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Animator playerAnimator;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<Animator>();

        // Register for cinematic events
        GameEventManager.Instance.OnTreeActivationStarted += StartCinematic;
        GameEventManager.Instance.OnTreeActivated += EndCinematic;
    }

    void StartCinematic()
    {
        // Disable player control during cinematic
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Celebrate"); // Make sure you have this animation
            playerAnimator.SetFloat("Speed", 0f);
        }
    }

    void EndCinematic()
    {
        // Re-enable player control
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    void OnDestroy()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnTreeActivationStarted -= StartCinematic;
            GameEventManager.Instance.OnTreeActivated -= EndCinematic;
        }
    }
}