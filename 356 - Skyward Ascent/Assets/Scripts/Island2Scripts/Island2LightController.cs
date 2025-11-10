using UnityEngine;
using System.Collections;

public class Island2LightingController : MonoBehaviour
{
    [Header("Lighting References")]
    public Light mainDirectionalLight;
    public GameObject island2Fog;

    [Header("Darkness Settings")]
    public float targetLightIntensity = 0.05f;
    public Color darkLightColor = new Color(0.1f, 0.1f, 0.3f);
    public Color darkFogColor = Color.black;
    public float fogDensity = 0.05f;

    [Header("Transition Settings")]
    public float transitionDuration = 3f;

    private float originalLightIntensity;
    private Color originalLightColor;
    private Color originalFogColor;
    private float originalFogDensity;
    private bool isTransitioning = false;

    void Start()
    {
        // Store original lighting settings
        if (mainDirectionalLight != null)
        {
            originalLightIntensity = mainDirectionalLight.intensity;
            originalLightColor = mainDirectionalLight.color;
        }

        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;

        // Ensure fog is enabled
        RenderSettings.fog = true;
    }

    public void ActivateIsland2Darkness()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToDarkness());
        }
    }

    public void RestoreNormalLighting()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToNormal());
        }
    }

    private IEnumerator TransitionToDarkness()
    {
        isTransitioning = true;
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            // Fade directional light
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(originalLightIntensity, targetLightIntensity, progress);
                mainDirectionalLight.color = Color.Lerp(originalLightColor, darkLightColor, progress);
            }

            // Fade fog
            RenderSettings.fogColor = Color.Lerp(originalFogColor, darkFogColor, progress);
            RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, fogDensity, progress);

            yield return null;
        }

        // Ensure final values
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = targetLightIntensity;
            mainDirectionalLight.color = darkLightColor;
        }
        RenderSettings.fogColor = darkFogColor;
        RenderSettings.fogDensity = fogDensity;

        isTransitioning = false;
    }

    private IEnumerator TransitionToNormal()
    {
        isTransitioning = true;
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            // Restore directional light
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(targetLightIntensity, originalLightIntensity, progress);
                mainDirectionalLight.color = Color.Lerp(darkLightColor, originalLightColor, progress);
            }

            // Restore fog
            RenderSettings.fogColor = Color.Lerp(darkFogColor, originalFogColor, progress);
            RenderSettings.fogDensity = Mathf.Lerp(fogDensity, originalFogDensity, progress);

            yield return null;
        }

        // Restore original values
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = originalLightIntensity;
            mainDirectionalLight.color = originalLightColor;
        }
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;

        isTransitioning = false;
    }
}