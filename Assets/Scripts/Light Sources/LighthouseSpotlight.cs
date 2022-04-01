using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LighthouseSpotlight : MonoBehaviour
{
    [SerializeField] AnimationCurve lightIntensity;

    Light light1;

    float DEFAULT_LIGHT_INTENSITY;
    const float SWAY_SPEED = 10f;
    const float INTENSITY_ADJUSTMENT_SPEED = 0.6f;

    // Start is called before the first frame update
    void Start()
    {
        light1 = GetComponent<Light>();
        DEFAULT_LIGHT_INTENSITY = light1.intensity;

        light1.intensity = DEFAULT_LIGHT_INTENSITY * lightIntensity.Evaluate(UIHandler.instance.dayTimeSlider.value);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, SWAY_SPEED * Time.deltaTime, 0));

        light1.intensity = Mathf.Lerp(light1.intensity, DEFAULT_LIGHT_INTENSITY * lightIntensity.Evaluate(UIHandler.instance.dayTimeSlider.value), Time.deltaTime * INTENSITY_ADJUSTMENT_SPEED); ;
    }
}
