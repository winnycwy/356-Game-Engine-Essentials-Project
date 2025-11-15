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
        }
        // No need to call AbilityUIManager anymore
        Debug.Log("Fire ability unlocked!");
    }

    // Helper methods to check ability status
    public bool IsFireAbilityUnlocked()
    {
        return fireAbility != null && fireAbility.IsFireAbilityUnlocked();
    }

    public bool IsFaeLightActive()
    {
        return faeLightAbility != null && faeLightAbility.IsLightActive();
    }
}