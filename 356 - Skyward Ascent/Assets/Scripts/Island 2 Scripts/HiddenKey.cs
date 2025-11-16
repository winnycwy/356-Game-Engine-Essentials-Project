using UnityEngine;
using System.Collections;

public class HiddenKey : MonoBehaviour
{
    [Header("Key Settings")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 2.0f;
    public float glowIntensity = 2.0f;

    [Header("Fae Light Detection")]
    public float detectionRange = 5.0f;
    public LayerMask faeLightLayer = 1;

    [Header("Collection Settings")]
    public float collectionDelay = 0.5f;

    // References
    private Renderer keyRenderer;
    private Light glowLight;
    private Material keyMaterial;
    private Color originalColor;
    private bool isVisible = false;
    private bool isFading = false;
    private bool isCollected = false;

    // Audio
    private AudioSource audioSource;
    public AudioClip appearSound;
    public AudioClip disappearSound;
    public AudioClip collectSound;

    // Event for when key is collected
    public System.Action<HiddenKey> OnKeyCollected;

    void Start()
    {
        // Get components
        keyRenderer = GetComponent<Renderer>();
        glowLight = GetComponentInChildren<Light>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }

        // Store original material properties
        if (keyRenderer != null)
        {
            keyMaterial = keyRenderer.material;
            originalColor = keyMaterial.color;

            // Set key to completely transparent at start
            Color transparentColor = originalColor;
            transparentColor.a = 0f;
            keyMaterial.color = transparentColor;
        }

        // Start with light off
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
        }

        // Disable collider initially
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        isVisible = false;
        isFading = false;
        isCollected = false;
    }

    void Update()
    {
        if (isCollected) return;

        // Check for Fae Light in range
        bool faeLightInRange = CheckForFaeLight();

        if (faeLightInRange && !isVisible && !isFading)
        {
            // Fae Light is shining on key - make it appear
            StartCoroutine(FadeInKey());
        }
        else if (!faeLightInRange && isVisible && !isFading && !isCollected)
        {
            // Fae Light moved away - make it disappear
            StartCoroutine(FadeOutKey());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") && isVisible)
        {
            CollectKey();
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

    private IEnumerator FadeInKey()
    {
        isFading = true;

        // Enable the renderer at the start of fade in
        if (keyRenderer != null)
        {
            keyRenderer.enabled = true;
        }

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
            SetKeyAlpha(progress);

            // Fade in light
            if (glowLight != null)
            {
                glowLight.intensity = progress * glowIntensity;
            }

            yield return null;
        }

        // Ensure final state
        SetKeyAlpha(1f);
        if (glowLight != null)
        {
            glowLight.intensity = glowIntensity;
        }

        isVisible = true;
        isFading = false;

        // Enable collider for collection after a short delay
        yield return new WaitForSeconds(collectionDelay);
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    private IEnumerator FadeOutKey()
    {
        isFading = true;

        // Disable collider immediately
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Play disappear sound
        if (disappearSound != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }

        float timer = 0f;
        float startAlpha = keyMaterial.color.a;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            float alpha = Mathf.Lerp(startAlpha, 0f, progress);

            // Fade out alpha
            SetKeyAlpha(alpha);

            // Fade out light
            if (glowLight != null)
            {
                glowLight.intensity = Mathf.Lerp(glowIntensity, 0f, progress);
            }

            yield return null;
        }

        // Ensure final state
        SetKeyAlpha(0f);
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
        }

        isVisible = false;
        isFading = false;
    }

    private void CollectKey()
    {
        if (isCollected) return;

        isCollected = true;

        // Play collect sound
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Notify that key was collected
        OnKeyCollected?.Invoke(this);

        // Hide the key
        if (keyRenderer != null)
        {
            keyRenderer.enabled = false;
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

        Debug.Log("Key collected!");
    }

    private void SetKeyAlpha(float alpha)
    {
        if (keyMaterial != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            keyMaterial.color = newColor;
        }
    }

    // Visualize detection range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public bool IsCollected()
    {
        return isCollected;
    }
}