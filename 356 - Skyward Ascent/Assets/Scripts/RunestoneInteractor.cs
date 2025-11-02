using UnityEngine;

public class RunestoneInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;           // Distance to interact
    public KeyCode interactKey = KeyCode.E;    // Key to activate runestone
    public LayerMask runestoneLayer;           // Layer for runstones

    private Runestone focusedRunestone = null;

    void Update()
    {
        CheckForRunestone();
        HandleRunestoneInteraction();
    }

    private void CheckForRunestone()
    {
        focusedRunestone = null;

        // Check all colliders in range on the runestone layer
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, runestoneLayer);
        foreach (Collider hit in hits)
        {
            Runestone rs = hit.GetComponent<Runestone>();
            if (rs != null && !rs.IsActivated())
            {
                focusedRunestone = rs;
                break; // Only focus the first available runestone
            }
        }
    }

    private void HandleRunestoneInteraction()
    {
        if (focusedRunestone != null && Input.GetKeyDown(interactKey))
        {
            focusedRunestone.ActivateRunestone();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}