using UnityEngine;
using System.Collections;

public class PlayerHurtEffect : MonoBehaviour
{
    [Header("Hurt Effect Settings")]
    public float flashDuration = 0.2f;
    public Color hurtColor = Color.red;

    private Material[] originalMaterials;
    private Renderer playerRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        playerRenderer = GetComponentInChildren<Renderer>();

        if (playerRenderer != null)
        {
            // Store original materials
            originalMaterials = playerRenderer.materials;
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
            for (int i = 0; i < playerRenderer.materials.Length; i++)
            {
                playerRenderer.materials[i].color = originalMaterials[i].color;
            }
        }
    }
}