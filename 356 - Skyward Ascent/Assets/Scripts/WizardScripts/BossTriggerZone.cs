using UnityEngine;
using TMPro;

public class BossTriggerZone : MonoBehaviour
{
    [Header("Boss Settings")]
    public BossController boss; // Drag the boss GameObject here

    [Header("UI Settings")]
    public GameObject tooltipPanel;   // The panel you want to show
    public TextMeshProUGUI tooltipText; // The TextMeshPro component inside the panel
    public float displayDuration = 3f; // How long the tooltip stays on screen

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

            // Show tooltip
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(true);

                // Hide tooltip after a delay
                Invoke(nameof(HideTooltip), displayDuration);
            }
        }
    }
    private void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}