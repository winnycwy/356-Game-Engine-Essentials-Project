using UnityEngine;

public class FairyTrigger : MonoBehaviour
{
    public FairyAI fairyAI; // Reference to your main FairyAI script

    void Start()
    {
        // If fairyAI is not assigned, try to find it
        if (fairyAI == null)
        {
            fairyAI = FindObjectOfType<FairyAI>();
            Debug.Log($"FairyTrigger: Auto-found FairyAI: {fairyAI != null}");
        }

        Debug.Log($"FairyTrigger: Initialized with FairyAI: {fairyAI != null}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"FairyTrigger: Something entered trigger: {other.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("FairyTrigger: Player entered trigger area");
            if (fairyAI != null)
            {
                fairyAI.PlayerInTrigger = true;
                Debug.Log($"FairyTrigger: Set PlayerInTrigger = true. Current fairy state: {fairyAI.GetCurrentState()}");
            }
            else
            {
                Debug.LogError("FairyTrigger: fairyAI reference is null!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"FairyTrigger: Something left trigger: {other.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("FairyTrigger: Player left trigger area");
            if (fairyAI != null)
            {
                fairyAI.PlayerInTrigger = false;
                Debug.Log($"FairyTrigger: Set PlayerInTrigger = false");
            }
        }
    }

    // Debugging: Show trigger area in scene view
    void OnDrawGizmos()
    {
        if (enabled)
        {
            Gizmos.color = Color.green;
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                if (collider is BoxCollider boxCollider)
                {
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                }
                else if (collider is SphereCollider sphereCollider)
                {
                    Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
                }
            }
        }
    }
}