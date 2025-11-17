using UnityEngine;

public class IslandRespawnZone : MonoBehaviour
{
    public int islandIndex = 0;
    public Transform respawnPoint;

    [Header("Visualization")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDeathHandler playerDeath = other.GetComponent<PlayerDeathHandler>();
            if (playerDeath != null)
            {
                playerDeath.SetCurrentIsland(islandIndex);
                Debug.Log($"Player entered Island {islandIndex + 1} respawn zone");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw the trigger area
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = gizmoColor;
            if (collider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else if (collider is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }

        // Draw respawn point and connection line
        if (respawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(respawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, respawnPoint.position);

            // Draw arrow towards respawn point
            Vector3 direction = (respawnPoint.position - transform.position).normalized;
            DrawArrow(transform.position, direction, 1f);
        }
    }

    private void DrawArrow(Vector3 position, Vector3 direction, float length)
    {
        Gizmos.DrawRay(position, direction * length);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 200, 0) * Vector3.forward;
        Gizmos.DrawRay(position + direction * length, right * 0.25f);
        Gizmos.DrawRay(position + direction * length, left * 0.25f);
    }
}