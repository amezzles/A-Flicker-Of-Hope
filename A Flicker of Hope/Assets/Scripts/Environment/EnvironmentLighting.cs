using UnityEngine;

public class SkyboxToggle : MonoBehaviour
{
    public Material naturalSky;
    public Material corruptSky;
    public Light directionalLight;

    private bool usingCorruptSky = false;

    void Start()
    {
        //enable fog
        RenderSettings.fog = true;
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

        //sky material
        RenderSettings.skybox = usingCorruptSky ? corruptSky : naturalSky;

        //fog density
        RenderSettings.fogDensity = usingCorruptSky ? 0.033f : 0f;

        //directional light color
        if (directionalLight != null)
        {
            directionalLight.color = usingCorruptSky
                ? new Color32(0x8E, 0x18, 0x94, 0xFF)
                : Color.white; 
        }

        //refresh global illumination environment
        DynamicGI.UpdateEnvironment();
    }
}