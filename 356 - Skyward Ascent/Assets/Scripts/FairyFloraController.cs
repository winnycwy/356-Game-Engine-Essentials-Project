using UnityEngine;

public class FairyFloraController : MonoBehaviour
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

        // Check if all petals are collected
        if (sunPetalsCollected >= requiredSunPetals && heartbloomTree != null)
        {
            ActivateHeartbloomTree();
        }
    }

    void ActivateHeartbloomTree()
    {
        if (heartbloomTree != null)
        {
            heartbloomTree.ActivateTree();
            Debug.Log("The Heartbloom Tree is now glowing with magical energy!");
        }
    }
}