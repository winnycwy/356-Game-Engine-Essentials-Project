using UnityEngine;
using System.Collections;

public class OpeningCutscene : MonoBehaviour
{
    [Header("Cutscene References")]
    public GameObject wizardSpirit;
    public DialogueSystem dialogueSystem;  // Your existing dialogue system
    public Animator wizardAnimator;        // Wizard's animator component

    [Header("Audio Settings")]
    public AudioSource typingAudioSource;

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
            // Only subscribe to line events now
            dialogueSystem.OnLineTypingStart += StartTypingSound;
            dialogueSystem.OnLineTypingComplete += StopTypingSound;

            dialogueSystem.StartDialogue(wizardDialogue, "Aetherius");

            // Hide wizard when dialogue ends
            StartCoroutine(HideWizardAfterDialogue());
        }
    }

    private void StartTypingSound()
    {
        if (typingAudioSource != null && !typingAudioSource.isPlaying)
        {
            typingAudioSource.loop = true;
            typingAudioSource.Play();
        }
    }

    private void StopTypingSound()
    {
        if (typingAudioSource != null && typingAudioSource.isPlaying)
        {
            typingAudioSource.Stop();
        }
    }

    void OnDestroy()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.OnLineTypingStart -= StartTypingSound;
            dialogueSystem.OnLineTypingComplete -= StopTypingSound;
        }

        StopTypingSound();
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