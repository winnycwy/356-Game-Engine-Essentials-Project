using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI interactionText;
    public GameObject interactionPanel;

    [Header("Interaction Messages")]
    public string runestoneMessage = "E - Activate Runestone";
    public string flowerMessage = "E - Collect Flower";

    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        // Hide interaction prompt at start
        HideInteractionPrompt();
    }

    public void ShowInteractionPrompt(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
            interactionText.gameObject.SetActive(true);
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }
}