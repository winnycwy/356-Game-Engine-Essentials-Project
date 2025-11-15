using UnityEngine;
using Ilumisoft.HealthSystem;

public class HealingItem : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] int healingAmount = 25;

    void Start()
    {
        // Register with the manager when created
        if (HealingItemManager.Instance != null)
        {
            HealingItemManager.Instance.RegisterHealingItem(this.gameObject);
            Debug.Log($"Healing item registered: {gameObject.name}");
        }
        else
        {
            Debug.LogError("HealingItemManager instance not found!");
        }
    }

    void Update()
    {
        transform.Rotate(0, 100 * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.CurrentHealth = Mathf.Min(playerHealth.CurrentHealth + healingAmount, playerHealth.MaxHealth);
                Debug.Log($"Healed! HP: {playerHealth.CurrentHealth}");

                // Instead of Destroy(gameObject), disable it
                gameObject.SetActive(false);
            }
        }
    }
}