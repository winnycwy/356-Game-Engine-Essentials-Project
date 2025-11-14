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
    public float heavyFogDensity = 0.15f;
    public float transitionDuration = 3f;

    [Header("Fae Light Vision Settings")]
    public float activeVignetteIntensity = 0.1f;  // Very light vignette when Fae Light is on
    public float inactiveVignetteIntensity = 0.6f; // Strong vignette when Fae Light is off
    public float activeExposure = 0f;             // Normal exposure
    public float inactiveExposure = -2f;          // Dark exposure
    public float activeFogDensity = 0.05f;        // Reduced fog
    public float effectTransitionSpeed = 2f;      // How fast effects transition

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

    // Target values for smooth transitions
    private float targetVignetteIntensity;
    private float targetExposure;
    private float targetFogDensity;

    void Start()
    {
        // Get post-processing effects
        if (darknessVolume != null && darknessVolume.profile != null)
        {
            darknessVolume.profile.TryGetSettings(out vignette);
            darknessVolume.profile.TryGetSettings(out colorGrading);
        }

        // Store original lighting
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
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            isInIsland2 = true;
            StartCoroutine(Island2Sequence());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInIsland2)
        {
            // Player left Island 2 - restore everything
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
                if (playerFaeLight == null)
                {
                    Debug.LogWarning("FaeLightAbility component not found on Player!");
                }
            }
            else
            {
                Debug.LogWarning("Player GameObject with tag 'Player' not found!");
            }
        }
    }

    private IEnumerator Island2Sequence()
    {
        Debug.Log("Player entered Island 2 - Starting darkness sequence");

        // Phase 1: Transition to darkness
        yield return StartCoroutine(TransitionToDarkness());

        // Phase 2: Wizard cutscene
        yield return StartCoroutine(PlayWizardCutscene());

        Debug.Log("Island 2 is now active! Use Fae Light (Q) to see clearly.");
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

        // Set initial darkness effects
        if (vignette != null) vignette.intensity.value = inactiveVignetteIntensity;
        if (colorGrading != null) colorGrading.postExposure.value = inactiveExposure;
    }

    private IEnumerator PlayWizardCutscene()
    {
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
            return;
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
        // Smoothly transition vignette
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * effectTransitionSpeed);
        }

        // Smoothly transition exposure
        if (colorGrading != null)
        {
            colorGrading.postExposure.value = Mathf.Lerp(colorGrading.postExposure.value, targetExposure, Time.deltaTime * effectTransitionSpeed);
        }

        // Smoothly transition fog density
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * effectTransitionSpeed);
    }

    private void RestoreIsland1Lighting()
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

        // Reset post-processing effects to default
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }
        if (colorGrading != null)
        {
            colorGrading.postExposure.value = 0f;
        }

        Debug.Log("Player left Island 2 - Lighting restored to normal");
    }

    // For debugging in Inspector
    [ContextMenu("Force Fae Light Vision")]
    public void ForceFaeLightVision()
    {
        if (vignette != null && colorGrading != null)
        {
            vignette.intensity.value = activeVignetteIntensity;
            colorGrading.postExposure.value = activeExposure;
            RenderSettings.fogDensity = activeFogDensity;
            Debug.Log("Forced Fae Light vision effects");
        }
    }

    [ContextMenu("Force Dark Vision")]
    public void ForceDarkVision()
    {
        if (vignette != null && colorGrading != null)
        {
            vignette.intensity.value = inactiveVignetteIntensity;
            colorGrading.postExposure.value = inactiveExposure;
            RenderSettings.fogDensity = heavyFogDensity;
            Debug.Log("Forced dark vision effects");
        }
    }
}