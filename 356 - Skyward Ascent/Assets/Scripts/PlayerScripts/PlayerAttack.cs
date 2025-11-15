using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 10f;
    public float attackDamage = 10f;
    public KeyCode attackKey = KeyCode.Mouse0;

    [Header("Raycast Settings")]
    public float raycastHeight = 1.5f; // Shoot from chest height instead of feet
    public bool useCameraDirection = true;

    private void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }

        // Visual debug - show where we're aiming from
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeight;
        Vector3 direction = useCameraDirection && Camera.main != null ?
                           Camera.main.transform.forward : transform.forward;

        Debug.DrawRay(rayOrigin, direction * attackRange, Color.blue, 0.1f);
    }

    public void Attack()
    {
        Debug.Log("=== ATTACK DEBUG ===");

        // ✅ FIX: Shoot from chest height, not feet
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeight;
        Vector3 direction = useCameraDirection && Camera.main != null ?
                           Camera.main.transform.forward : transform.forward;

        Debug.Log($"🎯 Ray origin: {rayOrigin} (height: {raycastHeight})");
        Debug.Log($"🎯 Ray direction: {direction}");
        Debug.Log($"🎯 Bee collider center Y: ~0.39");

        RaycastHit hit;
        bool didHit = Physics.Raycast(rayOrigin, direction, out hit, attackRange);

        Debug.Log($"🎯 Raycast hit: {didHit}");

        if (didHit)
        {
            Debug.Log($"✅ HIT: {hit.collider.gameObject.name}");
            Debug.Log($"📍 Hit point: {hit.point} (Y: {hit.point.y})");
            Debug.Log($"📏 Distance: {hit.distance:F2}");
            Debug.Log($"🏷️ Tag: {hit.collider.tag}");

            // Visual feedback
            Debug.DrawLine(rayOrigin, hit.point, Color.red, 2f);

            CheckForEnemy(hit.collider);
        }
        else
        {
            Debug.Log("❌ MISS: No collision detected");
            Debug.Log($"💡 Ray origin Y: {rayOrigin.y}, Bee collider center Y: ~0.39");
            Debug.Log("💡 Try adjusting raycastHeight in Inspector");

            // Show miss
            Debug.DrawRay(rayOrigin, direction * attackRange, Color.yellow, 2f);

            // ✅ ADDITIONAL: Try a sphere cast (wider detection)
            Debug.Log("🔄 Trying SphereCast with radius 0.3...");
            if (Physics.SphereCast(rayOrigin, 0.3f, direction, out hit, attackRange))
            {
                Debug.Log($"✅ SPHERECAST HIT: {hit.collider.gameObject.name}");
                CheckForEnemy(hit.collider);
            }
        }

        Debug.Log("=== ATTACK END ===");
    }

    private void CheckForEnemy(Collider collider)
    {
        Debug.Log("🔍 Checking for enemy components...");

        // Method 1: Check for BeeScript
        BeeScript bee = collider.GetComponent<BeeScript>();
        if (bee != null)
        {
            Debug.Log("✅ Found BeeScript! Dealing damage...");
            bee.TakeDamage(attackDamage);
            return;
        }

        // Method 2: Check for BeeHealthController
        BeeHealthController beeHealth = collider.GetComponent<BeeHealthController>();
        if (beeHealth != null)
        {
            Debug.Log("✅ Found BeeHealthController! Dealing damage...");
            beeHealth.TakeDamage(attackDamage);
            return;
        }

        Debug.Log("❌ No enemy components found");
    }
}