/*DRAFT 1 - does not account for  faelight ability
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class Island2DarknessTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public BoxCollider triggerCollider;

    [Header("Lighting Control")]
    public Light mainDirectionalLight;

    [Header("Post Processing")]
    public PostProcessVolume darknessVolume;

    [Header("Wizard Cutscene")]
    public GameObject wizardSpirit;
    public DialogueSystem dialogueSystem;
    public Animator wizardAnimator;

    [Header("Darkness Settings")]
    public float pitchBlackLightIntensity = 0f;
    public float heavyFogDensity = 0.15f;
    public float transitionDuration = 3f;

    [Header("Wizard Dialogue")]
    [TextArea]
    public string[] wizardDialogue = {
        "The Sylvan Canopy... even darker than I remembered.",
        "These woods hold secrets in their shadows, Staff-Bearer.",
        "I sense a familiar spark here - a flame that refused to be extinguished.",
        "Seek the guardian of these woods. Its fire may ignite a new power within your staff.",
        "But be warned... some paths require both light and warmth to navigate safely.",
        "Trust in the flames you carry, both old and new."
    };

    private float originalLightIntensity;
    private Color originalLightColor;
    private Color originalFogColor;
    private float originalFogDensity;
    private bool hasTriggered = false;
    private FaeLightAbility playerFaeLight;

    void Start()
    {
        // Store original lighting (Island 1 settings)
        if (mainDirectionalLight != null)
        {
            originalLightIntensity = mainDirectionalLight.intensity;
            originalLightColor = mainDirectionalLight.color;
        }

        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        RenderSettings.fog = true;

        // Ensure darkness volume starts disabled
        if (darknessVolume != null)
        {
            darknessVolume.weight = 0f;
        }

        // Setup trigger
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        // Find player's Fae Light ability
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerFaeLight = player.GetComponent<FaeLightAbility>();
        }
    }

    void Update()
    {
        // Adjust vision based on Fae Light when in Island 2
        if (hasTriggered && playerFaeLight != null)
        {
            UpdatePlayerVision();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(Island2Sequence());
        }
    }

    private IEnumerator Island2Sequence()
    {
        Debug.Log("Player entered Island 2 - Starting darkness sequence");

        // Phase 1: Transition to darkness
        yield return StartCoroutine(TransitionToDarkness());

        // Phase 2: Wizard cutscene
        yield return StartCoroutine(PlayWizardCutscene());

        Debug.Log("Island 2 is now active! Use Fae Light (Q) to navigate.");
    }

    private IEnumerator TransitionToDarkness()
    {
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            // Fade directional light to black
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(originalLightIntensity, pitchBlackLightIntensity, progress);
                mainDirectionalLight.color = Color.Lerp(originalLightColor, Color.black, progress);
            }

            // Increase fog for limited vision
            RenderSettings.fogColor = Color.Lerp(originalFogColor, Color.black, progress);
            RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, heavyFogDensity, progress);

            // Fade in post-processing
            if (darknessVolume != null)
            {
                darknessVolume.weight = progress;
            }

            yield return null;
        }

        // Ensure final darkness state
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = pitchBlackLightIntensity;
        }
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = heavyFogDensity;

        if (darknessVolume != null)
        {
            darknessVolume.weight = 1f;
        }
    }

    private IEnumerator PlayWizardCutscene()
    {
        if (wizardSpirit != null)
            wizardSpirit.SetActive(true);

        if (wizardAnimator != null)
        {
            wizardAnimator.SetTrigger("Appear");
            wizardAnimator.SetTrigger("StartTalking"); // ADD THIS LINE
        }

        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(wizardDialogue, "Aetherius");

            while (dialogueSystem.IsDialogueActive)
            {
                yield return null;
            }

            if (wizardAnimator != null)
                wizardAnimator.SetTrigger("Disappear");

            yield return new WaitForSeconds(1f);

            if (wizardSpirit != null)
                wizardSpirit.SetActive(false);
        }
    }

    private void UpdatePlayerVision()
    {
        if (playerFaeLight.IsLightActive())
        {
            // Fae Light is ON - better visibility
            RenderSettings.fogDensity = heavyFogDensity * 0.3f;
        }
        else
        {
            // Fae Light is OFF - poor visibility
            RenderSettings.fogDensity = heavyFogDensity;
        }
    }

    // Call this if player leaves Island 2 (optional)
    public void RestoreIsland1Lighting()
    {
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = originalLightIntensity;
            mainDirectionalLight.color = originalLightColor;
        }

        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;

        if (darknessVolume != null)
        {
            darknessVolume.weight = 0f;
        }

        hasTriggered = false;
    }

    // For debugging in Editor
    [ContextMenu("Test Darkness Transition")]
    public void TestDarknessTransition()
    {
        if (!hasTriggered)
        {
            StartCoroutine(TransitionToDarkness());
        }
    }

    [ContextMenu("Restore Normal Lighting")]
    public void TestRestoreLighting()
    {
        RestoreIsland1Lighting();
    }
}
*/
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class Island2DarknessTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public BoxCollider triggerCollider;

    [Header("Lighting Control")]
    public Light mainDirectionalLight;

    [Header("Post Processing")]
    public PostProcessVolume darknessVolume;

    [Header("Wizard Cutscene")]
    public GameObject wizardSpirit;
    public DialogueSystem dialogueSystem;
    public Animator wizardAnimator;

    [Header("Darkness Settings")]
    public float pitchBlackLightIntensity = 0f;
    public float heavyFogDensity = 0.4f;
    public float transitionDuration = 3f;

    [Header("Fae Light Vision Settings")]
    public float activeVignetteIntensity = 0.1f;
    public float inactiveVignetteIntensity = 0.6f;
    public float activeExposure = -1f;
    public float inactiveExposure = -5f;
    public float activeFogDensity = 0.05f;
    public float effectTransitionSpeed = 2f;

    [Header("Wizard Dialogue")]
    [TextArea]
    public string[] wizardDialogue = {
        "The Sylvan Canopy... even darker than I remembered.",
        "These woods hold secrets in their shadows, Staff-Bearer.",
        "I sense a familiar spark here - a flame that refused to be extinguished.",
        "Seek the guardian of these woods. Its fire may ignite a new power within your staff.",
        "But be warned... some paths require both light and warmth to navigate safely.",
        "Trust in the flames you carry, both old and new."
    };

    private Vignette vignette;
    private ColorGrading colorGrading;
    private float originalLightIntensity;
    private Color originalLightColor;
    private Color originalFogColor;
    private float originalFogDensity;
    private bool hasTriggered = false;
    private FaeLightAbility playerFaeLight;
    private bool isInIsland2 = false;

    private float targetVignetteIntensity;
    private float targetExposure;
    private float targetFogDensity;

    // For local volume control
    private bool isVolumeEnabled = false;
    private Collider volumeCollider;

    void Start()
    {
        Debug.Log("Island2DarknessTrigger - Start called");

        // Get post-processing effects
        if (darknessVolume != null && darknessVolume.profile != null)
        {
            bool hasVignette = darknessVolume.profile.TryGetSettings(out vignette);
            bool hasColorGrading = darknessVolume.profile.TryGetSettings(out colorGrading);

            Debug.Log($"Post-processing: Vignette={hasVignette}, ColorGrading={hasColorGrading}");

            // Get the volume's collider for local volume
            volumeCollider = darknessVolume.GetComponent<Collider>();
            if (volumeCollider != null)
            {
                Debug.Log($"Volume collider found: {volumeCollider.GetType().Name}");
            }

            // Initially disable the local volume
            darknessVolume.enabled = false;
            if (volumeCollider != null)
            {
                volumeCollider.enabled = false;
            }
        }
        else
        {
            Debug.LogError("Darkness Volume or Profile is missing!");
        }

        // Store original lighting
        if (mainDirectionalLight != null)
        {
            originalLightIntensity = mainDirectionalLight.intensity;
            originalLightColor = mainDirectionalLight.color;
            Debug.Log($"Original light intensity: {originalLightIntensity}");
        }

        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        RenderSettings.fog = true;

        // Setup trigger - make sure it's on a separate GameObject from Fae Light
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        // Find player's Fae Light ability
        FindPlayerFaeLight();

        // Set initial targets
        targetVignetteIntensity = inactiveVignetteIntensity;
        targetExposure = inactiveExposure;
        targetFogDensity = heavyFogDensity;
    }

    void Update()
    {
        // Only adjust effects when player is in Island 2
        if (isInIsland2)
        {
            UpdateFaeLightEffects();
            SmoothTransitionEffects();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.name} (Tag: {other.tag})");

        // ONLY trigger for the Player, ignore FaeLight projectiles and other objects
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            isInIsland2 = true;
            Debug.Log("Player entered Island 2 - Starting darkness sequence");
            StartCoroutine(Island2Sequence());
        }
        else if (other.name.Contains("FaeLight"))
        {
            Debug.Log("Ignoring FaeLight projectile - this is not the player");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // ONLY respond to Player exiting
        if (other.CompareTag("Player") && isInIsland2)
        {
            Debug.Log("Player left Island 2");
            isInIsland2 = false;
            RestoreIsland1Lighting();
        }
    }

    private void FindPlayerFaeLight()
    {
        if (playerFaeLight == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerFaeLight = player.GetComponent<FaeLightAbility>();
                if (playerFaeLight != null)
                {
                    Debug.Log("FaeLightAbility found on player!");
                }
                else
                {
                    Debug.LogError("FaeLightAbility component not found on Player! Checking children...");
                    playerFaeLight = player.GetComponentInChildren<FaeLightAbility>();
                    if (playerFaeLight != null)
                    {
                        Debug.Log("FaeLightAbility found in player children!");
                    }
                }
            }
        }
    }

    private IEnumerator Island2Sequence()
    {
        Debug.Log("Starting Island 2 darkness sequence");

        // Phase 1: Transition to darkness
        yield return StartCoroutine(TransitionToDarkness());

        // Phase 2: Wizard cutscene
        yield return StartCoroutine(PlayWizardCutscene());

        Debug.Log("Island 2 is now active! Use Fae Light (Q) to see clearly.");
    }

    private IEnumerator TransitionToDarkness()
    {
        // Enable the local volume
        if (darknessVolume != null)
        {
            darknessVolume.enabled = true;
            if (volumeCollider != null)
            {
                volumeCollider.enabled = true;
            }
            isVolumeEnabled = true;
            Debug.Log("Local Post-Processing Volume enabled");
        }

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            // Fade directional light to black
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(originalLightIntensity, pitchBlackLightIntensity, progress);
                mainDirectionalLight.color = Color.Lerp(originalLightColor, Color.black, progress);
            }

            // Increase fog for limited vision
            RenderSettings.fogColor = Color.Lerp(originalFogColor, Color.black, progress);
            RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, heavyFogDensity, progress);

            yield return null;
        }

        // Ensure final darkness state
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = pitchBlackLightIntensity;
        }
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = heavyFogDensity;

        // Set initial darkness effects
        if (vignette != null)
        {
            vignette.intensity.value = inactiveVignetteIntensity;
            Debug.Log($"Set vignette to: {vignette.intensity.value}");
        }
        if (colorGrading != null)
        {
            colorGrading.postExposure.value = inactiveExposure;
            Debug.Log($"Set exposure to: {colorGrading.postExposure.value}");
        }
    }

    private IEnumerator PlayWizardCutscene()
    {
        // Skip cutscene for testing - comment this out when you want the cutscene
        if (wizardSpirit == null || dialogueSystem == null)
        {
            Debug.Log("Skipping cutscene - components not set up");
            yield break;
        }

        if (wizardSpirit != null)
            wizardSpirit.SetActive(true);

        if (wizardAnimator != null)
        {
            wizardAnimator.SetTrigger("Appear");
            wizardAnimator.SetTrigger("StartTalking");
        }

        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(wizardDialogue, "Aetherius");

            while (dialogueSystem.IsDialogueActive)
            {
                yield return null;
            }

            if (wizardAnimator != null)
                wizardAnimator.SetTrigger("Disappear");

            yield return new WaitForSeconds(1f);

            if (wizardSpirit != null)
                wizardSpirit.SetActive(false);
        }
    }

    private void UpdateFaeLightEffects()
    {
        if (playerFaeLight == null)
        {
            FindPlayerFaeLight();
            if (playerFaeLight == null)
            {
                Debug.LogWarning("FaeLightAbility still not found!");
                return;
            }
        }

        bool isFaeLightActive = playerFaeLight.IsLightActive();

        // Set target values based on Fae Light state
        if (isFaeLightActive)
        {
            // FAE LIGHT ACTIVE - Clear vision
            targetVignetteIntensity = activeVignetteIntensity;
            targetExposure = activeExposure;
            targetFogDensity = activeFogDensity;
        }
        else
        {
            // FAE LIGHT INACTIVE - Dark vision
            targetVignetteIntensity = inactiveVignetteIntensity;
            targetExposure = inactiveExposure;
            targetFogDensity = heavyFogDensity;
        }
    }

    private void SmoothTransitionEffects()
    {
        // Only apply effects if the volume is enabled and we're in Island 2
        if (!isVolumeEnabled || !isInIsland2) return;

        // Smoothly transition vignette
        if (vignette != null)
        {
            float currentVignette = vignette.intensity.value;
            vignette.intensity.value = Mathf.Lerp(currentVignette, targetVignetteIntensity, Time.deltaTime * effectTransitionSpeed);
        }

        // Smoothly transition exposure
        if (colorGrading != null)
        {
            float currentExposure = colorGrading.postExposure.value;
            colorGrading.postExposure.value = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * effectTransitionSpeed);
        }

        // Smoothly transition fog density
        float currentFog = RenderSettings.fogDensity;
        RenderSettings.fogDensity = Mathf.Lerp(currentFog, targetFogDensity, Time.deltaTime * effectTransitionSpeed);
    }

    private void RestoreIsland1Lighting()
    {
        // Disable the local volume
        if (darknessVolume != null)
        {
            darknessVolume.enabled = false;
            if (volumeCollider != null)
            {
                volumeCollider.enabled = false;
            }
            isVolumeEnabled = false;
            Debug.Log("Local Post-Processing Volume disabled");
        }

        // Restore lighting
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = originalLightIntensity;
            mainDirectionalLight.color = originalLightColor;
        }

        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;

        Debug.Log("Player left Island 2 - Lighting restored to normal");
    }

    // Manual trigger for testing - call this from another script if needed
    public void ForceEnterIsland2()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            isInIsland2 = true;
            Debug.Log("Manually forcing Island 2 entry");
            StartCoroutine(TransitionToDarkness());
        }
    }

    [ContextMenu("Test Fae Light ON")]
    public void TestFaeLightOn()
    {
        if (vignette != null && colorGrading != null)
        {
            vignette.intensity.value = activeVignetteIntensity;
            colorGrading.postExposure.value = activeExposure;
            RenderSettings.fogDensity = activeFogDensity;
            Debug.Log($"TEST: Vignette={vignette.intensity.value}, Exposure={colorGrading.postExposure.value}");
        }
    }

    [ContextMenu("Test Fae Light OFF")]
    public void TestFaeLightOff()
    {
        if (vignette != null && colorGrading != null)
        {
            vignette.intensity.value = inactiveVignetteIntensity;
            colorGrading.postExposure.value = inactiveExposure;
            RenderSettings.fogDensity = heavyFogDensity;
            Debug.Log($"TEST: Vignette={vignette.intensity.value}, Exposure={colorGrading.postExposure.value}");
        }
    }

    [ContextMenu("Force Island 2 Entry")]
    public void DebugForceIsland2()
    {
        ForceEnterIsland2();
    }
}