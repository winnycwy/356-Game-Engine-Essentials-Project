using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHurtEffect : MonoBehaviour
{
    [Header("UI Hurt Effect Settings")]
    public Image hurtOverlay;
    public float flashDuration = 0.3f;
    public float maxAlpha = 0.3f;

    private bool isFlashing = false;

    void Start()
    {
        // Make sure the overlay starts invisible
        if (hurtOverlay != null)
        {
            hurtOverlay.color = new Color(1, 0, 0, 0);
        }
    }

    public void TriggerHurtEffect()
    {
        if (hurtOverlay != null && !isFlashing)
        {
            StartCoroutine(HurtFlash());
        }
    }

    private IEnumerator HurtFlash()
    {
        isFlashing = true;

        // Fade in (red appears quickly)
        float elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            float alpha = Mathf.Lerp(0, maxAlpha, elapsed / (flashDuration / 2));
            hurtOverlay.color = new Color(1, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out (red disappears slowly)
        elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            float alpha = Mathf.Lerp(maxAlpha, 0, elapsed / (flashDuration / 2));
            hurtOverlay.color = new Color(1, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it's completely invisible
        hurtOverlay.color = new Color(1, 0, 0, 0);
        isFlashing = false;
    }
}