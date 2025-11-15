using UnityEngine;
using System.Collections.Generic;

public class HealingItemManager : MonoBehaviour
{
    public static HealingItemManager Instance;

    private List<GameObject> healingItems = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterHealingItem(GameObject healingItem)
    {
        if (!healingItems.Contains(healingItem))
        {
            healingItems.Add(healingItem);
        }
    }

    public void ResetAllHealingItems()
    {
        Debug.Log($"Resetting {healingItems.Count} healing items");

        foreach (GameObject healingItem in healingItems)
        {
            if (healingItem != null)
            {
                healingItem.SetActive(true);
            }
        }
    }
}