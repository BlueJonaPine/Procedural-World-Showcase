using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointlight : MonoBehaviour
{
    Light pointlight;

    [SerializeField] AnimationCurve lightIntensity;

    [SerializeField] float INTENSITY_ADJUSTMENT_SPEED = 3f;
    float DEFAULT_LIGHT_INTENSITY;

    // Start is called before the first frame update
    void Start()
    {
        pointlight = GetComponent<Light>();
        DEFAULT_LIGHT_INTENSITY = pointlight.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        pointlight.intensity = ((Mathf.Cos(INTENSITY_ADJUSTMENT_SPEED * Time.time) + 1) / 2) * DEFAULT_LIGHT_INTENSITY * lightIntensity.Evaluate(UIHandler.instance.dayTimeSlider.value);
    }
}
