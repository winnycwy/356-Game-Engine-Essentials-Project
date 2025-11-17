using UnityEngine;
using System.Collections;

public class BossPhase1_Attacks : MonoBehaviour
{
    public Transform boss;
    public Transform player;

    public GameObject clonePrefab;
    public GameObject orbPrefab;

    public float cloneCooldown = 6f;
    public float orbCooldown = 5f;

    private Animator animator;
    private bool canUseClone = true;
    private bool canUseOrb = true;
    private bool isAttacking = false;

    void Start()
    {
        if (animator == null)
        {
            // Method 1: Try to get from the same GameObject
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            // Method 2: Try to get from parent
            animator = GetComponentInParent<Animator>();
        }

        if (animator == null)
        {
            // Method 3: Try to find anywhere on the boss
            animator = FindObjectOfType<Animator>();
        }

        if (animator != null)
        {
            Debug.Log("Animator found: " + animator.gameObject.name);
        }
        else
        {
            Debug.LogError("No Animator found! Please assign it manually in the Inspector.");
        }
    }

    void Update()
    {
        // Debug current animation state
        if (animator != null && Input.GetKeyDown(KeyCode.D))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current State: " + stateInfo.fullPathHash + " | Is Attacking: " + stateInfo.IsName("Attack"));
        }

        if (canUseClone)
        {
            StartCoroutine(SpawnClones());
        }

        if (canUseOrb)
        {
            StartCoroutine(ShootOrbs());
        }
    }

    IEnumerator SpawnClones()
    {
        canUseClone = false;

        // Wait if already attacking
        while (isAttacking)
            yield return null;

        isAttacking = true;

        if (animator != null)
        {
            // Check current state
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current state: " + (state.IsName("Idle") ? "Idle" : "Other"));

            // Check if we can transition
            bool canTransition = animator.IsInTransition(0);
            Debug.Log("Is in transition: " + canTransition);

            // Set trigger
            animator.SetTrigger("Attack");
            Debug.Log("Attack trigger set!");

            // Check immediately after
            animator.Update(0.01f);
            state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("State after trigger: " + (state.IsName("Attack") ? "Attack" : "Not Attack"));
        }

        // Wait for animation to reach the spawn point
        yield return new WaitForSeconds(0.5f);

        // spawn clones
        for (int i = 0; i < 1; i++)
        {
            Vector3 pos = boss.position + Random.insideUnitSphere * 3f;
            pos.y = boss.position.y;
            Instantiate(clonePrefab, pos, Quaternion.identity);
        }

        Debug.Log("Boss: Shadow Clones spawned");

        // Wait for attack animation to mostly finish
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;

        yield return new WaitForSeconds(cloneCooldown);
        canUseClone = true;
    }

    IEnumerator ShootOrbs()
    {
        canUseOrb = false;

        // Wait if already attacking
        while (isAttacking)
            yield return null;

        isAttacking = true;

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            Debug.Log("Attack animation triggered for orbs!");
        }
        else
        {
            Debug.LogError("Cannot play attack - Animator is still null!");
        }

        // Wait for animation to reach the shoot point
        yield return new WaitForSeconds(0.3f);

        // spawn 2 orbs
        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = boss.position + boss.forward * 2f;
            pos.y = boss.position.y + 1f;

            Instantiate(orbPrefab, pos, Quaternion.identity)
                .GetComponent<DarkFaeOrb>()
                .SetTarget(player);
        }

        Debug.Log("Boss: Dark Fae Orbs fired");

        // Wait for attack animation to mostly finish
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;

        yield return new WaitForSeconds(orbCooldown);
        canUseOrb = true;
    }
}