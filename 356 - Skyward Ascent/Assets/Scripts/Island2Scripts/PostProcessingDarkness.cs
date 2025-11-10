using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingDarkness : MonoBehaviour
{
    public PostProcessVolume darkVolume;
    public float transitionDuration = 3f;

    private float initialWeight = 0f;
    private float targetWeight = 1f;
    private bool isTransitioning = false;

    public void ActivateDarkness()
    {
        if (!isTransitioning && darkVolume != null)
        {
            StartCoroutine(TransitionVolumeWeight(targetWeight));
        }
    }

    public void DeactivateDarkness()
    {
        if (!isTransitioning && darkVolume != null)
        {
            StartCoroutine(TransitionVolumeWeight(initialWeight));
        }
    }

    private System.Collections.IEnumerator TransitionVolumeWeight(float target)
    {
        isTransitioning = true;
        float timer = 0f;
        float startWeight = darkVolume.weight;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            darkVolume.weight = Mathf.Lerp(startWeight, target, timer / transitionDuration);
            yield return null;
        }

        darkVolume.weight = target;
        isTransitioning = false;
    }
}