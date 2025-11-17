using UnityEngine;
using Ilumisoft.HealthSystem;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Health")]
    public Health bossHealth;

    [Header("Phase Controllers")]
    public BossPhase1_Attacks phase1Controller;
    public BossPhase2_Attacks phase2Controller;

    [Header("Animation")]
    public Animator animator;

    [Header("Activation")]
    public bool startActivated = false;

    [Header("Phase 3 Dialogue")]
    public DialogueSystem dialogueSystem;
    public string[] phase3Lines = new string[]
    {
        "I see now... I was not escaping the past... I was escaping myself.",
        "This tower... these memories... they were never my prison. I was.",
        "Thank you... for helping me remember what I truly am."
    };
    public string phase3Speaker = "Boss";

    private bool bossActivated = false;
    private bool phase2Started = false;
    private bool phase3Started = false;

    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<Health>();

        if (animator == null)
            animator = GetComponent<Animator>();

        bossHealth.OnHealthChanged += OnBossHealthChanged;
        bossHealth.OnHealthEmpty += OnBossDeath;

        if (phase1Controller != null)
            phase1Controller.enabled = startActivated;

        if (startActivated)
            bossActivated = true;
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
        if (!bossActivated) return;

        float current = bossHealth.CurrentHealth;
        float max = bossHealth.MaxHealth;

        // Play damage animation when taking damage
        if (difference < 0 && animator != null && !animator.GetBool("isDead"))
        {
            animator.SetTrigger("Damage");
        }

        // PHASE 2 TRIGGER ONLY
        if (!phase2Started && current <= max * 0.60f)
        {
            StartPhase2();
        }
    }

    public void ActivateBoss()
    {
        if (bossActivated) return;

        bossActivated = true;
        Debug.Log("Boss activated!");

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

        // Set death bool (not trigger)
        if (animator != null)
            animator.SetBool("isDead", true);

        // Disable any remaining attacks
        if (phase1Controller != null) phase1Controller.enabled = false;
        if (phase2Controller != null) phase2Controller.enabled = false;

        // Unsubscribe from health events so boss stops reacting to damage
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= OnBossHealthChanged;
            bossHealth.OnHealthEmpty -= OnBossDeath;
        }

        // Start dialogue after a delay to let death animation play
        StartCoroutine(StartDialogueAfterDeath());
    }

    private IEnumerator StartDialogueAfterDeath()
    {
        // Wait for death animation to start
        yield return new WaitForSeconds(1f);

        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(phase3Lines, phase3Speaker);
        }
    }
}