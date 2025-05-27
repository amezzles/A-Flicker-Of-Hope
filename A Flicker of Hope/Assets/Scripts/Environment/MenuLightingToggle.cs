using UnityEngine;
using TMPro;
using System.Collections;

public class MenuLightingToggle : MonoBehaviour
{
    public Material naturalSky;
    public Material corruptSky;
    public Light directionalLight;
    public TMP_Text[] gradientTexts;

    private bool usingCorruptSky = false;

    void Start()
    {
        RenderSettings.fog = true;
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 5f));
            ToggleLighting();

            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f)); 
            ToggleLighting();
        }
    }

    void ToggleLighting()
    {
        usingCorruptSky = !usingCorruptSky;

        // Skybox
        RenderSettings.skybox = usingCorruptSky ? corruptSky : naturalSky;

        // Fog
        RenderSettings.fogDensity = usingCorruptSky ? 0.033f : 0f;

        // Directional light color
        if (directionalLight != null)
        {
            directionalLight.color = usingCorruptSky
                ? new Color32(0x8E, 0x18, 0x94, 0xFF)
                : Color.white;
        }

        // Text gradient
        foreach (TMP_Text text in gradientTexts)
        {
            VertexGradient gradient = usingCorruptSky
                ? new VertexGradient(
                    new Color32(0xDF, 0x9A, 0xDD, 0xFF), // corrupt top
                    new Color32(0xDF, 0x9A, 0xDD, 0xFF),
                    new Color32(0x63, 0x28, 0x6B, 0xFF), // corrupt bottom
                    new Color32(0x63, 0x28, 0x6B, 0xFF))
                : new VertexGradient(
                    new Color32(0xFF, 0xD7, 0x00, 0xFF),
                    new Color32(0xFF, 0xD7, 0x00, 0xFF),
                    new Color32(0xF5, 0xC3, 0x00, 0xFF), 
                    new Color32(0xF5, 0xC3, 0x00, 0xFF));


            text.colorGradient = gradient;
        }

        DynamicGI.UpdateEnvironment();
    }
}
