using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;        // The main dialogue panel
    public TextMeshProUGUI dialogueText;    // Text component for dialogue
    public Image characterPortrait;         // Optional: Character image
    public TextMeshProUGUI speakerName;     // Optional: Speaker name

    [Header("Settings")]
    public float textSpeed = 0.05f;         // Typewriter effect speed
    public KeyCode advanceKey = KeyCode.Space; // Key to advance dialogue
    public bool allowMouseClick = true;     // Whether mouse clicks advance dialogue

    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string[] currentLines;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private PlayerController playerController;

    void Start()
    {
        // Ensure dialogue is hidden at start
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Find player controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // Check for spacebar input
        bool spacePressed = Input.GetKeyDown(advanceKey);

        // Check for mouse click if enabled
        bool mouseClicked = allowMouseClick && Input.GetMouseButtonDown(0);

        // Advance dialogue if either input is detected
        if (spacePressed || mouseClicked)
        {
            if (isTyping)
            {
                // Skip typing effect and show full line immediately
                SkipTyping();
            }
            else
            {
                // Move to next line
                ShowNextLine();
            }
        }
    }

    /// <summary>
    /// Start a new dialogue sequence
    /// </summary>
    public void StartDialogue(string[] lines, string speaker = "Fairy")
    {
        if (lines == null || lines.Length == 0) return;

        currentLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;

        // Update speaker name if provided
        if (speakerName != null)
            speakerName.text = speaker;

        // Show dialogue panel
        dialoguePanel.SetActive(true);

        // Disable player movement during dialogue
        if (playerController != null)
        {
            playerController.DisableMovement();
        }

        // Ensure cursor is visible during dialogue
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Display first line with typewriter effect
        DisplayCurrentLine();
    }

    /// <summary>
    /// Display the current line with typewriter effect
    /// </summary>
    private void DisplayCurrentLine()
    {
        if (currentLineIndex < currentLines.Length)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(currentLines[currentLineIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Typewriter effect coroutine
    /// </summary>
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// Skip the current typing effect and show full text
    /// </summary>
    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLines[currentLineIndex];
        isTyping = false;
    }

    /// <summary>
    /// Display the next line of dialogue
    /// </summary>
    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// End the current dialogue
    /// </summary>
    public void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialoguePanel.SetActive(false);
        currentLines = null;
        currentLineIndex = 0;

        // Note: Player movement is re-enabled by FairyAI when appropriate
        Debug.Log("Dialogue ended");
    }

    /// <summary>
    /// Check if dialogue is currently active
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    /// <summary>
    /// Force end dialogue from other scripts
    /// </summary>
    public void ForceEndDialogue()
    {
        EndDialogue();
    }

    /// <summary>
    /// Start dialogue with a single line (convenience method)
    /// </summary>
    public void StartSingleDialogue(string line, string speaker = "Fairy")
    {
        StartDialogue(new string[] { line }, speaker);
    }
}