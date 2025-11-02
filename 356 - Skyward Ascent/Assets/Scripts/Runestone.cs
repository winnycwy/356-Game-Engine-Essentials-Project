using UnityEngine;
using System.Collections;

public class Runestone : MonoBehaviour
{
    [Header("Glow Settings")]
    public Material normalMaterial;
    public Material glowingMaterial;
    public Light glowLight;
    public float glowIntensity = 2f;
    public float glowDuration = 1.5f;

    [Header("Audio")]
    public AudioClip activationSound;

    private bool isActivated = false;
    private Renderer stoneRenderer;
    private AudioSource audioSource;

    void Start()
    {
        stoneRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        // Set initial state
        if (stoneRenderer != null && normalMaterial != null)
        {
            stoneRenderer.material = normalMaterial;
        }

        // Setup glow light
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
            glowLight.enabled = false;
        }
    }

    public void ActivateRunestone()
    {
        if (!isActivated)
        {
            isActivated = true;
            StartCoroutine(GlowSequence());
        }
    }

    private IEnumerator GlowSequence()
    {
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Enable light
        if (glowLight != null)
        {
            glowLight.enabled = true;
        }

        // Smooth glow transition
        float elapsedTime = 0f;

        while (elapsedTime < glowDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / glowDuration;

            // Fade in light
            if (glowLight != null)
            {
                glowLight.intensity = Mathf.Lerp(0f, glowIntensity, progress);
            }

            // Optional: Change material
            if (stoneRenderer != null && glowingMaterial != null)
            {
                stoneRenderer.material.Lerp(normalMaterial, glowingMaterial, progress);
            }

            yield return null;
        }

        // Ensure final state
        if (glowLight != null)
        {
            glowLight.intensity = glowIntensity;
        }

        if (stoneRenderer != null && glowingMaterial != null)
        {
            stoneRenderer.material = glowingMaterial;
        }

        // Notify runestone manager
        RunestoneManager manager = FindObjectOfType<RunestoneManager>();
        if (manager != null)
        {
            manager.OnRunestoneActivated(this);
        }
    }

    public bool IsActivated()
    {
        return isActivated;
    }
}