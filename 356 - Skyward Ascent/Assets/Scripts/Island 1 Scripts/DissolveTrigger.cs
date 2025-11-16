using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class DissolveTrigger : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public Renderer[] targetRenderers;        // Multiple objects to dissolve
    public Material dissolveMaterial;         // Material with dissolve shader
    public string dissolveProperty = "dissolve";
    public float dissolveDuration = 2f;
    public bool destroyAfterDissolve = false;

    [Header("Audio Settings")]
    public AudioClip cageDissolveSound;
    [Range(0f, 1f)]
    public float dissolveVolume = 1f;

    private bool isDissolving = false;

    // Call this from RunestoneManager or anywhere
    public void StartDissolve()
    {
        if (!isDissolving)
        {
            StartCoroutine(DissolveRoutine());
        }
    }

    private IEnumerator DissolveRoutine()
    {
        isDissolving = true;

        // Play dissolve sound
        PlayDissolveSound();

        // Create material instances for all renderers
        Material[] materials = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null && dissolveMaterial != null)
            {
                materials[i] = new Material(dissolveMaterial);
                targetRenderers[i].material = materials[i];
            }
        }

        float time = 0f;
        while (time < dissolveDuration)
        {
            float value = Mathf.Lerp(0f, 1f, time / dissolveDuration);
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty(dissolveProperty))
                {
                    materials[i].SetFloat(dissolveProperty, value);
                }
            }
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure final state
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
                materials[i].SetFloat(dissolveProperty, 1f);

            if (destroyAfterDissolve && targetRenderers[i] != null)
                Destroy(targetRenderers[i].gameObject);
        }

        isDissolving = false;
    }

    private void PlayDissolveSound()
    {
        if (cageDissolveSound != null)
        {
            // Play at camera position to ensure it's heard
            Vector3 playPosition = Camera.main.transform.position;
            AudioSource.PlayClipAtPoint(cageDissolveSound, playPosition, dissolveVolume);
            Debug.Log("cage dissolve sound played at position: " + transform.position);
        }
        else
        {
            Debug.LogWarning("Dissolve sound or AudioSource not set up properly!", this);
        }
    }
}