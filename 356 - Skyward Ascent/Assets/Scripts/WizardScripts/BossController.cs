using UnityEngine;
using Ilumisoft.HealthSystem;

public class BossController : MonoBehaviour
{
    public Health bossHealth;

    [Header("Phase Controllers")]
    public BossPhase1_Attacks phase1Controller;
    public BossPhase2_Attacks phase2Controller;

    [Header("Trigger Settings")]
    public Collider activationTrigger; // Assign a trigger collider in the scene
    private bool bossActivated = false;

    private bool phase2Started = false;

    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<Health>();

        bossHealth.OnHealthChanged += OnBossHealthChanged;

        
        if (phase1Controller != null)
            phase1Controller.enabled = false;
    }

    private void OnBossHealthChanged(float difference)
    {
        float current = bossHealth.CurrentHealth;
        float max = bossHealth.MaxHealth;

        // PHASE 2 TRIGGER
        if (!phase2Started && current <= max * 0.60f)
        {
            StartPhase2();
        }

        
        if (current <= 0)
        {
            Debug.Log("BOSS: DEAD — Start Phase 3 later.");
        }
    }

    void StartPhase2()
    {
        phase2Started = true;

        Debug.Log("BOSS: Switching to PHASE 2");

        
        if (phase2Controller != null)
            phase2Controller.StartPhase2();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (bossActivated) return;

        if (other.CompareTag("Player"))
        {
            bossActivated = true;
            Debug.Log("Boss activated!");

            
            if (phase1Controller != null)
                phase1Controller.enabled = true;

            
        }
    }
}