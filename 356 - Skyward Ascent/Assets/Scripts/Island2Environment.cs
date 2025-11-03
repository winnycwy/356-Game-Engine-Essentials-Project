using UnityEngine;
public class Island2Environment : MonoBehaviour
{
    public Color darkAmbientColor = Color.gray* 0.3f;
    public Color darkFogColor = new Color(0.05f, 0.05f, 0.1f);
    public float darkFogDensity = 0.05f;

    private Color originalAmbient;
    private Color originalFogColor;
    private float originalFogDensity;

    void Start()
    {
        originalAmbient = RenderSettings.ambientLight;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.ambientLight = darkAmbientColor;
            RenderSettings.fog = true;
            RenderSettings.fogColor = darkFogColor;
            RenderSettings.fogDensity = darkFogDensity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.ambientLight = originalAmbient;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
        }
    }
}