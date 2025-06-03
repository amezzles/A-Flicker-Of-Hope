using UnityEngine;
using System.Collections;

public class EnvironmentLighting : MonoBehaviour
{
    [Header("Skyboxes")]
    public Material naturalSky;
    public Material corruptSky;

    [Header("Lighting")]
    public Light directionalLight;

    [Header("Transition Settings")]
    public float transitionDuration = 4f;

    private bool usingCorruptSky = true;

    void Start()
    {
        //enable fog and set initial lighting
        RenderSettings.fog = true;
        ApplyCorruptLighting();
    }

    void Update()
    {
        ////temp
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    ToggleLighting();
        //}
    }

    public void ToggleLighting()
    {
        usingCorruptSky = !usingCorruptSky;

        if (usingCorruptSky)
        {
            ApplyCorruptLighting();
        }
        else
        {
            ApplyNaturalLighting();
        }
    }

    private void ApplyCorruptLighting()
    {
        RenderSettings.skybox = corruptSky;
        RenderSettings.fogDensity = 0.033f;

        if (directionalLight != null)
            directionalLight.color = new Color32(0x8E, 0x18, 0x94, 0xFF);

        DynamicGI.UpdateEnvironment();
    }

    private void ApplyNaturalLighting()
    {
        RenderSettings.skybox = naturalSky;
        RenderSettings.fogDensity = 0f;

        if (directionalLight != null)
            directionalLight.color = Color.white;

        DynamicGI.UpdateEnvironment();
    }


    //called by Timeline signal to start smooth transition to natural lighting
    public void StartNaturalLightingTransition()
    {
        StartCoroutine(TransitionToNaturalLighting());
    }

    private IEnumerator TransitionToNaturalLighting()
    {
        float time = 0f;

        Color startColor = directionalLight.color;
        Color targetColor = Color.white;

        float startFog = RenderSettings.fogDensity;
        float targetFog = 0f;

        Material startSkybox = RenderSettings.skybox;
        Material targetSkybox = naturalSky;

        RenderSettings.skybox = targetSkybox;

        while (time < transitionDuration)
        {
            float t = time / transitionDuration;

            if (directionalLight != null)
                directionalLight.color = Color.Lerp(startColor, targetColor, t);

            RenderSettings.fogDensity = Mathf.Lerp(startFog, targetFog, t);

            time += Time.deltaTime;
            yield return null;
        }

        if (directionalLight != null)
            directionalLight.color = targetColor;

        RenderSettings.fogDensity = targetFog;
        RenderSettings.skybox = targetSkybox;

        DynamicGI.UpdateEnvironment();
    }
}
