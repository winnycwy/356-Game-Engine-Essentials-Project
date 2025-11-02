using UnityEngine;
using UnityEngine.Events;

public class RunestoneManager : MonoBehaviour
{
    [Header("Runestone Configuration")]
    public Runestone[] runestones;

    [Header("Events")]
    public UnityEvent onAllRunestonesActivated;

    private int activatedCount = 0;

    void Start()
    {
        // Initialize all runestones
        foreach (Runestone runestone in runestones)
        {
            if (runestone != null)
            {
                // You could register events here if needed
            }
        }
    }

    public void OnRunestoneActivated(Runestone activatedRunestone)
    {
        activatedCount++;

        Debug.Log($"Runestone activated! {activatedCount}/{runestones.Length}");

        // Check if all runestones are activated
        if (activatedCount >= runestones.Length)
        {
            AllRunestonesActivated();
        }
    }

    private void AllRunestonesActivated()
    {
        Debug.Log("All runestones activated! Opening the ancient door...");

        // Trigger whatever should happen when all are activated
        onAllRunestonesActivated?.Invoke();
    }

    public int GetActivatedCount()
    {
        return activatedCount;
    }

    public int GetTotalRunestones()
    {
        return runestones.Length;
    }
}
