using UnityEngine;

public class FireAbility : MonoBehaviour
{
    [Header("Fire Ability Settings")]
    public KeyCode fireKey = KeyCode.F;
    public GameObject fireProjectilePrefab;
    public Transform shootPoint;
    public float fireCooldown = 1f;

    [Header("UI")]
    public GameObject fireAbilityUI;

    private bool isAbilityUnlocked = false;
    private bool isOnCooldown = false;

    void Start()
    {
        if (fireAbilityUI != null)
            fireAbilityUI.SetActive(false);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        if (Input.GetKeyDown(fireKey) && !isOnCooldown)
        {
            ShootFire();
        }
    }

    public void UnlockFireAbility()
    {
        isAbilityUnlocked = true;

        if (fireAbilityUI != null)
            fireAbilityUI.SetActive(true);

        Debug.Log("Fire ability unlocked! Press F to shoot fire.");
    }

    private void ShootFire()
    {
        if (fireProjectilePrefab != null && shootPoint != null)
        {
            Instantiate(fireProjectilePrefab, shootPoint.position, shootPoint.rotation);
            StartCooldown();
            Debug.Log("Fire projectile launched!");
        }
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        Invoke(nameof(ResetCooldown), fireCooldown);
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
    }
}