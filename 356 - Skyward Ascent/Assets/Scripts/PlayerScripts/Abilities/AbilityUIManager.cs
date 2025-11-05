using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AbilityUIManager : MonoBehaviour
{
    [Header("Fae Light UI References")]
    public GameObject faeLightAbilitySlot;
    public Image faeLightIcon;
    public TextMeshProUGUI abilityName;
    public TextMeshProUGUI keybindText;

    [Header("Hold Progress UI")]
    public GameObject holdProgressPanel;
    public Image holdProgressFill;
    public TextMeshProUGUI holdPromptText;

    [Header("Ability Settings")]
    public Sprite faeLightSprite;
    public string faeLightName = "Fae Light";

    private static AbilityUIManager _instance;
    public static AbilityUIManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        // Hide ability UI initially
        if (faeLightAbilitySlot != null)
            faeLightAbilitySlot.SetActive(false);

        if (holdProgressPanel != null)
            holdProgressPanel.SetActive(false);
    }

    public void UnlockFaeLightAbility()
    {
        // Setup ability slot
        if (faeLightIcon != null && faeLightSprite != null)
            faeLightIcon.sprite = faeLightSprite;

        if (abilityName != null)
            abilityName.text = faeLightName;

        if (keybindText != null)
            keybindText.text = "Q";

        // Show ability slot
        if (faeLightAbilitySlot != null)
            faeLightAbilitySlot.SetActive(true);

        Debug.Log("Fae Light UI unlocked!");
    }

    public void ShowHoldProgress(bool show)
    {
        if (holdProgressPanel != null)
        {
            holdProgressPanel.SetActive(show);

            if (show && holdPromptText != null)
            {
                holdPromptText.text = "Hold Q to summon Fae Light";
            }
        }
    }

    public void UpdateHoldProgress(float progress)
    {
        if (holdProgressFill != null)
        {
            holdProgressFill.fillAmount = progress;

            // Optional: change color based on progress
            holdProgressFill.color = Color.Lerp(Color.yellow, Color.green, progress);
        }
    }
}