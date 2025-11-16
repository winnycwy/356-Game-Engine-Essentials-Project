using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FireAbility : MonoBehaviour
{
    [Header("Fire Ability Settings")]
    public KeyCode activationKey = KeyCode.Mouse1;
    public float damage = 20f;
    public float castDuration = 2f;
    public float cooldown = 5f;
    public int maxShotsPerBat = 5;

    [Header("Spell Positioning")]
    public Transform spellCastPoint; // Assign empty GameObject at staff tip
    public float spawnDistance = 5f;
    public float spawnHeight = 3f;

    [Header("Visual Effects")]
    public GameObject fireRainPrefab;
    public ParticleSystem castEffect;
    public Light castLight;

    [Header("Animation")]
    public Animator playerAnimator;
    public string castAnimationTrigger = "CastFireSpell";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip castSound;
    public AudioClip fireRainSound;

    [Header("UI References")]
    public GameObject fireAbilityUI;
    public Image buttonBackground;
    public Image buttonIcon;
    public TextMeshProUGUI keybindText;
    public Image cooldownOverlay;
    public Image pressEffect;

    [Header("Button Visuals")]
    public Sprite defaultBackground;
    public Sprite pressedBackground;
    public Color defaultIconColor = Color.white;
    public Color activeIconColor = new Color(1f, 0.6f, 0.2f); // Orange fire color
    public Color defaultTextColor = Color.white;
    public Color activeTextColor = new Color(1f, 0.6f, 0.2f);
    public Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);

    [Header("Button Effects")]
    public float pressScale = 0.85f;
    public float pressDuration = 0.1f;

    // Private variables
    private bool isAbilityUnlocked = false;
    private bool isOnCooldown = false;
    private GameObject currentFireRain;
    private Vector3 originalButtonScale;
    private Coroutine buttonEffectCoroutine;
    private float cooldownTimer = 0f;

    // Public property to check if ability is unlocked
    public bool IsFireAbilityUnlocked
    {
        get { return isAbilityUnlocked; }
    }

    void Start()
    {
        // Initially disable the ability
        isAbilityUnlocked = false;

        // Hide UI initially
        if (fireAbilityUI != null)
            fireAbilityUI.SetActive(false);

        // Store original button scale
        if (buttonBackground != null)
            originalButtonScale = buttonBackground.transform.localScale;

        // Initialize UI elements
        if (pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        // Disable visual effects initially
        if (castLight != null)
            castLight.enabled = false;

        if (castEffect != null)
            castEffect.Stop();

        // If no spell cast point specified, use player transform
        if (spellCastPoint == null)
        {
            // Try to find a child object called "SpellCastPoint" or create one
            spellCastPoint = transform.Find("SpellCastPoint");
            if (spellCastPoint == null)
            {
                GameObject castPoint = new GameObject("SpellCastPoint");
                castPoint.transform.SetParent(transform);
                castPoint.transform.localPosition = new Vector3(0, 1.5f, 1f); // Approximate staff position
                spellCastPoint = castPoint.transform;
            }
        }
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Handle cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            UpdateCooldownUI();

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
                UpdateCooldownUI();
            }
        }

        // Handle button visual feedback
        if (Input.GetKeyDown(activationKey) && !isOnCooldown)
        {
            StartButtonPressEffect();
        }

        if (Input.GetKeyUp(activationKey))
        {
            StartButtonReleaseEffect();
        }

        // Cast spell on key press (not hold)
        if (Input.GetKeyDown(activationKey) && !isOnCooldown)
        {
            CastFoxFlame();
        }
    }

    public void CastFoxFlame()
    {
        if (isOnCooldown || !isAbilityUnlocked) return;

        StartCoroutine(CastFireRoutine());
    }

    IEnumerator CastFireRoutine()
    {
        isOnCooldown = true;
        cooldownTimer = cooldown;

        // Update UI to show active state
        SetButtonActiveState(true);

        // Trigger cast animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(castAnimationTrigger);
        }

        // Play cast sound
        if (audioSource != null && castSound != null)
        {
            audioSource.PlayOneShot(castSound);
        }

        // Show cast effects at staff tip
        if (castEffect != null && spellCastPoint != null)
        {
            castEffect.transform.position = spellCastPoint.position;
            castEffect.Play();
        }

        if (castLight != null && spellCastPoint != null)
        {
            castLight.transform.position = spellCastPoint.position;
            castLight.enabled = true;
        }

        // Wait for animation to reach casting point
        yield return new WaitForSeconds(0.8f);

        // Spawn fire rain at calculated position
        Vector3 spawnPosition = CalculateSpellPosition();
        if (fireRainPrefab != null)
        {
            currentFireRain = Instantiate(fireRainPrefab, spawnPosition, Quaternion.identity);

            // Play fire rain sound
            if (audioSource != null && fireRainSound != null)
            {
                audioSource.PlayOneShot(fireRainSound);
            }

            // Destroy fire rain after duration
            Destroy(currentFireRain, castDuration);
        }

        // Hide cast effects
        yield return new WaitForSeconds(0.5f);

        if (castLight != null)
        {
            castLight.enabled = false;
        }

        if (castEffect != null)
        {
            castEffect.Stop();
        }

        // Update UI to show inactive state (but still on cooldown)
        SetButtonActiveState(false);
    }

    private Vector3 CalculateSpellPosition()
    {
        if (spellCastPoint != null)
        {
            // Cast from staff position forward
            return spellCastPoint.position + spellCastPoint.forward * spawnDistance + Vector3.up * spawnHeight;
        }
        else
        {
            // Fallback: cast from player position
            return transform.position + transform.forward * spawnDistance + Vector3.up * spawnHeight;
        }
    }

    // Call this when player completes the fox quest
    public void UnlockFireAbility()
    {
        isAbilityUnlocked = true;

        // Show UI
        if (fireAbilityUI != null)
            fireAbilityUI.SetActive(true);

        Debug.Log("Fox's Flame ability unlocked! Press Right Mouse Button to cast.");
    }

    private void StartButtonPressEffect()
    {
        if (buttonEffectCoroutine != null)
            StopCoroutine(buttonEffectCoroutine);

        buttonEffectCoroutine = StartCoroutine(ButtonPressEffect());
    }

    private void StartButtonReleaseEffect()
    {
        if (buttonEffectCoroutine != null)
            StopCoroutine(buttonEffectCoroutine);

        buttonEffectCoroutine = StartCoroutine(ButtonReleaseEffect());
    }

    private IEnumerator ButtonPressEffect()
    {
        if (buttonBackground == null) yield break;

        if (pressedBackground != null)
            buttonBackground.sprite = pressedBackground;

        if (pressEffect != null)
            pressEffect.color = new Color(1, 0.6f, 0.2f, 0.3f); // Orange fire effect

        float timer = 0f;
        Vector3 startScale = buttonBackground.transform.localScale;
        Vector3 targetScale = originalButtonScale * pressScale;

        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            buttonBackground.transform.localScale = Vector3.Lerp(startScale, targetScale, timer / pressDuration);
            yield return null;
        }

        buttonBackground.transform.localScale = targetScale;
    }

    private IEnumerator ButtonReleaseEffect()
    {
        if (buttonBackground == null) yield break;

        float timer = 0f;
        Vector3 startScale = buttonBackground.transform.localScale;
        Vector3 targetScale = originalButtonScale;

        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            buttonBackground.transform.localScale = Vector3.Lerp(startScale, targetScale, timer / pressDuration);
            yield return null;
        }

        buttonBackground.transform.localScale = targetScale;

        if (pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);

        if (!isOnCooldown && defaultBackground != null)
            buttonBackground.sprite = defaultBackground;
    }

    private void SetButtonActiveState(bool active)
    {
        if (buttonIcon != null)
            buttonIcon.color = active ? activeIconColor : defaultIconColor;

        if (keybindText != null)
            keybindText.color = active ? activeTextColor : defaultTextColor;

        if (active && pressEffect != null)
            pressEffect.color = new Color(1, 0.6f, 0.2f, 0.5f); // Orange fire effect
        else if (!active && pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);
    }

    private void UpdateCooldownUI()
    {
        if (cooldownOverlay != null)
        {
            float cooldownProgress = cooldownTimer / cooldown;
            cooldownOverlay.fillAmount = cooldownProgress;

            // Optional: Change button appearance during cooldown
            if (buttonIcon != null)
            {
                buttonIcon.color = isOnCooldown ? cooldownColor : defaultIconColor;
            }
        }
    }

    // Helper methods for other scripts
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    public float GetCooldownProgress()
    {
        return isOnCooldown ? (cooldownTimer / cooldown) : 0f;
    }

    // Visual debug
    void OnDrawGizmosSelected()
    {
        if (!isAbilityUnlocked || spellCastPoint == null) return;

        Gizmos.color = Color.red;
        Vector3 spawnPos = CalculateSpellPosition();
        Gizmos.DrawWireSphere(spawnPos, 1f);
        Gizmos.DrawLine(spellCastPoint.position, spawnPos);
    }
}