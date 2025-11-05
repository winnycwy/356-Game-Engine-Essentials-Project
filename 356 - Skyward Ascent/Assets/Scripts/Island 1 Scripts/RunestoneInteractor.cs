/*DRAFT 1
using UnityEngine;

public class RunestoneInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;           // Distance to interact
    public KeyCode interactKey = KeyCode.E;    // Key to activate runestone
    public LayerMask runestoneLayer;           // Layer for runstones

    private Runestone focusedRunestone = null;

    void Update()
    {
        CheckForRunestone();
        HandleRunestoneInteraction();
    }

    private void CheckForRunestone()
    {
        focusedRunestone = null;

        // Check all colliders in range on the runestone layer
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, runestoneLayer);
        foreach (Collider hit in hits)
        {
            Runestone rs = hit.GetComponent<Runestone>();
            if (rs != null && !rs.IsActivated())
            {
                focusedRunestone = rs;
                break; // Only focus the first available runestone
            }
        }
    }

    private void HandleRunestoneInteraction()
    {
        if (focusedRunestone != null && Input.GetKeyDown(interactKey))
        {
            focusedRunestone.ActivateRunestone();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
*/
/* DRAFT 2 - Add player animation
using UnityEngine;

public class RunestoneInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;           // Distance to interact
    public KeyCode interactKey = KeyCode.E;    // Key to activate runestone
    public LayerMask runestoneLayer;           // Layer for runestones

    [Header("Animation Settings")]
    public Animator playerAnimator;            // Reference to player's Animator
    public string interactAnimation = "Interact"; // Name of the animation trigger
    public float animationDelay = 0.5f;        // Delay before activating runestone

    private Runestone focusedRunestone = null;
    private bool isInteracting = false;

    void Start()
    {
        // If animator not assigned, try to get it from this GameObject
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isInteracting)
        {
            CheckForRunestone();
            HandleRunestoneInteraction();
        }
    }

    private void CheckForRunestone()
    {
        focusedRunestone = null;

        // Check all colliders in range on the runestone layer
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, runestoneLayer);
        foreach (Collider hit in hits)
        {
            Runestone rs = hit.GetComponent<Runestone>();
            if (rs != null && !rs.IsActivated())
            {
                focusedRunestone = rs;
                break; // Only focus the first available runestone
            }
        }
    }

    private void HandleRunestoneInteraction()
    {
        if (focusedRunestone != null && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(InteractionSequence());
        }
    }

    private System.Collections.IEnumerator InteractionSequence()
    {
        isInteracting = true;

        // Step 1: Play the interaction animation
        if (playerAnimator != null && !string.IsNullOrEmpty(interactAnimation))
        {
            playerAnimator.SetTrigger(interactAnimation);

            // Optional: Disable player movement during animation
            // GetComponent<PlayerMovement>().enabled = false;
        }

        // Step 2: Wait for animation to reach the right point
        yield return new WaitForSeconds(animationDelay);

        // Step 3: Activate the runestone
        if (focusedRunestone != null)
        {
            focusedRunestone.ActivateRunestone();
        }

        // Step 4: Wait for animation to finish (optional)
        // You can use animation events for more precise timing

        isInteracting = false;
    }

    // Animation Event method (call this from your animation timeline)
    public void OnInteractAnimationComplete()
    {
        // This method can be called by an Animation Event
        // Useful if you want precise timing
    }

    private void OnDrawGizmosSelected()
    {
        // Show interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
*/
using UnityEngine;
using System.Collections;

public class RunestoneInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask runestoneLayer;

    [Header("Animation Settings")]
    public Animator playerAnimator;
    public string interactAnimation = "Interact";
    public float animationDelay = 0.5f;

    private Runestone focusedRunestone = null;
    private bool isInteracting = false;

    void Start()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isInteracting)
        {
            CheckForRunestone();
            HandleRunestoneInteraction();
            UpdateUI();
        }
    }

    private void CheckForRunestone()
    {
        // Store the previous focused runestone
        Runestone previousRunestone = focusedRunestone;
        focusedRunestone = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, runestoneLayer);

        foreach (Collider hit in hits)
        {
            Runestone rs = hit.GetComponent<Runestone>();
            if (rs != null && !rs.IsActivated())
            {
                focusedRunestone = rs;
                break;
            }
        }

        // If we had a runestone focused but now we don't, hide the UI
        if (previousRunestone != null && focusedRunestone == null)
        {
            UIManager.Instance.HideInteractionPrompt();
        }
    }

    private void UpdateUI()
    {
        if (focusedRunestone != null)
        {
            UIManager.Instance.ShowInteractionPrompt("E - Activate Runestone");
        }
        // Note: We don't hide UI here anymore - it's handled in CheckForRunestone
    }

    private void HandleRunestoneInteraction()
    {
        if (focusedRunestone != null && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(InteractionSequence());
        }
    }

    private System.Collections.IEnumerator InteractionSequence()
    {
        isInteracting = true;

        // Hide UI immediately when interaction starts
        UIManager.Instance.HideInteractionPrompt();

        if (playerAnimator != null && !string.IsNullOrEmpty(interactAnimation))
        {
            playerAnimator.SetTrigger(interactAnimation);
        }

        yield return new WaitForSeconds(animationDelay);

        if (focusedRunestone != null)
        {
            focusedRunestone.ActivateRunestone();
        }

        isInteracting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = focusedRunestone != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}