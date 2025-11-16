using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    [Header("Ability References")]
    public FaeLightAbility faeLightAbility;
    public FireAbility fireAbility;

    public void UnlockFireAbility()
    {
        if (fireAbility != null)
        {
            fireAbility.UnlockFireAbility();
            Debug.Log("PlayerAbilityManager: Fire ability unlock called!");
        }
        else
        {
            Debug.LogError("PlayerAbilityManager: FireAbility reference is null!");
        }
    }

    // FIXED: Property access without parentheses
    public bool IsFireAbilityUnlocked()
    {
        return fireAbility != null && fireAbility.IsFireAbilityUnlocked;
    }

    public bool IsFaeLightActive()
    {
        return faeLightAbility != null && faeLightAbility.IsLightActive();
    }

    // Helper method to check if references are set
    public void CheckAbilityReferences()
    {
        if (faeLightAbility == null)
            Debug.LogWarning("FaeLightAbility reference not set in PlayerAbilityManager!");

        if (fireAbility == null)
            Debug.LogWarning("FireAbility reference not set in PlayerAbilityManager!");
    }
}