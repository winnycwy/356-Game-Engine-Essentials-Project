using UnityEngine;
using System.Collections;

public class HeartbloomTreeController : MonoBehaviour
{
    [Header("Tree States")]
    public bool isActivated = false;
    public Material dormantMaterial;
    public Material activatedMaterial;
    public Light treeGlowLight;

    [Header("Teleport Settings")]
    public string nextIslandSceneName = "Island2";
    public Transform teleportPoint;
    public float teleportDelay = 2f;

    [Header("Particle Effects")]
    public ParticleSystem activationParticles;
    public ParticleSystem teleportParticles;

    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip teleportSound;

    private Renderer treeRenderer;
    private AudioSource audioSource;
    private bool isTeleporting = false;

    void Start()
    {
        treeRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        // Set initial state
        SetTreeState(false);
    }

    void SetTreeState(bool activated)
    {
        if (treeRenderer != null)
        {
            treeRenderer.material = activated ? activatedMaterial : dormantMaterial;
        }

        if (treeGlowLight != null)
        {
            treeGlowLight.enabled = activated;
        }

        isActivated = activated;
    }

    // Call this when the fairy lights the tree (after collecting all Sun Petals)
    public void ActivateTree()
    {
        if (isActivated) return;

        SetTreeState(true);

        // Play activation effects
        if (activationParticles != null)
            activationParticles.Play();

        if (audioSource != null && activationSound != null)
            audioSource.PlayOneShot(activationSound);

        Debug.Log("Heartbloom Tree Activated! Ready for teleportation.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActivated || isTeleporting) return;

        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            StartCoroutine(TeleportPlayer(other.gameObject));
        }
    }

    IEnumerator TeleportPlayer(GameObject player)
    {
        isTeleporting = true;

        // Disable player movement during teleport
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Play teleport effects
        if (teleportParticles != null)
            teleportParticles.Play();

        if (audioSource != null && teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        // Optional: Fade out screen or other visual effects
        Debug.Log("Teleporting to next island...");

        // Wait for the teleport delay
        yield return new WaitForSeconds(teleportDelay);

        // Actually teleport (load next scene)
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextIslandSceneName);
    }

    // For debugging in the editor
    void Update()
    {
        // Remove this in final build - for testing only
        if (Input.GetKeyDown(KeyCode.T) && !isActivated)
        {
            ActivateTree();
        }
    }
}
