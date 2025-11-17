using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endingCanvas;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI narrationText;
    public Image fadePanel;
    public Button continueButton;

    [Header("Ending Sequence")]
    public string actTitle = "Act III: The Redemption";
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
    public float fadeInDuration = 2f;
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

    private Vector3 cameraStartPosition;
    private float cameraStartFov;
    private bool cameraEffectPlaying = false;

    private void Start()
    {
        // Hide ending UI initially
        if (endingCanvas != null)
            endingCanvas.SetActive(false);

        // Setup continue button
        if (continueButton != null)
            continueButton.onClick.AddListener(ReturnToMainMenu);

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
        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // Start camera effects
        if (mainCamera != null && cameraFinalPosition != null)
        {
            StartCoroutine(CameraEndingEffect());
        }

        // Start redemption music
        if (audioSource != null && redemptionMusic != null)
        {
            audioSource.clip = redemptionMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Show ending canvas
        if (endingCanvas != null)
            endingCanvas.SetActive(true);

        // Initial fade in
        if (fadePanel != null)
        {
            fadePanel.color = Color.black;
            yield return StartCoroutine(FadePanel(0f, fadeInDuration));
        }

        // Display act title
        if (titleText != null)
        {
            titleText.text = actTitle;
            titleText.gameObject.SetActive(true);
            yield return new WaitForSeconds(3f);
        }

        // Display each narration line
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

        // Show final message and continue button
        if (subtitleText != null)
        {
            subtitleText.text = "Thank you for playing";
            subtitleText.gameObject.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        // Wait for camera effect to complete
        yield return new WaitUntil(() => !cameraEffectPlaying);
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

    public void ReturnToMainMenu()
    {
        // Fade out audio
        if (audioSource != null)
            StartCoroutine(FadeAudioOut(1f));

        // Load main menu after fade
        StartCoroutine(LoadMainMenuAfterDelay(1f));
    }

    private IEnumerator FadeAudioOut(float duration)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    private IEnumerator LoadMainMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MainMenu");
    }
}