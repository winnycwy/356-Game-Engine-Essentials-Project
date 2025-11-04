using UnityEngine;
using UnityEngine.UI;

public class InteractableCharacter : MonoBehaviour
{
    [Header("Dialogue")]
    public string characterName = "NPC";
    [TextArea] public string[] dialogueLines;
    [TextArea] public string[] specialDialogue;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public GameObject interactPromptUI;  // UI like "Press E to talk"

    private bool playerInRange = false;
    private bool hasSpecialDialogue = false; // toggled when certain events happen

    void Start()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);

            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
                dialogueSystem.EndDialogue();
        }
    }

    private void StartConversation()
    {
        if (dialogueSystem == null) return;

        // Only start if not active
        if (!dialogueSystem.IsDialogueActive)
        {
            if (hasSpecialDialogue && specialDialogue.Length > 0)
                dialogueSystem.StartDialogue(specialDialogue, characterName);
            else
                dialogueSystem.StartDialogue(dialogueLines, characterName);
        }
    }

    // Called externally (e.g., after picking up flower)
    public void EnableSpecialDialogue()
    {
        hasSpecialDialogue = true;
    }
}