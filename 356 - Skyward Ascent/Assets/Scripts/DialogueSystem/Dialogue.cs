/*DRAFT 1
using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerName;
    public float textSpeed = 0.05f;

    private string[] currentLines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    public bool IsDialogueActive => dialoguePanel.activeSelf;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                if (currentLineIndex < currentLines.Length - 1)
                    ShowNextLine();
                else
                    EndDialogue();
            }
        }
    }

    public void StartDialogue(string[] lines, string speaker = "")
    {
        if (lines == null || lines.Length == 0) return;

        // Prevent restarting dialogue if already active
        if (IsDialogueActive) return;

        currentLines = lines;
        currentLineIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerName != null)
            speakerName.text = speaker;

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLines[currentLineIndex];
        isTyping = false;
    }

    private void ShowNextLine()
    {
        currentLineIndex++;
        DisplayCurrentLine();
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialoguePanel.SetActive(false);
        currentLines = null;
        currentLineIndex = 0;
        isTyping = false;
        typingCoroutine = null;
    }
}
*/
using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerName;
    public float textSpeed = 0.05f;

    private string[] currentLines;
    private int currentLineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private InteractableCharacter currentCharacter;

    public bool IsDialogueActive => dialoguePanel != null && dialoguePanel.activeSelf;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!IsDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                if (currentLineIndex < currentLines.Length - 1)
                    ShowNextLine();
                else
                    EndDialogue();
            }
        }
    }

    public void StartDialogue(string[] lines, string speaker = "", InteractableCharacter character = null)
    {
        if (lines == null || lines.Length == 0) return;

        if (IsDialogueActive) return;

        currentLines = lines;
        currentLineIndex = 0;
        currentCharacter = character;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerName != null)
            speakerName.text = speaker;

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLines[currentLineIndex];
        isTyping = false;
    }

    private void ShowNextLine()
    {
        currentLineIndex++;
        DisplayCurrentLine();
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentLines = null;
        currentLineIndex = 0;
        isTyping = false;
        typingCoroutine = null;

        if (currentCharacter != null)
        {
            currentCharacter.OnDialogueComplete();
        }

        currentCharacter = null;
    }

    public void ForceEndDialogue()
    {
        EndDialogue();
    }
}