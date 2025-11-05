/*DRAFT 1
using UnityEngine;
using System.Collections;

public class TeleportToIsland : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportDestination;
    public string playerTag = "Player";

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTeleporting && other.CompareTag(playerTag))
        {
            StartCoroutine(TeleportWithFade(other.gameObject));
        }
    }

    private IEnumerator TeleportWithFade(GameObject player)
    {
        isTeleporting = true;

        // Fade out
        yield return StartCoroutine(FadeScreen(1f)); // Fade to black

        // Teleport player
        player.transform.position = teleportDestination.position;
        player.transform.rotation = teleportDestination.rotation;

        // Fade in
        yield return StartCoroutine(FadeScreen(0f)); // Fade to clear

        isTeleporting = false;
    }

    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
*/
using UnityEngine;
using System.Collections;

public class TeleportToIsland : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportDestination;
    public string playerTag = "Player";
    public bool startDisabled = true; // Portal starts inactive until fairy activates it

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Visual Effects")]
    public ParticleSystem activationParticles;
    public Light portalLight;

    private bool isTeleporting = false;
    private bool isPortalActive = false;

    void Start()
    {
        // If portal should start disabled, disable the collider
        if (startDisabled)
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            isPortalActive = false;

            // Disable visual effects initially
            if (activationParticles != null)
                activationParticles.Stop();

            if (portalLight != null)
                portalLight.enabled = false;
        }
        else
        {
            isPortalActive = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTeleporting && isPortalActive && other.CompareTag(playerTag))
        {
            StartCoroutine(TeleportWithFade(other.gameObject));
        }
    }

    // Call this method to activate the portal (from InteractableCharacter)
    public void ActivatePortal()
    {
        isPortalActive = true;

        // Enable the collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        // Play activation effects
        StartCoroutine(PlayActivationEffects());

        Debug.Log("Heartbloom Portal activated!");
    }

    private IEnumerator PlayActivationEffects()
    {
        // Play particles
        if (activationParticles != null)
            activationParticles.Play();

        // Fade in light
        if (portalLight != null)
        {
            portalLight.enabled = true;
            float originalIntensity = portalLight.intensity;
            portalLight.intensity = 0f;

            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                portalLight.intensity = Mathf.Lerp(0f, originalIntensity, timer);
                yield return null;
            }
        }
    }

    private IEnumerator TeleportWithFade(GameObject player)
    {
        isTeleporting = true;

        // Fade out
        yield return StartCoroutine(FadeScreen(1f));

        // Teleport player
        player.transform.position = teleportDestination.position;
        player.transform.rotation = teleportDestination.rotation;

        // Fade in
        yield return StartCoroutine(FadeScreen(0f));

        isTeleporting = false;
    }

    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}