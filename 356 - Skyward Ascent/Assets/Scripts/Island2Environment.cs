
using UnityEngine;
public class Island2Environment : MonoBehaviour
{
    [Header("Lighting Settings")]
    public Light mainDirectionalLight;
    public float targetIntensity = 0.3f;
    public Color targetLightColor = Color.blue;
    public float transitionSpeed = 2f;

    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.1f, 0.1f, 0.2f);
    public float fogDensity = 0.05f;

    private float originalIntensity;
    private Color originalLightColor;
    private bool originalFogState;
    private Color originalFogColor;
    private float originalFogDensity;

    private bool playerInIsland2 = false;

    private void Start()
    {
        // Store original lighting settings
        if (mainDirectionalLight != null)
        {
            originalIntensity = mainDirectionalLight.intensity;
            originalLightColor = mainDirectionalLight.color;
        }

        originalFogState = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInIsland2 = true;
            EnterSpookyLighting();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInIsland2 = false;
            ExitSpookyLighting();
        }
    }

    private void EnterSpookyLighting()
    {
        // Enable fog for spooky atmosphere
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
    }

    private void ExitSpookyLighting()
    {
        // Restore original fog settings
        RenderSettings.fog = originalFogState;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
    }

    private void Update()
    {
        if (mainDirectionalLight == null) return;

        if (playerInIsland2)
        {
            // Smoothly transition to spooky lighting
            mainDirectionalLight.intensity = Mathf.Lerp(mainDirectionalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
            mainDirectionalLight.color = Color.Lerp(mainDirectionalLight.color, targetLightColor, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // Smoothly transition back to normal lighting
            mainDirectionalLight.intensity = Mathf.Lerp(mainDirectionalLight.intensity, originalIntensity, Time.deltaTime * transitionSpeed);
            mainDirectionalLight.color = Color.Lerp(mainDirectionalLight.color, originalLightColor, Time.deltaTime * transitionSpeed);
        }
    }
}
