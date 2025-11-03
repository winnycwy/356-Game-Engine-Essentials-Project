using UnityEngine;
using StarterAssets;
using System.Collections;

public class FlowerTrigger : MonoBehaviour
{
    public Animator playerAnimator;
    public string triggerName = "PickUpFlower";

    private bool playerInRange = false;
    private StarterAssetsInputs playerInput;
    private GameObject flower;

    void Start()
    {
        flower = gameObject; // reference to this flower
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // find the player's Animator if not assigned
            if (playerAnimator == null)
            {
                playerAnimator = other.GetComponentInChildren<Animator>();
            }

            if (playerInput == null)
                playerInput = other.GetComponent<StarterAssetsInputs>();

            // Reset interact to prevent accidental trigger
            if (playerInput != null)
                playerInput.interact = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // reset interact when leaving
            if (playerInput != null)
                playerInput.interact = false;
        }
    }

    void Update()
    {
        // Only trigger if player is in the zone AND presses E
        if (playerInRange && playerInput != null && playerInput.interact)
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(triggerName);
                StartCoroutine(DisableFlowerAfterDelay(0.8f)); // hardcode 0.8 seconds
            }
            else
            {
                Debug.LogWarning("Animator not found on Player.");
            }

            // Reset interact so it can't trigger again accidentally
            playerInput.interact = false;
        }

    }

    private IEnumerator DisableFlowerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        flower.SetActive(false);
    }
}
