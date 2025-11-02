using UnityEngine;
using System.Collections;

public class FairyCinematicController : MonoBehaviour
{
    [Header("References")]
    public Transform playerHandPosition; // Where fairy goes to get power
    public Transform heartbloomTree;
    public ParticleSystem powerUpParticles;
    public ParticleSystem trailParticles;

    [Header("Animation Settings")]
    public float flyToPlayerSpeed = 3f;
    public float flyToTreeSpeed = 5f;
    public float powerTransferTime = 2f;
    public float rotationSpeed = 180f;

    [Header("Fairy States")]
    public Light fairyGlow;
    public float maxGlowIntensity = 5f;

    private Vector3 startPosition;
    private bool isInCinematic = false;
    private Animator animator;

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();

        // Start with fairy dim
        if (fairyGlow != null)
            fairyGlow.intensity = 0.1f;

        // Register for events
        GameEventManager.Instance.OnAllPetalsCollected += StartCinematic;
    }

    void StartCinematic()
    {
        if (isInCinematic) return;

        isInCinematic = true;
        StartCoroutine(CinematicSequence());
    }

    IEnumerator CinematicSequence()
    {
        GameEventManager.Instance.TreeActivationStarted();

        // Step 1: Fly to player's hand
        yield return StartCoroutine(FlyToPosition(playerHandPosition.position, flyToPlayerSpeed));

        // Step 2: Power transfer sequence
        yield return StartCoroutine(PowerTransfer());

        // Step 3: Fly to Heartbloom tree
        yield return StartCoroutine(FlyToPosition(heartbloomTree.position, flyToTreeSpeed));

        // Step 4: Activate the tree
        ActivateTree();
    }

    IEnumerator FlyToPosition(Vector3 targetPosition, float speed)
    {
        if (animator != null)
            animator.SetBool("IsFlying", true);

        if (trailParticles != null)
            trailParticles.Play();

        float distance = Vector3.Distance(transform.position, targetPosition);

        while (distance > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Look at target
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            distance = Vector3.Distance(transform.position, targetPosition);
            yield return null;
        }
    }

    IEnumerator PowerTransfer()
    {
        if (animator != null)
            animator.SetTrigger("PowerUp");

        // Play power-up particles
        if (powerUpParticles != null)
            powerUpParticles.Play();

        // Gradually increase glow
        float elapsedTime = 0f;
        while (elapsedTime < powerTransferTime)
        {
            if (fairyGlow != null)
            {
                float intensity = Mathf.Lerp(0.1f, maxGlowIntensity, elapsedTime / powerTransferTime);
                fairyGlow.intensity = intensity;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure full glow
        if (fairyGlow != null)
            fairyGlow.intensity = maxGlowIntensity;

        GameEventManager.Instance.FairyPowerRestored();
    }

    void ActivateTree()
    {
        HeartbloomTreeController treeController = heartbloomTree.GetComponent<HeartbloomTreeController>();
        if (treeController != null)
        {
            treeController.ActivateTree();
        }

        if (animator != null)
            animator.SetBool("IsFlying", false);

        GameEventManager.Instance.TreeActivated();

        // Fairy can now return to following player or stay at tree
        Debug.Log("Fairy cinematic completed!");
    }

    void OnDestroy()
    {
        // Unregister events
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnAllPetalsCollected -= StartCinematic;
        }
    }
}