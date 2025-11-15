using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FireAbility : MonoBehaviour
{
    [Header("Fire Ability Settings")]
    public GameObject fireProjectilePrefab;
    public Transform fireSpawnPoint;
    public KeyCode activateKey = KeyCode.Mouse1; // Right mouse button
    public float fireSpeed = 15f;
    public float fireCooldown = 0.5f;
    public int damage = 20;

    [Header("UI References")]
    public GameObject fireAbilityUI;
    public Image buttonBackground;
    public Image buttonIcon;
    public TextMeshProUGUI keybindText;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;

    [Header("Button Visuals")]
    public Sprite defaultBackground;
    public Sprite pressedBackground;
    public Color defaultIconColor = Color.white;
    public Color activeIconColor = new Color(1f, 0.6f, 0.3f);
    public Color defaultTextColor = Color.white;
    public Color activeTextColor = new Color(1f, 0.6f, 0.3f);

    [Header("Button Effects")]
    public float pressScale = 0.85f;
    public float pressDuration = 0.1f;

    private bool isAbilityUnlocked = false;
    private bool isOnCooldown = false;
    private float currentCooldown;
    private Vector3 originalButtonScale;
    private Coroutine buttonEffectCoroutine;

    void Start()
    {
        if (fireAbilityUI != null)
            fireAbilityUI.SetActive(false);

        if (buttonBackground != null)
            originalButtonScale = buttonBackground.transform.localScale;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Handle cooldown
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            UpdateCooldownUI();

            if (currentCooldown <= 0f)
            {
                isOnCooldown = false;
                ResetCooldownUI();
            }
        }

        // Handle input - RIGHT MOUSE CLICK
        if (Input.GetMouseButtonDown(1) && !isOnCooldown) // 1 = right mouse button
        {
            ShootFireProjectile();
            StartButtonPressEffect();
        }

        if (Input.GetMouseButtonUp(1))
        {
            StartButtonReleaseEffect();
        }
    }

    public void UnlockFireAbility()
    {
        isAbilityUnlocked = true;

        if (fireAbilityUI != null)
            fireAbilityUI.SetActive(true);

        Debug.Log("Fire Ability unlocked! Press R to shoot fire projectiles.");
    }

    private void ShootFireProjectile()
    {
        if (fireProjectilePrefab != null && fireSpawnPoint != null)
        {
            GameObject fireball = Instantiate(fireProjectilePrefab, fireSpawnPoint.position, fireSpawnPoint.rotation);

            // Set up projectile component
            FireProjectile projectile = fireball.GetComponent<FireProjectile>();
            if (projectile != null)
            {
                projectile.damage = damage;
            }

            // Add velocity
            Rigidbody rb = fireball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = fireSpawnPoint.forward * fireSpeed;
            }

            // Start cooldown
            StartCooldown();
            SetButtonActiveState(true);
        }
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = fireCooldown;
    }

    private void UpdateCooldownUI()
    {
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = currentCooldown / fireCooldown;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.text = Mathf.Ceil(currentCooldown).ToString();
        }
    }

    private void ResetCooldownUI()
    {
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);

        SetButtonActiveState(false);
    }

    // Button effect methods (similar to Fae Light)
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

        if (!isOnCooldown && defaultBackground != null)
            buttonBackground.sprite = defaultBackground;
    }

    private void SetButtonActiveState(bool active)
    {
        if (buttonIcon != null)
            buttonIcon.color = active ? activeIconColor : defaultIconColor;
        if (keybindText != null)
            keybindText.color = active ? activeTextColor : defaultTextColor;
    }

    public bool IsFireAbilityUnlocked()
    {
        return isAbilityUnlocked;
    }
}