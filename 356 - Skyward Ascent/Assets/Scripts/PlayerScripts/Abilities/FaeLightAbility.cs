using UnityEngine;
using System.Collections;

public class FaeLightAbility : MonoBehaviour
{
    [Header("Fae Light Settings")]
    public GameObject faeLightPrefab;          // The ball of light GameObject
    public Transform lightSpawnPoint;          // Where the light spawns (e.g., player's hand or front)
    public KeyCode activateKey = KeyCode.Q;    // Key to hold for Fae Light
    public float maxLightDistance = 3f;        // How far the light can float from player
    public float lightFollowSpeed = 2f;        // How quickly light follows player

    [Header("UI Settings")]
    public GameObject faeLightUI;              // The UI icon in bottom right

    private GameObject currentFaeLight;
    private bool isAbilityUnlocked = false;
    private bool isLightActive = false;

    void Start()
    {
        // Hide UI initially
        if (faeLightUI != null)
            faeLightUI.SetActive(false);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Hold Q to activate/deactivate Fae Light
        if (Input.GetKeyDown(activateKey))
        {
            ToggleFaeLight();
        }

        // Update light position if active
        if (isLightActive && currentFaeLight != null)
        {
            UpdateLightPosition();
        }
    }

    public void UnlockFaeLight()
    {
        isAbilityUnlocked = true;

        // Show UI icon
        if (faeLightUI != null)
            faeLightUI.SetActive(true);

        Debug.Log("Fae Light ability unlocked! Hold Q to activate.");
    }

    private void ToggleFaeLight()
    {
        if (!isLightActive)
        {
            ActivateFaeLight();
        }
        else
        {
            DeactivateFaeLight();
        }
    }

    private void ActivateFaeLight()
    {
        if (faeLightPrefab != null && lightSpawnPoint != null)
        {
            currentFaeLight = Instantiate(faeLightPrefab, lightSpawnPoint.position, Quaternion.identity);
            isLightActive = true;
            Debug.Log("Fae Light activated!");
        }
    }

    private void DeactivateFaeLight()
    {
        if (currentFaeLight != null)
        {
            Destroy(currentFaeLight);
            isLightActive = false;
            Debug.Log("Fae Light deactivated!");
        }
    }

    private void UpdateLightPosition()
    {
        // Make the light float around the player
        Vector3 targetPosition = lightSpawnPoint.position +
            new Vector3(Mathf.Sin(Time.time) * maxLightDistance,
                       Mathf.Cos(Time.time) * 0.5f,
                       Mathf.Cos(Time.time) * maxLightDistance);

        currentFaeLight.transform.position = Vector3.Lerp(
            currentFaeLight.transform.position,
            targetPosition,
            Time.deltaTime * lightFollowSpeed
        );
    }

    // Public method to check if light is active (for puzzles)
    public bool IsLightActive()
    {
        return isLightActive && currentFaeLight != null;
    }

    // Get the current light instance (for other scripts to reference)
    public GameObject GetCurrentLight()
    {
        return currentFaeLight;
    }
}