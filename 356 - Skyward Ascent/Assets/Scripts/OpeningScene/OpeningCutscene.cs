using UnityEngine;
using System.Collections;

public class OpeningCutscene : MonoBehaviour
{
    [Header("Cutscene References")]
    public GameObject wizardSpirit;
    public DialogueSystem dialogueSystem;  // Your existing dialogue system
    public Animator wizardAnimator;        // Wizard's animator component

    [Header("Wizard Dialogue")]
    [TextArea]
    public string[] wizardDialogue = {
        "Ah, you are awake. Do you know this place?",
        "No... I see you do not. We are both lost, it seems.",
        "That staff... it feels important.",
        "Do not fear the height, Staff-Bearer. Focus on the path ahead."
    };

    void Start()
    {
        // Hide wizard at start
        if (wizardSpirit != null)
            wizardSpirit.SetActive(false);

        // Start cutscene after delay
        Invoke(nameof(StartCutscene), 1f);
    }

    void StartCutscene()
    {
        // Show wizard
        if (wizardSpirit != null)
            wizardSpirit.SetActive(true);

        // Play wizard animation (e.g., "Appear" or "Idle")
        if (wizardAnimator != null)
            wizardAnimator.SetTrigger("StartTalking");

        // Start dialogue using your existing system
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(wizardDialogue, "Aetherius");

            // Hide wizard when dialogue ends
            StartCoroutine(HideWizardAfterDialogue());
        }
    }

    private IEnumerator HideWizardAfterDialogue()
    {
        // Wait for dialogue to complete
        while (dialogueSystem.IsDialogueActive)
        {
            yield return null;
        }

        // Play disappear animation
        if (wizardAnimator != null)
            wizardAnimator.SetTrigger("Disappear");

        // Wait for animation then hide
        yield return new WaitForSeconds(1f);

        if (wizardSpirit != null)
            wizardSpirit.SetActive(false);
    }
}