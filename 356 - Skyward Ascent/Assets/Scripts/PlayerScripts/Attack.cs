/* DRAFT 1
using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animator animator;
    public Weapon weapon; // reference to weapon hit script

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        animator.SetTrigger("Attack");

        // enable the weapon collider for short window
        weapon.EnableDamage();
    }
}*/
using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animator animator;
    public Weapon weapon;

    [Header("Combo Settings")]
    public float comboWindow = 0.5f;
    public int maxCombo = 3;

    private float lastAttackTime = 0f;
    private int currentCombo = 0;
    private bool inCombo = false;
    private bool canAcceptInput = true;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAcceptInput)
        {
            if (inCombo && currentCombo < maxCombo && Time.time - lastAttackTime <= comboWindow)
            {
                // Continue combo
                PerformComboAttack();
            }
            else if (!inCombo || Time.time - lastAttackTime > comboWindow)
            {
                // Start new combo
                StartCombo();
            }
        }

        // Auto-reset combo if window expires
        if (inCombo && Time.time - lastAttackTime > comboWindow)
        {
            ResetCombo();
        }
    }

    void StartCombo()
    {
        currentCombo = 1;
        inCombo = true;
        PerformAttack("Attack1");
        Debug.Log("Started new combo - Attack1");
    }

    void PerformComboAttack()
    {
        currentCombo++;
        string attackTrigger = "Attack" + currentCombo;
        PerformAttack(attackTrigger);
        Debug.Log($"Combo continued - {attackTrigger}");
    }

    void PerformAttack(string triggerName)
    {
        lastAttackTime = Time.time;
        canAcceptInput = false; // Prevent spam clicking

        // Reset all triggers to avoid conflicts
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");

        // Set the current trigger
        animator.SetTrigger(triggerName);

        weapon.EnableDamage();

        Debug.Log($"Attack performed: {triggerName}");

        // Re-enable input after a short delay
        Invoke("EnableInput", 0.1f);
    }

    void EnableInput()
    {
        canAcceptInput = true;
    }

    void ResetCombo()
    {
        inCombo = false;
        currentCombo = 0;
        Debug.Log("Combo reset");
    }

    // Call this via Animation Event at the end of each attack animation
    public void OnAttackEnd()
    {
        // Optional: Add any cleanup here
    }
}