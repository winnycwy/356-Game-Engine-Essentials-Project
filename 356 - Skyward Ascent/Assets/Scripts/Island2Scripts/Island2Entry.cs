using UnityEngine;
using System.Collections;

public class Island2Entry : MonoBehaviour
{
    [Header("Island 2 Setup")]
    public Island2LightingController lightingController;
    public PostProcessingDarkness postProcessingDarkness;
    public GameObject wizardSpirit;
    public DialogueSystem dialogueSystem;
    public Animator wizardAnimator;

    [Header("Wizard Dialogue")]
    [TextArea]
    public string[] wizardDialogue = {
        "The Sylvan Canopy... even darker than I remembered.",
        "These woods hold secrets in their shadows, Staff-Bearer.",
        "I sense a familiar spark here - a flame that refused to be extinguished.",
        "Seek the guardian of these woods. Its fire may ignite a new power within your staff.",
        "But be warned... some paths require both light and warmth to navigate safely.",
        "Trust in the flames you carry, both old and new."
    };

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartIsland2Sequence();
        }
    }

    private void StartIsland2Sequence()
    {
        // Start darkness transition
        if (lightingController != null)
            lightingController.ActivateIsland2Darkness();

        if (postProcessingDarkness != null)
            postProcessingDarkness.ActivateDarkness();

        // Start wizard cutscene after a brief delay
        Invoke(nameof(StartWizardCutscene), 2f);
    }

    private void StartWizardCutscene()
    {
        // Show wizard
        if (wizardSpirit != null)
            wizardSpirit.SetActive(true);

        // Play appear animation
        if (wizardAnimator != null)
            wizardAnimator.SetTrigger("Appear");

        // Start dialogue
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(wizardDialogue, "Aetherius");
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