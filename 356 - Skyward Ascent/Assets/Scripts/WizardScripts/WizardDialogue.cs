using UnityEngine;

public class WizardDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string[] wizardLines = {
        "Greetings, traveler. I am Eldrin, keeper of ancient knowledge.",
        "The crystals you seek are hidden in the forbidden caverns.",
        "Be wary of the shadow creatures that dwell there."
    };

    private DialogueSystem dialogueSystem;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !dialogueSystem.IsDialogueActive())
        {
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        dialogueSystem.StartDialogue(wizardLines, "Wizard Eldrin");
    }

    // Call this from other scripts to trigger dialogue
    public void TriggerWizardDialogue()
    {
        if (!dialogueSystem.IsDialogueActive())
        {
            StartDialogue();
        }
    }
}