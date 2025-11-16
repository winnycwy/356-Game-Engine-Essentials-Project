/*DRAFT 1
using UnityEngine;
using System.Collections;

public class FaeLightAbility : MonoBehaviour
{
    [Header("Fae Light Settings")]
    public GameObject faeLightPrefab;          // The ball of light GameObject
    public Transform lightSpawnPoint;          // Where the light spawns (e.g., player's hand or front)
    public KeyCode activateKey = KeyCode.Q;    // Key to hold for Fae Light
    public float maxLightDistance = 3f;        // How far the light can float from player
    public float lightFollowSpeed = 2f;        // How quickly light follows player

    [Header("UI Settings")]
    public GameObject faeLightUI;              // The UI icon in bottom right

    private GameObject currentFaeLight;
    private bool isAbilityUnlocked = false;
    private bool isLightActive = false;

    void Start()
    {
        // Hide UI initially
        if (faeLightUI != null)
            faeLightUI.SetActive(false);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Hold Q to activate/deactivate Fae Light
        if (Input.GetKeyDown(activateKey))
        {
            ToggleFaeLight();
        }

        // Update light position if active
        if (isLightActive && currentFaeLight != null)
        {
            UpdateLightPosition();
        }
    }

    public void UnlockFaeLight()
    {
        isAbilityUnlocked = true;

        // Show UI icon
        if (faeLightUI != null)
            faeLightUI.SetActive(true);

        Debug.Log("Fae Light ability unlocked! Hold Q to activate.");
    }

    private void ToggleFaeLight()
    {
        if (!isLightActive)
        {
            ActivateFaeLight();
        }
        else
        {
            DeactivateFaeLight();
        }
    }

    private void ActivateFaeLight()
    {
        if (faeLightPrefab != null && lightSpawnPoint != null)
        {
            currentFaeLight = Instantiate(faeLightPrefab, lightSpawnPoint.position, Quaternion.identity);
            isLightActive = true;
            Debug.Log("Fae Light activated!");
        }
    }

    private void DeactivateFaeLight()
    {
        if (currentFaeLight != null)
        {
            Destroy(currentFaeLight);
            isLightActive = false;
            Debug.Log("Fae Light deactivated!");
        }
    }

    private void UpdateLightPosition()
    {
        // Make the light float around the player
        Vector3 targetPosition = lightSpawnPoint.position +
            new Vector3(Mathf.Sin(Time.time) * maxLightDistance,
                       Mathf.Cos(Time.time) * 0.5f,
                       Mathf.Cos(Time.time) * maxLightDistance);

        currentFaeLight.transform.position = Vector3.Lerp(
            currentFaeLight.transform.position,
            targetPosition,
            Time.deltaTime * lightFollowSpeed
        );
    }

    // Public method to check if light is active (for puzzles)
    public bool IsLightActive()
    {
        return isLightActive && currentFaeLight != null;
    }

    // Get the current light instance (for other scripts to reference)
    public GameObject GetCurrentLight()
    {
        return currentFaeLight;
    }
}
*/
/* DRAFT 2 - Update FaeLightAbility Script (Fix positioning and hold Q)
using UnityEngine;
using System.Collections;

public class FaeLightAbility : MonoBehaviour
{
    [Header("Fae Light Settings")]
    public GameObject faeLightPrefab;
    public Transform lightSpawnPoint;
    public KeyCode activateKey = KeyCode.Q;
    public float lightDistance = 2f;           // Distance from player
    public float lightHeight = 1f;             // Height relative to player
    public float lightAngle = 30f;             // Angle in front of player (degrees)

    [Header("UI Settings")]
    public GameObject faeLightUI;

    private GameObject currentFaeLight;
    private bool isAbilityUnlocked = false;
    private bool isLightActive = false;

    void Start()
    {
        if (faeLightUI != null)
            faeLightUI.SetActive(false);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Hold Q to activate, release to deactivate
        if (Input.GetKey(activateKey) && !isLightActive)
        {
            ActivateFaeLight();
        }
        else if (!Input.GetKey(activateKey) && isLightActive)
        {
            DeactivateFaeLight();
        }

        // Update light position if active
        if (isLightActive && currentFaeLight != null)
        {
            UpdateLightPosition();
        }
    }

    public void UnlockFaeLight()
    {
        isAbilityUnlocked = true;

        if (faeLightUI != null)
            faeLightUI.SetActive(true);

        Debug.Log("Fae Light ability unlocked! Hold Q to activate.");
    }

    private void ActivateFaeLight()
    {
        if (faeLightPrefab != null)
        {
            currentFaeLight = Instantiate(faeLightPrefab);
            isLightActive = true;
            UpdateLightPosition(); // Set initial position
            Debug.Log("Fae Light activated!");
        }
    }

    private void DeactivateFaeLight()
    {
        if (currentFaeLight != null)
        {
            Destroy(currentFaeLight);
            isLightActive = false;
            Debug.Log("Fae Light deactivated!");
        }
    }

    private void UpdateLightPosition()
    {
        if (currentFaeLight == null) return;

        // Calculate position in front and to the side of player
        Vector3 playerForward = transform.forward;
        Vector3 playerRight = transform.right;

        // Position: 30 degrees to the right front of player
        Quaternion rotation = Quaternion.Euler(0, lightAngle, 0);
        Vector3 offset = rotation * playerForward * lightDistance;
        offset.y = lightHeight; // Add height

        currentFaeLight.transform.position = transform.position + offset;

        // Make light face the same direction as player
        currentFaeLight.transform.rotation = Quaternion.LookRotation(playerForward);
    }

    public bool IsLightActive()
    {
        return isLightActive && currentFaeLight != null;
    }

    public GameObject GetCurrentLight()
    {
        return currentFaeLight;
    }
}

*/
/*DRAFT 3
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FaeLightAbility : MonoBehaviour
{
    [Header("Fae Light Settings")]
    public GameObject faeLightPrefab;
    public Transform lightSpawnPoint;
    public KeyCode activateKey = KeyCode.Q;
    public float lightDistance = 2f;
    public float lightHeight = 1f;
    public float lightAngle = 30f;

    [Header("UI Settings")]
    public GameObject faeLightUI;
    public UnityEngine.UI.Image faeLightButtonImage;
    public Sprite defaultButtonSprite;
    public Sprite pressedButtonSprite;
    public Color defaultColor = Color.white;
    public Color activeColor = new Color(1f, 0.9f, 0.3f); // Gold/yellow when active

    [Header("Button Effects")]
    public float pressScale = 0.9f;
    public float pressDuration = 0.1f;

    private GameObject currentFaeLight;
    private bool isAbilityUnlocked = false;
    private bool isLightActive = false;
    private Vector3 originalButtonScale;
    private Coroutine buttonEffectCoroutine;

    void Start()
    {
        if (faeLightUI != null)
            faeLightUI.SetActive(false);

        // Store original button scale for animation
        if (faeLightButtonImage != null)
            originalButtonScale = faeLightButtonImage.transform.localScale;
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Handle button press/release with visual feedback
        if (Input.GetKeyDown(activateKey) && !isLightActive)
        {
            StartButtonPressEffect();
        }
        else if (Input.GetKeyUp(activateKey) && isLightActive)
        {
            StartButtonReleaseEffect();
            DeactivateFaeLight();
        }

        // Hold Q to keep light active
        if (Input.GetKey(activateKey) && !isLightActive)
        {
            ActivateFaeLight();
        }

        // Update light position if active
        if (isLightActive && currentFaeLight != null)
        {
            UpdateLightPosition();
        }
    }

    public void UnlockFaeLight()
    {
        isAbilityUnlocked = true;

        if (faeLightUI != null)
            faeLightUI.SetActive(true);

        Debug.Log("Fae Light ability unlocked! Hold Q to activate.");
    }

    private void ActivateFaeLight()
    {
        if (faeLightPrefab != null)
        {
            currentFaeLight = Instantiate(faeLightPrefab);
            isLightActive = true;
            UpdateLightPosition();

            // Update UI to show active state
            SetButtonActiveState(true);
            Debug.Log("Fae Light activated!");
        }
    }

    private void DeactivateFaeLight()
    {
        if (currentFaeLight != null)
        {
            Destroy(currentFaeLight);
            isLightActive = false;

            // Update UI to show inactive state
            SetButtonActiveState(false);
            Debug.Log("Fae Light deactivated!");
        }
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
        if (faeLightButtonImage == null) yield break;

        // Change to pressed sprite
        if (pressedButtonSprite != null)
            faeLightButtonImage.sprite = pressedButtonSprite;

        // Scale down
        float timer = 0f;
        Vector3 startScale = faeLightButtonImage.transform.localScale;
        Vector3 targetScale = originalButtonScale * pressScale;

        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            faeLightButtonImage.transform.localScale = Vector3.Lerp(startScale, targetScale, timer / pressDuration);
            yield return null;
        }

        faeLightButtonImage.transform.localScale = targetScale;
    }

    private IEnumerator ButtonReleaseEffect()
    {
        if (faeLightButtonImage == null) yield break;

        // Scale back up
        float timer = 0f;
        Vector3 startScale = faeLightButtonImage.transform.localScale;
        Vector3 targetScale = originalButtonScale;

        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            faeLightButtonImage.transform.localScale = Vector3.Lerp(startScale, targetScale, timer / pressDuration);
            yield return null;
        }

        faeLightButtonImage.transform.localScale = targetScale;

        // Change back to default sprite
        if (defaultButtonSprite != null)
            faeLightButtonImage.sprite = defaultButtonSprite;
    }

    private void SetButtonActiveState(bool active)
    {
        if (faeLightButtonImage == null) return;

        if (active)
        {
            // Change color when light is active
            faeLightButtonImage.color = activeColor;

            // Keep pressed sprite while active
            if (pressedButtonSprite != null)
                faeLightButtonImage.sprite = pressedButtonSprite;
        }
        else
        {
            // Return to default appearance
            faeLightButtonImage.color = defaultColor;

            if (defaultButtonSprite != null)
                faeLightButtonImage.sprite = defaultButtonSprite;
        }
    }

    private void UpdateLightPosition()
    {
        if (currentFaeLight == null) return;

        Vector3 playerForward = transform.forward;
        Vector3 playerRight = transform.right;

        Quaternion rotation = Quaternion.Euler(0, lightAngle, 0);
        Vector3 offset = rotation * playerForward * lightDistance;
        offset.y = lightHeight;


        currentFaeLight.transform.position = transform.position + offset;
        currentFaeLight.transform.rotation = Quaternion.LookRotation(playerForward);

    }

    public bool IsLightActive()
    {
        return isLightActive && currentFaeLight != null;
    }

    public GameObject GetCurrentLight()
    {
        return currentFaeLight;
    }
}
*/
/*DRAFT 4 - Floating but light intensity remains the same
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FaeLightAbility : MonoBehaviour
{
    [Header("Fae Light Settings")]
    public GameObject faeLightPrefab;
    public Transform lightSpawnPoint;          // Optional reference point
    public KeyCode activateKey = KeyCode.Q;
    public float lightDistance = 2f;
    public float lightHeight = 1f;
    public float lightAngle = 30f;

    [Header("Floating Animation")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 1.5f;
    public float rotationSpeed = 20f;

    [Header("UI References")]
    public GameObject faeLightUI;
    public Image buttonBackground;
    public Image buttonIcon;
    public TextMeshProUGUI keybindText;
    public Image pressEffect;

    [Header("Button Visuals")]
    public Sprite defaultBackground;
    public Sprite pressedBackground;
    public Color defaultIconColor = Color.white;
    public Color activeIconColor = new Color(1f, 0.9f, 0.3f);
    public Color defaultTextColor = Color.white;
    public Color activeTextColor = new Color(1f, 0.9f, 0.3f);

    [Header("Button Effects")]
    public float pressScale = 0.85f;
    public float pressDuration = 0.1f;

    private GameObject currentFaeLight;
    private Light faePointLight; // Store reference to the light component
    private bool isAbilityUnlocked = false;
    private bool isLightActive = false;
    private Vector3 originalButtonScale;
    private Coroutine buttonEffectCoroutine;
    private Vector3 lightBasePosition;

    void Start()
    {
        if (faeLightUI != null)
            faeLightUI.SetActive(false);

        if (buttonBackground != null)
            originalButtonScale = buttonBackground.transform.localScale;

        if (pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        // Handle button visual feedback
        if (Input.GetKeyDown(activateKey))
        {
            StartButtonPressEffect();
        }

        if (Input.GetKeyUp(activateKey))
        {
            StartButtonReleaseEffect();
            if (isLightActive)
                DeactivateFaeLight();
        }

        // Hold Q to keep light active
        if (Input.GetKey(activateKey) && !isLightActive)
        {
            ActivateFaeLight();
        }

        // Update light position and animation if active
        if (isLightActive && currentFaeLight != null)
        {
            UpdateLightPosition();
            AnimateFaeLight();
        }
    }

    public void UnlockFaeLight()
    {
        isAbilityUnlocked = true;

        if (faeLightUI != null)
            faeLightUI.SetActive(true);

        Debug.Log("Fae Light ability unlocked! Hold Q to activate.");
    }

    private void ActivateFaeLight()
    {
        if (faeLightPrefab != null)
        {
            // Calculate initial position first
            CalculateBasePosition();

            // Instantiate at the calculated position instead of lightSpawnPoint
            currentFaeLight = Instantiate(faeLightPrefab, lightBasePosition, Quaternion.identity);

            // Get the light component immediately
            faePointLight = currentFaeLight.GetComponentInChildren<Light>();
            if (faePointLight == null)
            {
                Debug.LogWarning("No Light component found on Fae Light prefab!");
            }

            isLightActive = true;
            SetButtonActiveState(true);

            Debug.Log("Fae Light activated! Light component: " + (faePointLight != null));
        }
    }

    private void DeactivateFaeLight()
    {
        if (currentFaeLight != null)
        {
            Destroy(currentFaeLight);
            currentFaeLight = null;
            faePointLight = null;
            isLightActive = false;
            SetButtonActiveState(false);
            Debug.Log("Fae Light deactivated!");
        }
    }

    private void CalculateBasePosition()
    {
        Vector3 playerForward = transform.forward;

        Quaternion rotation = Quaternion.Euler(0, lightAngle, 0);
        Vector3 offset = rotation * playerForward * lightDistance;
        offset.y = lightHeight;

        // Use player position as base, not lightSpawnPoint
        lightBasePosition = transform.position + offset;
    }

    private void UpdateLightPosition()
    {
        if (currentFaeLight == null) return;

        // Recalculate base position (in case player moved)
        CalculateBasePosition();

        // Apply floating animation on top of base position
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        Vector3 animatedPosition = lightBasePosition + new Vector3(0, yOffset, 0);

        currentFaeLight.transform.position = animatedPosition;

        // Make light face the same direction as player
        currentFaeLight.transform.rotation = Quaternion.LookRotation(transform.forward);
    }

    private void AnimateFaeLight()
    {
        if (currentFaeLight == null) return;

        // Gentle rotation
        currentFaeLight.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);

        // Light pulsing - use stored reference instead of GetComponent every frame
        if (faePointLight != null)
        {
            float pulse = (Mathf.Sin(Time.time * floatSpeed * 2f) + 1f) * 0.5f;
            faePointLight.intensity = Mathf.Lerp(1f, 2f, pulse);
        }
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
            pressEffect.color = new Color(1, 1, 1, 0.3f);

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

        if (!isLightActive && defaultBackground != null)
            buttonBackground.sprite = defaultBackground;
    }

    private void SetButtonActiveState(bool active)
    {
        if (buttonIcon != null)
            buttonIcon.color = active ? activeIconColor : defaultIconColor;

        if (keybindText != null)
            keybindText.color = active ? activeTextColor : defaultTextColor;

        if (active && pressEffect != null)
            pressEffect.color = new Color(1, 1, 0.5f, 0.5f);
        else if (!active && pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);
    }

    public bool IsLightActive()
    {
        return isLightActive && currentFaeLight != null;
    }

    public GameObject GetCurrentLight()
    {
        return currentFaeLight;
    }
}
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FaeLightAbility : MonoBehaviour
{
    [Header("Fae Light Settings")]
    public GameObject faeLightPrefab;
    public Transform lightSpawnPoint;
    public KeyCode activateKey = KeyCode.Q;
    public float lightDistance = 2f;
    public float lightHeight = 1f;
    public float lightAngle = 30f;

    [Header("Floating Animation")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 1.5f;
    public float rotationSpeed = 20f;

    [Header("Light Pulsing")]
    public bool enableLightPulsing = true; // Option to disable pulsing
    public float pulseIntensityMultiplier = 0.2f; // How much to pulse (0.2 = ±20%)

    [Header("UI References")]
    public GameObject faeLightUI;
    public Image buttonBackground;
    public Image buttonIcon;
    public TextMeshProUGUI keybindText;
    public Image pressEffect;

    [Header("Button Visuals")]
    public Sprite defaultBackground;
    public Sprite pressedBackground;
    public Color defaultIconColor = Color.white;
    public Color activeIconColor = new Color(1f, 0.9f, 0.3f);
    public Color defaultTextColor = Color.white;
    public Color activeTextColor = new Color(1f, 0.9f, 0.3f);

    [Header("Button Effects")]
    public float pressScale = 0.85f;
    public float pressDuration = 0.1f;

    public float orbDamageRadius = 2f;
    public float orbDamagePerSecond = 1f;

    private GameObject currentFaeLight;
    private Light faePointLight;
    private bool isAbilityUnlocked = false;
    private bool isLightActive = false;
    private Vector3 originalButtonScale;
    private Coroutine buttonEffectCoroutine;
    private Vector3 lightBasePosition;
    private float baseLightIntensity; // Store the prefab's original intensity

    void Start()
    {
        if (faeLightUI != null)
            faeLightUI.SetActive(false);

        if (buttonBackground != null)
            originalButtonScale = buttonBackground.transform.localScale;

        if (pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if (!isAbilityUnlocked) return;

        if (Input.GetKeyDown(activateKey))
        {
            StartButtonPressEffect();
        }

        if (Input.GetKeyUp(activateKey))
        {
            StartButtonReleaseEffect();
            if (isLightActive)
                DeactivateFaeLight();
        }

        if (Input.GetKey(activateKey) && !isLightActive)
        {
            ActivateFaeLight();
        }

        if (isLightActive && currentFaeLight != null)
        {
            UpdateLightPosition();
            AnimateFaeLight();

            // DAMAGE ORBS IN RANGE
            Collider[] hits = Physics.OverlapSphere(currentFaeLight.transform.position, orbDamageRadius);
            foreach (Collider hit in hits)
            {
                DarkFaeOrb orb = hit.GetComponent<DarkFaeOrb>();
                if (orb != null)
                {
                    orb.TakeFaeLightDamage(orbDamagePerSecond * Time.deltaTime);
                }
            }
        }
    }

    public void UnlockFaeLight()
    {
        isAbilityUnlocked = true;

        if (faeLightUI != null)
            faeLightUI.SetActive(true);

        Debug.Log("Fae Light ability unlocked! Hold Q to activate.");
    }

    // Modify the ActivateFaeLight method:
    private void ActivateFaeLight()
    {
        if (faeLightPrefab != null)
        {
            CalculateBasePosition();
            currentFaeLight = Instantiate(faeLightPrefab, lightBasePosition, Quaternion.identity);

            faePointLight = currentFaeLight.GetComponentInChildren<Light>();
            if (faePointLight != null)
            {
                baseLightIntensity = faePointLight.intensity;
            }

            isLightActive = true;
            SetButtonActiveState(true);

        }
    }

    // Modify the DeactivateFaeLight method:
    private void DeactivateFaeLight()
    {
        if (currentFaeLight != null)
        {
            Destroy(currentFaeLight);
            currentFaeLight = null;
            faePointLight = null;
            isLightActive = false;
            SetButtonActiveState(false);
        }
    }
    private void CalculateBasePosition()
    {
        Vector3 playerForward = transform.forward;
        Quaternion rotation = Quaternion.Euler(0, lightAngle, 0);
        Vector3 offset = rotation * playerForward * lightDistance;
        offset.y = lightHeight;
        lightBasePosition = transform.position + offset;
    }

    private void UpdateLightPosition()
    {
        if (currentFaeLight == null) return;

        CalculateBasePosition();
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        Vector3 animatedPosition = lightBasePosition + new Vector3(0, yOffset, 0);
        currentFaeLight.transform.position = animatedPosition;
        currentFaeLight.transform.rotation = Quaternion.LookRotation(transform.forward);
    }

    private void AnimateFaeLight()
    {
        if (currentFaeLight == null) return;

        // Gentle rotation
        currentFaeLight.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);

        // Light pulsing - respect the prefab's base intensity
        if (faePointLight != null && enableLightPulsing)
        {
            float pulse = (Mathf.Sin(Time.time * floatSpeed * 2f) + 1f) * 0.5f;
            float pulseAmount = Mathf.Lerp(1f - pulseIntensityMultiplier, 1f + pulseIntensityMultiplier, pulse);
            faePointLight.intensity = baseLightIntensity * pulseAmount; // Use base intensity
        }
    }

    // ... (rest of your button effect methods remain the same)
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
            pressEffect.color = new Color(1, 1, 1, 0.3f);

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
        if (!isLightActive && defaultBackground != null)
            buttonBackground.sprite = defaultBackground;
    }

    private void SetButtonActiveState(bool active)
    {
        if (buttonIcon != null)
            buttonIcon.color = active ? activeIconColor : defaultIconColor;
        if (keybindText != null)
            keybindText.color = active ? activeTextColor : defaultTextColor;
        if (active && pressEffect != null)
            pressEffect.color = new Color(1, 1, 0.5f, 0.5f);
        else if (!active && pressEffect != null)
            pressEffect.color = new Color(1, 1, 1, 0);
    }

    public bool IsLightActive()
    {
        return isLightActive && currentFaeLight != null;
    }

    public GameObject GetCurrentLight()
    {
        return currentFaeLight;
    }
}