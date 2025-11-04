using UnityEngine;

public class WizardDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea]
    public string[] wizardLines = {
        "Greetings, traveler. I am Eldrin, keeper of ancient knowledge.",
        "The crystals you seek are hidden in the forbidden caverns.",
        "Be wary of the shadow creatures that dwell there."
    };

    [Header("References")]
    public DialogueSystem dialogueSystem;

    void Start()
    {
        // Automatically find dialogue system in scene if not assigned
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Start dialogue automatically when player enters trigger
        if (other.CompareTag("Player") && dialogueSystem != null && !dialogueSystem.IsDialogueActive)
        {
            dialogueSystem.StartDialogue(wizardLines, "Wizard Eldrin");
        }
    }
}