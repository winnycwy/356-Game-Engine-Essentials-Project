using UnityEngine;

public class CrystalCollectible : MonoBehaviour
{
    [Header("Collection Settings")]
    public string playerTag = "Player";
    public int crystalValue = 1;
    public AudioClip collectSound;

    private HiddenCrystal hiddenCrystal;
    private bool isCollected = false;

    void Start()
    {
        hiddenCrystal = GetComponent<HiddenCrystal>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag(playerTag))
        {
            CollectCrystal();
        }
    }

    private void CollectCrystal()
    {
        isCollected = true;

        // Play collect sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Add to player's crystal count
        CrystalManager playerCrystals = FindObjectOfType<CrystalManager>();
        if (playerCrystals != null)
        {
            playerCrystals.AddCrystal(crystalValue);
        }

        // Visual effects
        StartCoroutine(CollectionEffect());

        Debug.Log($"Collected crystal! Total: {playerCrystals?.GetCrystalCount()}");
    }

    private System.Collections.IEnumerator CollectionEffect()
    {
        // Add particle effects, animation, etc.

        // Destroy after effect
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}