using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudRain : MonoBehaviour
{
    ParticleSystem rainParticleSystem;
    MeshRenderer meshRenderer;

    float MAXIMUM_RAIN_RATE_OVER_TIME;

    void Start()
    {
        rainParticleSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
        meshRenderer = GetComponent<MeshRenderer>();
        MAXIMUM_RAIN_RATE_OVER_TIME = rainParticleSystem.emission.rateOverTime.constant;
    }

    void Update()
    {
        bool emmit_rain;
        float rain_strength = CloudController.instance.rainStrength;
        ParticleSystem.EmissionModule emmission_module = rainParticleSystem.emission;

        /* Depending on the mesh renderer (if the cloud is rendered or not), check if emission must be en- or disabled */
        emmit_rain = meshRenderer.enabled;
        /* Depending on the rain strength, check if emission must be en- or disabled */
        if (Mathf.Approximately(rain_strength, 0))
            emmit_rain = false;
        else
            emmit_rain &= true;

        /* en-/disable the rain and adjust the density of the rain */
        if (emmit_rain)
        {
            if (!emmission_module.enabled)
                emmission_module.enabled = true;

            emmission_module.rateOverTime = MAXIMUM_RAIN_RATE_OVER_TIME * rain_strength;
        }
        else
        {
            if (emmission_module.enabled)
                emmission_module.enabled = false;
        }
    }
}
