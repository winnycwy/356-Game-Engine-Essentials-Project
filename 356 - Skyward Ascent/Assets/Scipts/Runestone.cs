using UnityEngine;
using System.Collections;

public class Runestone : MonoBehaviour
{
    [Header("Glow Settings")]
    public Material normalMaterial;       // Material 1 stays unchanged
    public Material glowingMaterial;      // Material 2 will change to this
    public Light glowLight;
    public float glowIntensity = 2f;
    public float glowDuration = 1.5f;

    [Header("Audio")]
    public AudioClip activationSound;

    private bool isActivated = false;
    private Renderer stoneRenderer;
    private AudioSource audioSource;
    private Material glowMaterialInstance; // Instance of glowing material

    void Start()
    {
        stoneRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        // Ensure materials are set correctly
        if (stoneRenderer != null && normalMaterial != null)
        {
            Material[] mats = stoneRenderer.materials;
            mats[0] = normalMaterial;  // keep material 1 as normal
            stoneRenderer.materials = mats;
        }

        if (glowLight != null)
        {
            glowLight.intensity = 0f;
            glowLight.enabled = false;
        }

        // Create instance of glowing material for material 2
        if (glowingMaterial != null)
            glowMaterialInstance = new Material(glowingMaterial);
    }

    // Called externally (player presses E)
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
            audioSource.PlayOneShot(activationSound);

        if (glowLight != null)
            glowLight.enabled = true;

        float elapsedTime = 0f;

        while (elapsedTime < glowDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / glowDuration;

            // Fade light
            if (glowLight != null)
                glowLight.intensity = Mathf.Lerp(0f, glowIntensity, progress);

            // Lerp only material 2
            if (stoneRenderer != null && glowMaterialInstance != null)
            {
                Material[] mats = stoneRenderer.materials;
                mats[1].Lerp(normalMaterial, glowMaterialInstance, progress); // change only material 2
                stoneRenderer.materials = mats;
            }

            yield return null;
        }

        // Ensure final state
        if (glowLight != null)
            glowLight.intensity = glowIntensity;

        if (stoneRenderer != null && glowMaterialInstance != null)
        {
            Material[] mats = stoneRenderer.materials;
            mats[1] = glowMaterialInstance; // final glowing material to material 2
            stoneRenderer.materials = mats;
        }

        // Notify manager
        RunestoneManager manager = FindObjectOfType<RunestoneManager>();
        if (manager != null)
            manager.OnRunestoneActivated(this);
    }

    public bool IsActivated()
    {
        return isActivated;
    }
}