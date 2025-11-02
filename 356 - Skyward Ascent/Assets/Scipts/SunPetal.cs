using UnityEngine;

public class SunPetal : MonoBehaviour
{
    [Header("Petal Properties")]
    public float rotationSpeed = 50f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;
    private bool isCollected = false;
    private FairyFloraController fairyController;

    void Start()
    {
        startPosition = transform.position;
        fairyController = FindObjectOfType<FairyFloraController>();
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

        if (other.CompareTag("Player"))
        {
            CollectPetal();
        }
    }

    void CollectPetal()
    {
        isCollected = true;

        if (fairyController != null)
        {
            fairyController.CollectSunPetal();
        }

        PlayCollectionEffects();
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 2f);
    }

    void PlayCollectionEffects()
    {
        ParticleSystem collectParticles = GetComponent<ParticleSystem>();
        if (collectParticles != null)
            collectParticles.Play();
    }
}