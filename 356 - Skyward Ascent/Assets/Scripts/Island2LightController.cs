using UnityEngine;

public class Island2LightController : MonoBehaviour
{
    [Header("Light Settings")]
    public GameObject mainDirectionalLight; // Drag your main sun light here
    public GameObject island2AmbientLight; // Optional: specific light for Island 2

    [Header("Fog Settings")]
    public Color island2FogColor = Color.black;
    public float island2FogDensity = 0.05f;

    private Color originalFogColor;
    private float originalFogDensity;
    private bool isActive = false;

    private void Start()
    {
        // Store original fog settings
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive && other.CompareTag("Player"))
        {
            ActivateIsland2Environment();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            DeactivateIsland2Environment();
        }
    }

    private void ActivateIsland2Environment()
    {
        isActive = true;

        // Disable main sunlight
        if (mainDirectionalLight != null)
            mainDirectionalLight.SetActive(false);

        // Enable Island 2 specific lighting
        if (island2AmbientLight != null)
            island2AmbientLight.SetActive(true);

        // Change fog settings for dark atmosphere
        RenderSettings.fogColor = island2FogColor;
        RenderSettings.fogDensity = island2FogDensity;

        Debug.Log("Island 2 dark environment activated");
    }

    private void DeactivateIsland2Environment()
    {
        isActive = false;

        // Restore main sunlight
        if (mainDirectionalLight != null)
            mainDirectionalLight.SetActive(true);

        // Disable Island 2 specific lighting
        if (island2AmbientLight != null)
            island2AmbientLight.SetActive(false);

        // Restore original fog settings
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;

        Debug.Log("Island 2 dark environment deactivated");
    }
}