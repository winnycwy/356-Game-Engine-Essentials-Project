using UnityEngine;
using UnityEngine.Events;

public class RunestoneManager : MonoBehaviour
{
    [Header("Runestone Configuration")]
    public Runestone[] runestones;


    [Header("Events")]
    public UnityEvent onAllRunestonesActivated;

    private int activatedCount = 0;

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
        // Any other UnityEvents
        onAllRunestonesActivated?.Invoke();
    }
}