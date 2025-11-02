using UnityEngine;

public class SunPetal : MonoBehaviour
{
    [Header("Petal Properties")]
    public float rotationSpeed = 50f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isCollected)
        {
            // Floating animation
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Rotation animation
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Check if player collected the petal
        if (other.CompareTag("Player"))
        {
            CollectPetal(other.gameObject);
        }
    }

    void CollectPetal(GameObject player)
    {
        isCollected = true;

        // Get the Fairy controller from the player
        FairyFloraController fairy = player.GetComponent<FairyFloraController>();
        if (fairy != null)
        {
            fairy.CollectSunPetal();
        }

        // Play collection effects
        PlayCollectionEffects();

        // Disable the petal
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Destroy after effects finish
        Destroy(gameObject, 2f);
    }

    void PlayCollectionEffects()
    {
        // Add particle effects, sounds, etc.
        Debug.Log("Sun Petal collected!");

        // Example: You could add a particle system that plays here
        ParticleSystem collectParticles = GetComponent<ParticleSystem>();
        if (collectParticles != null)
            collectParticles.Play();
    }
}
