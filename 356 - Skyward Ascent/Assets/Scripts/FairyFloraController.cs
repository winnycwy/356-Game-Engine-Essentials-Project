using UnityEngine;

public class FairyPipController : MonoBehaviour
{
    [Header("Tree Reference")]
    public HeartbloomTreeController heartbloomTree;

    [Header("Fairy Properties")]
    public int sunPetalsCollected = 0;
    public int requiredSunPetals = 3;

    public void CollectSunPetal()
    {
        sunPetalsCollected++;
        Debug.Log($"Sun Petal collected! {sunPetalsCollected}/{requiredSunPetals}");

        if (sunPetalsCollected >= requiredSunPetals)
        {
            // Trigger the cinematic event
            GameEventManager.Instance.AllPetalsCollected();
        }
    }
}