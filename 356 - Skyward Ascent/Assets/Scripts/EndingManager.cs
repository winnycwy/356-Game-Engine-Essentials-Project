using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endingCanvas;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI narrationText;
    public Image fadePanel;
    public Button quitButton;

    [Header("Background Image")]
    public Image backgroundImage; // Drag your background image here
    public Sprite endingBackground; // Assign your picture/sprite

    [Header("Ending Sequence")]
    public string[] narrationLines = new string[]
    {
        "The Defeat: You don't kill him. You break his corrupted shell.",
        "In his final moments, as his magic fades, his true, remorseful self is revealed.",
        "The Freedom: With his last bit of power, he transfers all the reclaimed memories back to you and the Tower.",
        "He apologizes, finally accepting his past, and his spirit finds peace, dissolving into the now-calm sky.",
        "The Tower is healed because he is finally healed.",
        "You take your place as the new Guardian of the Sky Tower."
    };

    [Header("Timing")]
    public float fadeToBlackDuration = 1.5f; // Fade TO black
    public float fadeFromBlackDuration = 2f;  // Fade FROM black to background
    public float lineDisplayDuration = 4f;
    public float betweenLineDelay = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip redemptionMusic; // Only this music needed

    [Header("Camera Effects")]
    public Camera mainCamera;
    public Transform cameraFinalPosition;
    public float cameraMoveDuration = 5f;
    public float cameraFovChange = 10f;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript; // Assign your player movement script here

    private Vector3 cameraStartPosition;
    private float cameraStartFov;
    private bool cameraEffectPlaying = false;
    private bool endingSequenceActive = false;

    private void Start()
    {
        // Hide ending UI initially
        if (endingCanvas != null)
            endingCanvas.SetActive(false);

        // Setup quit button
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClick);

        // Get camera references
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cameraStartPosition = mainCamera.transform.position;
            cameraStartFov = mainCamera.fieldOfView;
        }

        // Get audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void StartEndingSequence()
    {
        // ENABLE THIS GAMEOBJECT FIRST
        gameObject.SetActive(true);
        endingSequenceActive = true;

        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // DISABLE PLAYER MOVEMENT
        DisablePlayerMovement();

        // LOCK AND HIDE CURSOR during the story part
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ENABLE THE CANVAS FIRST (but keep it black for now)
        if (endingCanvas != null)
            endingCanvas.SetActive(true);

        // Set up fade panel to be fully black and visible
        if (fadePanel != null)
        {
            fadePanel.color = Color.black;
            fadePanel.gameObject.SetActive(true);
        }

        // Start redemption music
        if (audioSource != null && redemptionMusic != null)
        {
            audioSource.clip = redemptionMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Set background image (but keep it hidden behind black for now)
        if (backgroundImage != null && endingBackground != null)
        {
            backgroundImage.sprite = endingBackground;
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.color = new Color(1, 1, 1, 0); // Make background invisible initially
        }

        // Start camera effects
        if (mainCamera != null && cameraFinalPosition != null)
        {
            StartCoroutine(CameraEndingEffect());
        }

        // STEP 1: We're already at black screen (from fadePanel)
        // Just wait a moment at black for dramatic effect
        yield return new WaitForSeconds(0.5f);

        // STEP 2: Fade FROM black to reveal background image
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadePanel(0f, fadeFromBlackDuration)); // Fade black overlay to transparent
        }

        // STEP 3: Background is now fully visible, wait a moment
        yield return new WaitForSeconds(1f);

        // Display each narration line (START DIRECTLY WITH STORY)
        if (narrationText != null)
        {
            foreach (string line in narrationLines)
            {
                narrationText.text = line;
                narrationText.gameObject.SetActive(true);

                // Fade in text
                Color originalColor = narrationText.color;
                narrationText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                yield return StartCoroutine(FadeTMPText(narrationText, 1f, 1f));

                yield return new WaitForSeconds(lineDisplayDuration);

                // Fade out text
                yield return StartCoroutine(FadeTMPText(narrationText, 0f, 0.5f));

                yield return new WaitForSeconds(betweenLineDelay);
            }
        }

        // STEP 4: Fade to black for the final "End" screen
        yield return StartCoroutine(FadeToFinalScreen());
    }

    private IEnumerator FadeToFinalScreen()
    {
        // Fade out the background image to black
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadePanel(1f, 2f)); // Fade to black
        }

        // Hide the background image
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }

        // Show final message on black background
        if (subtitleText != null)
        {
            subtitleText.text = "End";
            subtitleText.gameObject.SetActive(true);

            // Fade in the "End" text
            Color originalColor = subtitleText.color;
            subtitleText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            yield return StartCoroutine(FadeTMPText(subtitleText, 1f, 1.5f));
        }

        // UNLOCK AND SHOW CURSOR so player can click the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show quit button after a delay
        yield return new WaitForSeconds(1f);

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);

            // Make sure the button is interactable
            quitButton.interactable = true;

            // Select the button so it can be clicked with keyboard/controller
            quitButton.Select();
        }

        // Wait for camera effect to complete
        yield return new WaitUntil(() => !cameraEffectPlaying);
    }

    private void DisablePlayerMovement()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
        else
        {
            // Try to find the player movement script automatically
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Try common movement script names
                MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in scripts)
                {
                    if (script.GetType().Name.Contains("Movement") ||
                        script.GetType().Name.Contains("Controller") ||
                        script.GetType().Name.Contains("Player"))
                    {
                        script.enabled = false;
                        Debug.Log("Disabled player movement: " + script.GetType().Name);
                    }
                }
            }
        }
    }

    private IEnumerator CameraEndingEffect()
    {
        cameraEffectPlaying = true;

        float currentTime = 0f;
        Vector3 startPos = mainCamera.transform.position;
        float startFov = mainCamera.fieldOfView;

        while (currentTime < cameraMoveDuration)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / cameraMoveDuration;

            // Move camera smoothly
            mainCamera.transform.position = Vector3.Lerp(startPos, cameraFinalPosition.position, progress);

            // Change FOV for dramatic effect
            mainCamera.fieldOfView = Mathf.Lerp(startFov, startFov + cameraFovChange, progress);

            // Slowly rotate camera for cinematic feel
            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                cameraFinalPosition.rotation,
                progress * 0.5f
            );

            yield return null;
        }

        cameraEffectPlaying = false;
    }

    private IEnumerator FadePanel(float targetAlpha, float duration)
    {
        float currentTime = 0f;
        Color startColor = fadePanel.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, targetColor, currentTime / duration);
            yield return null;
        }
    }

    private IEnumerator FadeTMPText(TextMeshProUGUI text, float targetAlpha, float duration)
    {
        float currentTime = 0f;
        Color startColor = text.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            text.color = Color.Lerp(startColor, targetColor, currentTime / duration);
            yield return null;
        }
    }

    // Quit button click handler
    public void OnQuitButtonClick()
    {
        Debug.Log("Quit button clicked - Quitting game");

        // Change button text to show it's working
        if (quitButton != null)
        {
            TextMeshProUGUI buttonText = quitButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "QUITTING...";
            }

            // Disable button to prevent multiple clicks
            quitButton.interactable = false;
        }

        // Quit the game after a short delay
        StartCoroutine(QuitAfterDelay(1f));
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        QuitGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        // Fade out audio
        if (audioSource != null)
            StartCoroutine(QuitAfterAudioFade(1f));
        else
            QuitImmediately();
    }

    private IEnumerator QuitAfterAudioFade(float duration)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        QuitImmediately();
    }

    private void QuitImmediately()
    {
#if UNITY_EDITOR
        // If running in Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running in built application
        Application.Quit();
#endif
    }

    // Optional: Handle escape key to quit as well
    private void Update()
    {
        if (endingSequenceActive && Input.GetKeyDown(KeyCode.Escape))
        {
            OnQuitButtonClick();
        }
    }
}