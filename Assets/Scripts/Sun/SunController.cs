using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SunController : MonoBehaviour
{
    [SerializeField] UIHandler uiHandler;
    [SerializeField] Volume postProcessingVolume;
    [SerializeField] AnimationCurve sunIntensityAnimationCurve;
    [SerializeField] AnimationCurve ColorAdjustmentAnimationCurve;
    [SerializeField] Color sunRiseColor;
    [SerializeField] Color sunSetColor;

    bool dayTimeAdjustmentIsDone = false;
    float dayTime = -1;
    ColorAdjustments colorAdjustments;
    Light sun;

    Vector3 DEFAULT_ROTATION;
    float DEFAULT_INTENSITY;
    Color DEFAULT_COLOR;
    float SUN_ROTATION_SPEED = 2.5f;
    float SUN_ROTATION_LOCK_LIMIT = 0.5f;
    float SUN_INTENSITY_SPEED = 1.5f;
    float SUN_INTENSITY_LOCK_LIMIT = 0.01f;
    float SUN_COLOR_ADJUSTMENT_SPEED = 2f;
    float SUN_COLOR_LOCK_LIMIT = 0.005f;
    float DEFAULT_SHADOW_STRENGTH;
    float DEFAULT_POST_EXPOSURE;

    #region DEBUG *********************************
    #if UNITY_EDITOR
        [Header("Debug Monitor")]
        [SerializeField] float PostExposure;
        [SerializeField] float DayTime;
    #endif
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        sun = GetComponent<Light>();

        postProcessingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        DEFAULT_POST_EXPOSURE = colorAdjustments.postExposure.value;

        DEFAULT_ROTATION = transform.rotation.eulerAngles;
        DEFAULT_INTENSITY = sun.intensity;
        DEFAULT_COLOR = sun.color;

        DEFAULT_SHADOW_STRENGTH = sun.shadowStrength;
    }

    // Update is called once per frame
    void Update()
    {
        float target_day_time = uiHandler.dayTimeSlider.value;

        if(!dayTimeAdjustmentIsDone)
        {
            /* update sun rotation --> return true if rotation is adjusted */
            dayTimeAdjustmentIsDone = UpdateSunRotation(target_day_time);

            /* update sun intensity --> return true if sun intensity is adjusted */
            dayTimeAdjustmentIsDone &= UpdateSunIntensity(target_day_time);

            /* update sun color and post processing --> return true if sun color is adjusted */
            dayTimeAdjustmentIsDone &= UpdateSunColor(target_day_time);
        }

        if (!Helper.ValueInTargetTolerance(dayTime, target_day_time, 0.01f))
        {
            dayTime = target_day_time;
            dayTimeAdjustmentIsDone = false;
        }

        float rain_strength = 0;
        if (CloudController.instance.cloudsEnabled)
            rain_strength = uiHandler.rainStrengthSlider.value;

        float target_post_exposure = DEFAULT_POST_EXPOSURE - Unity.Mathematics.math.remap(0, 1, 0, 1.3f, rain_strength);
        colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, target_post_exposure, Time.deltaTime * 0.8f);

        float target_shadow_strength = DEFAULT_SHADOW_STRENGTH - rain_strength * 0.6f;
        sun.shadowStrength = Mathf.Lerp(sun.shadowStrength, target_shadow_strength, Time.deltaTime * 0.8f);

        #region DEBUG *********************************
        #if UNITY_EDITOR
            DayTime = dayTime;
            PostExposure = colorAdjustments.postExposure.value;
        #endif
        #endregion
    }

    bool UpdateSunRotation(float day_time)
    {
        float target_angle = DEFAULT_ROTATION.x - ((day_time - 12) / 24) * 300;

        if (Helper.ValueInTargetTolerance(transform.rotation.eulerAngles.x, target_angle, SUN_ROTATION_LOCK_LIMIT))
        {
            transform.rotation = Quaternion.Euler(target_angle, DEFAULT_ROTATION.y, DEFAULT_ROTATION.z);
            return true;
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(target_angle, DEFAULT_ROTATION.y, DEFAULT_ROTATION.z), SUN_ROTATION_SPEED * Time.deltaTime);
            return false;
        }
    }

    bool UpdateSunIntensity(float day_time)
    {
        float target_intensity = DEFAULT_INTENSITY * sunIntensityAnimationCurve.Evaluate(day_time);

        if (Helper.ValueInTargetTolerance(sun.intensity, target_intensity, SUN_INTENSITY_LOCK_LIMIT))
        {
            sun.intensity = target_intensity;
            return true;
        }
        else
        {
            sun.intensity = Mathf.Lerp(sun.intensity, target_intensity, SUN_INTENSITY_SPEED * Time.deltaTime);
            return false;
        }
    }

    bool UpdateSunColor(float day_time)
    {
        bool sun_color_adj_is_done = false;
        bool post_proc_adj_is_done = false;

        float am_influence = day_time / 12;
        float pm_influence = (24 - day_time) / 12;

        float r;
        float g;
        float b;

        /* color adjustment of sun ***************************/
        if (day_time < 12)
        {
            r = (DEFAULT_COLOR.r * am_influence) + (sunRiseColor.r - (sunRiseColor.r * am_influence));
            g = (DEFAULT_COLOR.g * am_influence) + (sunRiseColor.g - (sunRiseColor.g * am_influence));
            b = (DEFAULT_COLOR.b * am_influence) + (sunRiseColor.b - (sunRiseColor.b * am_influence));
        }
        else
        {
            r = (DEFAULT_COLOR.r * pm_influence) + (sunSetColor.r - (sunSetColor.r * pm_influence));
            g = (DEFAULT_COLOR.g * pm_influence) + (sunSetColor.g - (sunSetColor.g * pm_influence));
            b = (DEFAULT_COLOR.b * pm_influence) + (sunSetColor.b - (sunSetColor.b * pm_influence));
        }
        if (Helper.ValueInTargetTolerance(sun.color.r, r, SUN_COLOR_LOCK_LIMIT) && 
            Helper.ValueInTargetTolerance(sun.color.g, g, SUN_COLOR_LOCK_LIMIT) &&
            Helper.ValueInTargetTolerance(sun.color.b, b, SUN_COLOR_LOCK_LIMIT))
        {
            sun.color = new Color(r, g, b, 1.0f);
            sun_color_adj_is_done = true;
        }
        else
        {
            sun.color = Color.Lerp(sun.color, new Color(r, g, b, 1.0f), SUN_COLOR_ADJUSTMENT_SPEED * Time.deltaTime);
        }

        /* color adjustment of post processing ***************/
        float color_adj_strength = ColorAdjustmentAnimationCurve.Evaluate(day_time);
        if (day_time < 12)
        {
            r = am_influence + (sunRiseColor.r * color_adj_strength - (sunRiseColor.r * color_adj_strength * am_influence));
            g = am_influence + (sunRiseColor.g * color_adj_strength - (sunRiseColor.g * color_adj_strength * am_influence));
            b = am_influence + (sunRiseColor.b * color_adj_strength - (sunRiseColor.b * color_adj_strength * am_influence));
        }
        else
        {
            r = pm_influence + (sunSetColor.r * color_adj_strength - (sunSetColor.r * color_adj_strength * pm_influence));
            g = pm_influence + (sunSetColor.g * color_adj_strength - (sunSetColor.g * color_adj_strength * pm_influence));
            b = pm_influence + (sunSetColor.b * color_adj_strength - (sunSetColor.b * color_adj_strength * pm_influence));
        }
        if (Helper.ValueInTargetTolerance(colorAdjustments.colorFilter.value.r, r, SUN_COLOR_LOCK_LIMIT) &&
            Helper.ValueInTargetTolerance(colorAdjustments.colorFilter.value.g, g, SUN_COLOR_LOCK_LIMIT) &&
            Helper.ValueInTargetTolerance(colorAdjustments.colorFilter.value.b, b, SUN_COLOR_LOCK_LIMIT))
        {
            colorAdjustments.colorFilter.value = new Color(r, g, b, 1.0f);
            post_proc_adj_is_done = true;
        }
        else
        {
            colorAdjustments.colorFilter.value = Color.Lerp(colorAdjustments.colorFilter.value, new Color(r, g, b, 1.0f), SUN_COLOR_ADJUSTMENT_SPEED * Time.deltaTime);
        }

        return sun_color_adj_is_done & post_proc_adj_is_done;
    }
}
