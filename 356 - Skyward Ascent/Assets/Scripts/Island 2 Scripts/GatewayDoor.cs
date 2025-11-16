using UnityEngine;
using System.Collections;

public class GatewayDoor : MonoBehaviour
{
    [Header("Door References")]
    public string doorName = "GatewayToIsland3";
    public Transform doorTransform; // The actual door part that moves

    [Header("Door Opening Settings")]
    public float openAngle = 90f; // Angle to open the door
    public float openDuration = 2.0f; // How long it takes to open
    public Vector3 openAxis = Vector3.up; // Axis to rotate around (usually up for doors)

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public string lockedMessage = "E - Unlock Door";
    public string unlockedMessage = "E - Open Door";

    [Header("Key Reference")]
    public HiddenKey requiredKey; // Assign the key in Inspector

    // State variables
    private bool isLocked = true;
    private bool isOpen = false;
    private bool isOpening = false;
    private bool playerInRange = false;

    // Audio
    private AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    // Original rotation
    private Quaternion originalRotation;
    private Quaternion targetRotation;

    void Start()
    {
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Store original rotation
        if (doorTransform != null)
        {
            originalRotation = doorTransform.localRotation;
        }
        else
        {
            Debug.LogError("Door Transform not assigned!");
        }

        // Subscribe to key collection event
        if (requiredKey != null)
        {
            requiredKey.OnKeyCollected += OnKeyCollected;
        }
        else
        {
            Debug.LogWarning("No required key assigned to door!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt();
            Debug.Log("Player entered door interaction range");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionPrompt();
            Debug.Log("Player left door interaction range");
        }
    }

    void Update()
    {
        // Handle interaction input only when player is near
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryInteractWithDoor();
        }
    }

    private void ShowInteractionPrompt()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager instance not found!");
            return;
        }

        if (isLocked)
        {
            if (requiredKey != null && requiredKey.IsCollected())
            {
                // Player has key but door is still locked
                UIManager.Instance.ShowInteractionPrompt(lockedMessage);
            }
            // No message if player doesn't have key - removed noKeyMessage
        }
        else
        {
            // Door is unlocked
            UIManager.Instance.ShowInteractionPrompt(unlockedMessage);
        }
    }

    private void HideInteractionPrompt()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideInteractionPrompt();
        }
    }

    private void TryInteractWithDoor()
    {
        if (isOpening) return;

        Debug.Log("Trying to interact with door. Locked: " + isLocked);

        if (isLocked)
        {
            // Check if player has the key
            if (requiredKey != null && requiredKey.IsCollected())
            {
                UnlockAndOpenDoor();
            }
            else
            {
                // Play locked sound but don't show message
                if (lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                Debug.Log("Door is locked! Find the hidden key first.");
            }
        }
        else
        {
            // Toggle door open/close
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }

    private void UnlockAndOpenDoor()
    {
        isLocked = false;
        Debug.Log("Door unlocked!");

        // Play unlock sound
        if (unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        // Update UI text immediately
        if (playerInRange)
        {
            ShowInteractionPrompt();
        }

        // Automatically open the door after unlocking
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (isOpening || isOpen) return;

        isOpening = true;
        Debug.Log("Opening door...");

        // Calculate target rotation
        targetRotation = originalRotation * Quaternion.Euler(openAxis * openAngle);

        // Play open sound
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Hide interaction prompt while opening
        HideInteractionPrompt();

        StartCoroutine(AnimateDoorOpen());
    }

    private void CloseDoor()
    {
        if (isOpening || !isOpen) return;

        isOpening = true;
        Debug.Log("Closing door...");

        targetRotation = originalRotation;

        // Play close sound
        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        // Hide interaction prompt while closing
        HideInteractionPrompt();

        StartCoroutine(AnimateDoorClose());
    }

    private System.Collections.IEnumerator AnimateDoorOpen()
    {
        float timer = 0f;
        Quaternion startRotation = doorTransform.localRotation;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / openDuration;

            doorTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, progress);

            yield return null;
        }

        // Ensure final rotation
        doorTransform.localRotation = targetRotation;

        isOpen = true;
        isOpening = false;
        Debug.Log("Door fully opened");

        // Show prompt again if player is still in range
        if (playerInRange)
        {
            ShowInteractionPrompt();
        }
    }

    private System.Collections.IEnumerator AnimateDoorClose()
    {
        float timer = 0f;
        Quaternion startRotation = doorTransform.localRotation;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / openDuration;

            doorTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, progress);

            yield return null;
        }

        // Ensure final rotation
        doorTransform.localRotation = targetRotation;

        isOpen = false;
        isOpening = false;
        Debug.Log("Door fully closed");

        // Show prompt again if player is still in range
        if (playerInRange)
        {
            ShowInteractionPrompt();
        }
    }

    private void OnKeyCollected(HiddenKey collectedKey)
    {
        Debug.Log("Key collected! Door can now be unlocked.");

        // If player is already in range, update the UI
        if (playerInRange)
        {
            ShowInteractionPrompt();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from event
        if (requiredKey != null)
        {
            requiredKey.OnKeyCollected -= OnKeyCollected;
        }
    }
}