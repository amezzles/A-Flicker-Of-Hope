using UnityEngine;

public class EnvironmentLighting : MonoBehaviour
{
    public Material naturalSky;
    public Material corruptSky;
    public Light directionalLight;

    private bool usingCorruptSky = true;

    void Start()
    {
        //enable fog
        RenderSettings.fog = true;

        //apply initial corrupt lighting
        RenderSettings.skybox = corruptSky;
        RenderSettings.fogDensity = 0.033f;

        if (directionalLight != null)
        {
            directionalLight.color = new Color32(0x8E, 0x18, 0x94, 0xFF);
        }

        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleLighting();
        }
    }

    void ToggleLighting()
    {
        usingCorruptSky = !usingCorruptSky;

        RenderSettings.skybox = usingCorruptSky ? corruptSky : naturalSky;

        RenderSettings.fogDensity = usingCorruptSky ? 0.033f : 0f;

        if (directionalLight != null)
        {
            directionalLight.color = usingCorruptSky
                ? new Color32(0x8E, 0x18, 0x94, 0xFF)
                : Color.white;
        }

        DynamicGI.UpdateEnvironment();
    }
}
