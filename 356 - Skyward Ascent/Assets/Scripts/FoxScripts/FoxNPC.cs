using UnityEngine;
using System.Collections;

public class FoxNPC : MonoBehaviour
{
    [Header("Fox Dialogue")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;
    public bool startDialogueAutomatically = true;

    [TextArea]
    public string[] initialDialogue = {
        "Greetings, traveler. I sense the wizard's magic upon you... and the light you carry.",
        "I am Ignis, last guardian of the ancient flame in these darkening woods.",
        "I replenish power through fire crystals, but dark magic have concealed them",
        "Without it, my lifeforce falters, and I grow weaker everyday.",
        "Your light has guided you this far... would you help me retrieve what was lost? I can offer you more than gratitude in return."
    };

    [TextArea]
    public string[] afterCrystalDialogue = {
        "Thank you for retrieving the fire crystal!",
        "I will now infuse your staff with the power of fire magic.",
        "...",
        "I have heard about your journey to find Aetherius.",
        "Be warned, he is not the person he once was...",
        "Up ahead, mischievous Spritelings have stolen a sacred key from our ancestral den - a key that holds memories of this tower's past.",
        "The Spritelings nest high in the canopy, drawn to the key's shimmer... a place too treacherous for me to reach alone.",
        "Find the key, you'll need it to unlock the wizard's lair"
    };

    [Header("Quest Items")]
    public GameObject fireCrystal; // Hidden fire crystal on tree trunk
    public GameObject sacredKey; // Key at top of tree trunk
    public GameObject[] spritelings; // Spriteling enemies

    [Header("Player Abilities")]
    public PlayerAbilityManager playerAbilities;
    public GameObject fireAbilityUnlockEffect;

    private bool playerInRange = false;
    private bool hasSpokenInitial = false;
    private bool playerHasCrystal = false;
    private bool hasUnlockedFireAbility = false;
    private bool hasGivenKeyQuest = false;

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // Initially hide quest items
        if (fireCrystal != null)
            fireCrystal.SetActive(false);

        if (sacredKey != null)
            sacredKey.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!hasSpokenInitial && startDialogueAutomatically)
            {
                StartInitialConversation();
            }
            else if (playerHasCrystal && !hasUnlockedFireAbility)
            {
                // Player has crystal - trigger ability unlock dialogue
                StartCrystalConversation();
            }
            else if (hasGivenKeyQuest)
            {
                // Optional: Add reminder dialogue if player returns
                ShowReminderDialogue();
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
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!hasSpokenInitial)
            {
                StartInitialConversation();
            }
            else if (playerHasCrystal && !hasUnlockedFireAbility)
            {
                StartCrystalConversation();
            }
            else if (hasGivenKeyQuest)
            {
                ShowReminderDialogue();
            }
        }
    }

    private void StartInitialConversation()
    {
        if (dialogueSystem == null || hasSpokenInitial) return;
        if (dialogueSystem.IsDialogueActive) return;

        dialogueSystem.StartDialogue(initialDialogue, "Ignis");
        hasSpokenInitial = true;

        // Hide prompt
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // Start quest to find fire crystal
        StartCoroutine(StartCrystalQuest());
    }

    private void StartCrystalConversation()
    {
        if (dialogueSystem == null || hasUnlockedFireAbility) return;
        if (dialogueSystem.IsDialogueActive) return;

        dialogueSystem.StartDialogue(afterCrystalDialogue, "Ignis");

        // Hide prompt
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // Unlock fire ability and start key quest
        StartCoroutine(UnlockFireAbilityAndStartKeyQuest());
    }

    private IEnumerator StartCrystalQuest()
    {
        // Wait for dialogue to complete
        while (dialogueSystem.IsDialogueActive)
        {
            yield return null;
        }

        // Reveal fire crystal (hidden on tree trunk, requires Fae Light to see)
        if (fireCrystal != null)
        {
            fireCrystal.SetActive(true);
            Debug.Log("Fire crystal revealed! Use Fae Light ability to find it on the tree trunk.");
        }

        Debug.Log("Find the fire crystal hidden on the tree trunk using Fae Light!");
    }

    private IEnumerator UnlockFireAbilityAndStartKeyQuest()
    {
        // Wait for dialogue to complete
        while (dialogueSystem.IsDialogueActive)
        {
            yield return null;
        }

        // Unlock fire ability for player
        if (playerAbilities != null)
        {
            playerAbilities.UnlockFireAbility();

            // Show unlock effect
            if (fireAbilityUnlockEffect != null)
            {
                fireAbilityUnlockEffect.SetActive(true);
                yield return new WaitForSeconds(2f);
                fireAbilityUnlockEffect.SetActive(false);
            }
        }

        hasUnlockedFireAbility = true;
        playerHasCrystal = false; // Crystal is consumed

        // Activate Spritelings and key quest
        ActivateKeyQuest();

        Debug.Log("Fire ability unlocked! Now find the sacred key at the top of the tree trunk.");
    }

    private void ActivateKeyQuest()
    {
        // Activate Spriteling enemies
        foreach (GameObject spriteling in spritelings)
        {
            if (spriteling != null)
                spriteling.SetActive(true);
        }

        // Show the sacred key at top of tree trunk
        if (sacredKey != null)
            sacredKey.SetActive(true);

        hasGivenKeyQuest = true;
    }

    private void ShowReminderDialogue()
    {
        if (dialogueSystem == null || dialogueSystem.IsDialogueActive) return;

        string[] reminderDialogue = {
            "Remember, find the sacred key that the Spritelings stole.",
            "You'll need it to unlock the wizard's lair up ahead.",
            "Use your new fire ability to defeat the Spritelings!"
        };

        dialogueSystem.StartDialogue(reminderDialogue, "Ignis");
    }

    // Call this when player finds the fire crystal (using Fae Light)
    public void OnCrystalFound()
    {
        if (!playerHasCrystal)
        {
            playerHasCrystal = true;
            Debug.Log("Fire crystal found! Return to Ignis to unlock fire ability.");

            // Hide the crystal after finding it
            if (fireCrystal != null)
                fireCrystal.SetActive(false);
        }
    }

    // Call this when player defeats Spritelings and retrieves key
    public void OnKeyRetrieved()
    {
        Debug.Log("Sacred key retrieved! Door to Island 3 is now open.");
        // This would typically be handled by another script that manages the door/portal
    }

    // Helper methods
    public bool HasUnlockedFireAbility()
    {
        return hasUnlockedFireAbility;
    }

    public bool HasGivenKeyQuest()
    {
        return hasGivenKeyQuest;
    }

    public bool PlayerHasCrystal()
    {
        return playerHasCrystal;
    }
}