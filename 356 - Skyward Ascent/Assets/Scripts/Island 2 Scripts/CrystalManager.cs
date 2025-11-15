using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CrystalManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI crystalCountText;
    public GameObject crystalCollectedPopup;
    public AudioClip collectSound;

    private int totalCrystals = 0;
    private AudioSource audioSource;

    public static CrystalManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        UpdateUI();
    }

    public void AddCrystal(int amount = 1)  // This is the correct method name
    {
        totalCrystals += amount;

        // Play sound
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Show popup
        if (crystalCollectedPopup != null)
        {
            StartCoroutine(ShowPopup());
        }

        UpdateUI();
        Debug.Log($"Crystal collected! Total: {totalCrystals}");
    }

    public int GetCrystalCount()
    {
        return totalCrystals;
    }

    private void UpdateUI()
    {
        if (crystalCountText != null)
        {
            crystalCountText.text = $"Crystals: {totalCrystals}";
        }
    }

    private IEnumerator ShowPopup()
    {
        if (crystalCollectedPopup != null)
        {
            crystalCollectedPopup.SetActive(true);
            yield return new WaitForSeconds(2f);
            crystalCollectedPopup.SetActive(false);
        }
    }

    [ContextMenu("Reset Crystals")]
    public void ResetCrystals()
    {
        totalCrystals = 0;
        UpdateUI();
    }
}