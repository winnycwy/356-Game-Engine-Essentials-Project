using UnityEngine;
using System.Collections;

public class HiddenCrystal : MonoBehaviour
{
    [Header("Crystal Settings")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 2.0f;
    public float glowIntensity = 2.0f;

    [Header("Fae Light Detection")]
    public float detectionRange = 5.0f;
    public LayerMask faeLightLayer = 1;

    [Header("Collection Settings")]
    public bool isCollectible = true;
    public int crystalValue = 1;
    public float collectionDelay = 0.5f; // Delay before collection after appearing

    // Particle effects
    public ParticleSystem collectParticles;
    public GameObject crystalModel; // Reference to the visual part

    private Renderer crystalRenderer;
    private Light glowLight;
    private Material crystalMaterial;
    private Color originalColor;
    private bool isVisible = false;
    private bool isFading = false;
    private bool isCollected = false;

    // Audio
    private AudioSource audioSource;
    public AudioClip appearSound;
    public AudioClip disappearSound;
    public AudioClip collectSound;

    void Start()
    {
        // Get components
        crystalRenderer = GetComponent<Renderer>();
        glowLight = GetComponentInChildren<Light>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }

        // If no specific crystal model reference, use this object's renderer
        if (crystalModel == null && crystalRenderer != null)
        {
            crystalModel = crystalRenderer.gameObject;
        }

        // Store original material properties
        if (crystalRenderer != null)
        {
            crystalMaterial = crystalRenderer.material;
            originalColor = crystalMaterial.color;

            // Start invisible
            SetCrystalAlpha(0f);
        }

        // Start with light off
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
        }

        // Disable collider initially if collectible
        if (isCollectible)
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    void Update()
    {
        if (isCollected) return;

        // Check for Fae Light in range
        bool faeLightInRange = CheckForFaeLight();

        if (faeLightInRange && !isVisible && !isFading)
        {
            // Fae Light is shining on crystal - make it appear
            StartCoroutine(FadeInCrystal());
        }
        else if (!faeLightInRange && isVisible && !isFading && !isCollected)
        {
            // Fae Light moved away - make it disappear
            StartCoroutine(FadeOutCrystal());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") && isVisible && isCollectible)
        {
            CollectCrystal();
        }
    }

    private bool CheckForFaeLight()
    {
        // Check for FaeLight objects by tag
        GameObject[] faeLights = GameObject.FindGameObjectsWithTag("FaeLight");
        foreach (GameObject faeLight in faeLights)
        {
            if (faeLight.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, faeLight.transform.position);
                if (distance <= detectionRange)
                {
                    return true;
                }
            }
        }

        // Additional check for lights
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.name.Contains("FaeLight") && light.enabled && light.intensity > 0.1f)
            {
                float distance = Vector3.Distance(transform.position, light.transform.position);
                if (distance <= detectionRange)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator FadeInCrystal()
    {
        isFading = true;

        // Play appear sound
        if (appearSound != null)
        {
            audioSource.PlayOneShot(appearSound);
        }

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;

            // Fade in alpha
            SetCrystalAlpha(progress);

            // Fade in light
            if (glowLight != null)
            {
                glowLight.intensity = progress * glowIntensity;
            }

            yield return null;
        }

        // Ensure final state
        SetCrystalAlpha(1f);
        if (glowLight != null)
        {
            glowLight.intensity = glowIntensity;
        }

        isVisible = true;
        isFading = false;

        // Enable collider for collection after a short delay
        if (isCollectible)
        {
            yield return new WaitForSeconds(collectionDelay);
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }

        Debug.Log("Crystal fully appeared and ready for collection!");
    }

    private IEnumerator FadeOutCrystal()
    {
        isFading = true;

        // Disable collider immediately
        if (isCollectible)
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        // Play disappear sound
        if (disappearSound != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }

        float timer = 0f;
        float startAlpha = crystalMaterial.color.a;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            float alpha = Mathf.Lerp(startAlpha, 0f, progress);

            // Fade out alpha
            SetCrystalAlpha(alpha);

            // Fade out light
            if (glowLight != null)
            {
                glowLight.intensity = Mathf.Lerp(glowIntensity, 0f, progress);
            }

            yield return null;
        }

        // Ensure final state
        SetCrystalAlpha(0f);
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
        }

        isVisible = false;
        isFading = false;

        Debug.Log("Crystal fully disappeared!");
    }

    private void CollectCrystal()
    {
        if (isCollected) return;

        isCollected = true;

        // Play collect sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Add to crystal manager
        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.AddCrystal(crystalValue);
        }
        else
        {
            Debug.LogWarning("CrystalManager instance not found!");
        }

        // Play collection effects
        StartCoroutine(PlayCollectionEffects());

        Debug.Log("Crystal collected!");
    }

    private IEnumerator PlayCollectionEffects()
    {
        // Hide the crystal model
        if (crystalModel != null)
        {
            crystalModel.SetActive(false);
        }

        // Disable collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Disable light
        if (glowLight != null)
        {
            glowLight.enabled = false;
        }

        // Play particle effects
        if (collectParticles != null)
        {
            collectParticles.Play();
            yield return new WaitForSeconds(collectParticles.main.duration);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Destroy or keep for respawn
        Destroy(gameObject);
    }

    private void SetCrystalAlpha(float alpha)
    {
        if (crystalMaterial != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            crystalMaterial.color = newColor;
        }
    }

    // Visualize detection range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Show collection range if collectible
        if (isCollectible)
        {
            Gizmos.color = Color.green;
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.DrawWireCube(transform.position, collider.bounds.size);
            }
        }
    }

    // Public methods for debugging
    [ContextMenu("Test Appear")]
    public void TestAppear()
    {
        if (!isVisible && !isCollected)
        {
            StartCoroutine(FadeInCrystal());
        }
    }

    [ContextMenu("Test Disappear")]
    public void TestDisappear()
    {
        if (isVisible && !isCollected)
        {
            StartCoroutine(FadeOutCrystal());
        }
    }

    [ContextMenu("Test Collect")]
    public void TestCollect()
    {
        if (isVisible && !isCollected)
        {
            CollectCrystal();
        }
    }
}