using UnityEngine;
using Ilumisoft.HealthSystem;

public class BossPhase2_Attacks : MonoBehaviour
{
    [Header("Phase 2 Objects")]
    public GameObject shieldObject;
    public Health[] crystals; // 3 crystals with Health component

    public Transform phase2Position;
    public float moveSpeed = 5f; // optional smoothing

    public BossPhase1_Attacks phase1Attacks;

    private bool phase2Active = false;

    void Start()
    {
        shieldObject.SetActive(false);

        // Disable all crystals at start
        foreach (var c in crystals)
        {
            if (c != null)
                c.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!phase2Active) return;

        // Move boss to the Phase 2 position
        if (phase2Position != null && transform.position != phase2Position.position)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                phase2Position.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    public void StartPhase2()
    {
        if (phase2Active) return;
        phase2Active = true;

        Debug.Log("PHASE 2 STARTED!");

        // Enable shield
        shieldObject.SetActive(true);

        // Enable crystals and subscribe to their death
        foreach (var c in crystals)
        {
            if (c != null)
            {
                c.gameObject.SetActive(true);
                c.OnHealthEmpty += OnCrystalDestroyed;
            }
        }
    }

    private void OnCrystalDestroyed()
    {
        // Make a list of crystals to remove
        for (int i = 0; i < crystals.Length; i++)
        {
            if (crystals[i] != null && !crystals[i].IsAlive)
            {
                crystals[i].OnHealthEmpty -= OnCrystalDestroyed;
                Destroy(crystals[i].gameObject);
                crystals[i] = null; // clear reference
            }
        }

        // Check if all crystals are destroyed
        bool allDead = true;
        foreach (var c in crystals)
        {
            if (c != null)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            EndPhase2();
        }
    }

    private void EndPhase2()
    {
        phase2Active = false;

        // Disable shield
        shieldObject.SetActive(false);

        // Stop Phase 1 attacks (boss no longer attacks)
        if (phase1Attacks != null)
            phase1Attacks.gameObject.SetActive(false);

        Debug.Log("PHASE 2 COMPLETED!");
    }
}