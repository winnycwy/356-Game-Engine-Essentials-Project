using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    [Header("Boss Settings")]
    public BossController boss; // Drag the boss GameObject here

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("Player entered trigger, activating boss!");

            if (boss != null)
                boss.ActivateBoss();
        }
    }
}