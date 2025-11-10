using UnityEngine;
using System.Collections;

public class FoxNPC : MonoBehaviour
{
    [Header("Fox Dialogue")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;
    public bool startDialogueAutomatically = true;

    [TextArea]
    public string[] foxDialogue = {
        "Greetings, traveler. I sense the wizard's magic upon you... and the light you carry.",
        "I am Ember, last guardian of the ancient flame in these darkening woods.",
        "Mischievous Spritelings have stolen a sacred key from our ancestral den - a key that holds memories of this tower's past.",
        "Without it, the balance of this forest falters, and the shadows grow bolder.",
        "The Spritelings nest high in the canopy, drawn to the key's shimmer... a place too treacherous for me to reach alone.",
        "Your light has guided you this far... would you help me retrieve what was lost? I can offer you more than gratitude in return."
    };

    [Header("Quest Items")]
    public GameObject sacredKey; // Reference to the key object
    public GameObject[] spritelingNests; // Spriteling locations

    private bool playerInRange = false;
    private bool hasSpoken = false;

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasSpoken && other.CompareTag("Player"))
        {
            playerInRange = true;

            if (startDialogueAutomatically)
            {
                StartConversation();
            }
            else
            {
                if (interactPromptUI != null)
                    interactPromptUI.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        // Manual interaction if not auto-start
        if (!startDialogueAutomatically && playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null || hasSpoken) return;
        if (dialogueSystem.IsDialogueActive) return;

        dialogueSystem.StartDialogue(foxDialogue, "Ember");
        hasSpoken = true;

        // Hide prompt
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // Start quest
        StartCoroutine(StartFoxQuest());
    }

    private IEnumerator StartFoxQuest()
    {
        // Wait for dialogue to complete
        while (dialogueSystem.IsDialogueActive)
        {
            yield return null;
        }

        // Activate Spritelings and key quest
        ActivateSpritelingQuest();

        Debug.Log("Fox quest started! Find the Spritelings and retrieve the sacred key.");
    }

    private void ActivateSpritelingQuest()
    {
        // Activate Spriteling enemies
        foreach (GameObject nest in spritelingNests)
        {
            if (nest != null)
                nest.SetActive(true);
        }

        // Hide the sacred key (will be revealed when Spritelings are defeated)
        if (sacredKey != null)
            sacredKey.SetActive(false);
    }

    // Call this when player defeats Spritelings and finds key
    public void OnKeyRetrieved()
    {
        Debug.Log("Sacred key retrieved! Return to Ember.");
        // You can add more dialogue or rewards here
    }
}