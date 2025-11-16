using UnityEngine;
using System.Collections;

public class PlayerHurtEffect : MonoBehaviour
{
    [Header("Hurt Effect Settings")]
    public float flashDuration = 0.2f;
    public Color hurtColor = Color.red;

    private Material[] originalMaterials;
    private Renderer playerRenderer;
    private bool isFlashing = false;

    void Start()
    {
        playerRenderer = GetComponentInChildren<Renderer>();

        if (playerRenderer != null)
        {
            // ✅ FIX: Create copies of materials to avoid shared material issues
            originalMaterials = new Material[playerRenderer.materials.Length];
            for (int i = 0; i < playerRenderer.materials.Length; i++)
            {
                originalMaterials[i] = new Material(playerRenderer.materials[i]);
            }
        }
        else
        {
            Debug.LogError("Player Renderer not found for hurt effect!");
        }
    }

    public void TriggerHurtEffect()
    {
        if (playerRenderer != null && !isFlashing)
        {
            StartCoroutine(HurtFlash());
        }
    }

    private IEnumerator HurtFlash()
    {
        isFlashing = true;

        // Change to hurt color
        SetMaterialColor(hurtColor);

        // Wait for flash duration
        yield return new WaitForSeconds(flashDuration);

        // Return to original color
        ResetMaterialColor();

        isFlashing = false;
    }

    private void SetMaterialColor(Color color)
    {
        if (playerRenderer != null)
        {
            // ✅ FIX: Change color on all materials
            foreach (Material mat in playerRenderer.materials)
            {
                mat.color = color;
            }
        }
    }

    private void ResetMaterialColor()
    {
        if (playerRenderer != null && originalMaterials != null)
        {
            // ✅ FIX: Restore original materials completely
            for (int i = 0; i < playerRenderer.materials.Length; i++)
            {
                if (i < originalMaterials.Length)
                {
                    playerRenderer.materials[i].color = originalMaterials[i].color;
                }
            }
        }
    }

    // ✅ ADD THIS: Clean up when destroyed
    private void OnDestroy()
    {
        if (originalMaterials != null)
        {
            foreach (Material mat in originalMaterials)
            {
                if (mat != null)
                    DestroyImmediate(mat);
            }
        }
    }
}