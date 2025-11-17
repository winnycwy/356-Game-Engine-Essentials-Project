using UnityEngine;
using Ilumisoft.HealthSystem;

public class BossController : MonoBehaviour
{

    [Header("Health")]
    public Health bossHealth;

    [Header("Animator")]
    public Animator anim;

    [Header("Phase Controllers")]
    public BossPhase1_Attacks phase1Controller;
    public BossPhase2_Attacks phase2Controller;

    [Header("Activation")]
    public bool startActivated = false; // optional, auto-start

    [Header("Phase 3 Dialogue")]
    public DialogueSystem dialogueSystem;
    public string[] phase3Lines = new string[]
    {
        "I see now... I was not escaping the past... I was escaping myself.",
        "This tower... these memories... they were never my prison. I was.",
        "Thank you... for helping me remember what I truly am."
    };

    [Header("Ending")]
    public EndingManager endingManager;
    public float endingSequenceDelay = 3f;


    public string phase3Speaker = "Boss";

    private bool bossActivated = false;
    private bool phase2Started = false;
    private bool phase3Started = false;

    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<Health>();

        if (anim == null)
            anim = GetComponent<Animator>();

        bossHealth.OnHealthChanged += OnBossHealthChanged;
        bossHealth.OnHealthEmpty += OnBossDeath; // ADDED THIS LINE

        // Disable attacks until activation
        if (phase1Controller != null)
            phase1Controller.enabled = startActivated;

        if (startActivated)
        {
            bossActivated = true;
            anim.SetTrigger("StartBattle");
        }

    }

    private void OnBossDeath()
    {
        if (!phase3Started)
        {
            StartPhase3();
        }
    }

    private void OnBossHealthChanged(float difference)
    {
        if (!bossActivated) return; // Only track health changes after activation

        // Damage reaction animation
        if (difference < 0)
            anim.SetTrigger("Hit");


        float current = bossHealth.CurrentHealth;
        float max = bossHealth.MaxHealth;

        // PHASE 2 TRIGGER
        if (!phase2Started && current <= max * 0.60f)
        {
            StartPhase2();
        }

        // PHASE 3 TRIGGER
        if (!phase3Started && current <= 0)
        {
            StartPhase3();
        }
    }

    public void ActivateBoss()
    {
        if (bossActivated) return;

        bossActivated = true;
        Debug.Log("Boss activated!");

        anim.SetTrigger("StartBattle");

        if (phase1Controller != null)
            phase1Controller.enabled = true;
    }

    private void StartPhase2()
    {
        phase2Started = true;
        Debug.Log("BOSS: Switching to PHASE 2");

        if (phase2Controller != null)
            phase2Controller.StartPhase2();
    }

    private void StartPhase3()
    {
        phase3Started = true;
        Debug.Log("BOSS: Switching to PHASE 3 (Dialogue)");

        anim.SetTrigger("Kneel");


        // Disable any remaining attacks
        if (phase1Controller != null) phase1Controller.enabled = false;
        if (phase2Controller != null) phase2Controller.enabled = false;

        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= OnBossHealthChanged;
            bossHealth.OnHealthEmpty -= OnBossDeath;
        }

        // Start dialogue and ending sequence
        StartCoroutine(StartDialogueAndEnding());
    }

    private System.Collections.IEnumerator StartDialogueAndEnding()
    {
        // Wait for death animation to start
        yield return new WaitForSeconds(1f);

        // Play dialogue
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(phase3Lines, phase3Speaker);

            // Wait for dialogue to complete
            yield return new WaitUntil(() => !dialogueSystem.IsDialogueActive);
        }

        // Wait a moment before ending screen
        yield return new WaitForSeconds(endingSequenceDelay);

        // Start ending sequence (this will play the redemption music)
        if (endingManager != null)
        {
            endingManager.StartEndingSequence();
        }
        

    }
}