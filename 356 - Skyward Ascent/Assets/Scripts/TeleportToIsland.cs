using UnityEngine;
using System.Collections;

public class TeleportToIsland : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportDestination;
    public string playerTag = "Player";

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTeleporting && other.CompareTag(playerTag))
        {
            StartCoroutine(TeleportWithFade(other.gameObject));
        }
    }

    private IEnumerator TeleportWithFade(GameObject player)
    {
        isTeleporting = true;

        // Fade out
        yield return StartCoroutine(FadeScreen(1f)); // Fade to black

        // Teleport player
        player.transform.position = teleportDestination.position;
        player.transform.rotation = teleportDestination.rotation;

        // Fade in
        yield return StartCoroutine(FadeScreen(0f)); // Fade to clear

        isTeleporting = false;
    }

    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}