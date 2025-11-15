using UnityEngine;
using System.Collections;

public class VisionBlock : MonoBehaviour
{
    [Header("References")]
    public GameObject globalVolumeObject;   // Vision blocking overlay
    public BeeScript enemyScript;           // Enemy's BeeScript

    [Header("Settings")]
    public float blockDuration = 1.5f;   // Optional auto-unblock
    public float cooldown = 1f;          // Prevent repeated triggers
    public float stunDuration = 3f;      // How long enemy stops attacking

    private bool onCooldown = false;
    private bool playerInside = false;

    private FaeLightAbility playerFaeLight;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || onCooldown) return;

        playerInside = true;

        // Get reference to player's FaeLightAbility
        playerFaeLight = other.GetComponent<FaeLightAbility>();

        // Start vision block
        if (globalVolumeObject != null)
            globalVolumeObject.SetActive(true);

        StartCoroutine(BlockDuration());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerFaeLight = null;
        }
    }

    private void Update()
    {
        if (!playerInside || playerFaeLight == null) return;

        // If player activates FaeLight
        if (playerFaeLight.IsLightActive())
        {
            RemoveBlockAndStunEnemy();
        }
    }

    private IEnumerator BlockDuration()
    {
        yield return new WaitForSeconds(blockDuration);

        if (globalVolumeObject != null)
            globalVolumeObject.SetActive(false);
    }

    private void RemoveBlockAndStunEnemy()
    {
        // Remove vision block immediately
        if (globalVolumeObject != null)
            globalVolumeObject.SetActive(false);

        // Stop enemy from attacking temporarily
        if (enemyScript != null)
        {
            enemyScript.enabled = false; // Disable entire script
            StartCoroutine(RestoreEnemyAfterStun(enemyScript, stunDuration));
        }

        // Prevent multiple triggers while inside
        playerInside = false;
        onCooldown = true;
        StartCoroutine(ResetCooldown());
    }

    private IEnumerator RestoreEnemyAfterStun(BeeScript script, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (script != null)
            script.enabled = true;
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}