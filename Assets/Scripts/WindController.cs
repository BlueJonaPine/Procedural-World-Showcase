using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindController : MonoBehaviour
{
    public static WindController instance;

    [Header("Wind Shader")]
    public Material windVegetationMaterial = null;

    [HideInInspector]
    public Vector3 actualWindDirection = Vector3.zero;
    [HideInInspector]
    public float actualWindStrength = 0;

    bool windDirectionChanged = false;
    bool windStrengthChanged = false;

    const float WIND_DIRECTION_ADAPTION_SPEED = 2.5f;
    const float WIND_STRENGTH_ADAPTION_SPEED = 1f;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Instance of WindController already exists in scene!", this);
            this.enabled = false; //Disable instead of destroy so you can see it in inspector and stop whatever has 2 of them
            return;
        }
        instance = this;
    }

    void Update()
    {
        Vector3 targetWindDirection = WindDirectionSliderToDirection();
        float targetWindStrength = UIHandler.instance.windStrengthSlider.value;

        /* Depending on the target wind direction, adapt the actual wind direction in the shader */
        if (Vector3.Distance(actualWindDirection, targetWindDirection) > 0.01f)
        {
            actualWindDirection = Vector3.Lerp(actualWindDirection, targetWindDirection, WIND_DIRECTION_ADAPTION_SPEED * Time.deltaTime);
            windDirectionChanged = true;
        }
        else
        {
            actualWindDirection = targetWindDirection;
        }
        if (windDirectionChanged)
        {
            windVegetationMaterial.SetVector(Shader.PropertyToID("_WindDirection"), actualWindDirection);
            windDirectionChanged = false;
        }

        /* Depending on the target wind strength, adapt the actual wind strength in the shader */
        if (!Helper.ValueInTargetTolerance(actualWindStrength, targetWindStrength, 0.01f))
        {
            actualWindStrength = Mathf.Lerp(actualWindStrength, targetWindStrength, WIND_STRENGTH_ADAPTION_SPEED * Time.deltaTime);
            windStrengthChanged = true;
        }
        else
        {
            actualWindStrength = targetWindStrength;
        }
        if(windStrengthChanged)
        {
            windVegetationMaterial.SetFloat(Shader.PropertyToID("_WindStrength"), actualWindStrength);
            windStrengthChanged = false;
        }
    }

    private Vector3 WindDirectionSliderToDirection()
    {
        Vector3 direction = Vector3.zero;
        float degree = -UIHandler.instance.windDirectionSlider.value * 360f;
        direction.x = Mathf.Sin(degree * Mathf.Deg2Rad);
        direction.z = -Mathf.Cos(degree * Mathf.Deg2Rad);

        return direction;
    }
}
