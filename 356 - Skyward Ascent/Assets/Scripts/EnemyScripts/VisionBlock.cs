using UnityEngine;
using System.Collections;

public class VisionBlock : MonoBehaviour
{
    [Header("Reference to global volume object that should turn ON when hit")]
    public GameObject globalVolumeObject;

    [Header("Timing")]
    public float blockDuration = 1.5f;
    public float cooldown = 1.0f;

    private bool onCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !onCooldown)
        {
            StartCoroutine(BlockVision(other));
        }
    }

    private IEnumerator BlockVision(Collider player)
    {
        onCooldown = true;

        // Enable global volume
        if (globalVolumeObject != null)
            globalVolumeObject.SetActive(true);

        // Disable FaeLightAbility script on player
        FaeLightAbility faelight = player.GetComponent<FaeLightAbility>();
        if (faelight != null)
            faelight.enabled = false;

        // Wait for block duration
        yield return new WaitForSeconds(blockDuration);

        // Disable global volume
        if (globalVolumeObject != null)
            globalVolumeObject.SetActive(false);

        // Re-enable FaeLightAbility
        if (faelight != null)
            faelight.enabled = true;

        // Wait for cooldown before allowing another block
        yield return new WaitForSeconds(cooldown);

        onCooldown = false;
    }
}