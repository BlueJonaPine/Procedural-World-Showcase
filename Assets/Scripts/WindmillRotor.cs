using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillRotor : MonoBehaviour
{
    Transform rotorTransform = null;
    float actualSpeed = 0;

    float RANDOM_SPEED_DEVIATION;
    const float SPEED_ADAPTION_SPEED = 0.55f;

    #region DEBUG *********************************
    #if UNITY_EDITOR
    [Header("Debug Monitor")]
        [SerializeField] float ActualSpeed;
    #endif
    #endregion

    void Start()
    {
        rotorTransform = transform.GetChild(0);
        RANDOM_SPEED_DEVIATION = Random.Range(0.9f, 1.1f);
    }

    void Update()
    {
        float target_speed = UIHandler.instance.windStrengthSlider.value * 1.2f * RANDOM_SPEED_DEVIATION;

        if (Helper.ValueInTargetTolerance(actualSpeed, target_speed, 0.01f))
            actualSpeed = target_speed;
        else
            actualSpeed = Mathf.Lerp(actualSpeed, target_speed, SPEED_ADAPTION_SPEED * Time.deltaTime);

        rotorTransform.Rotate(new Vector3(0, actualSpeed, 0), Space.Self);

        #if UNITY_EDITOR
            ActualSpeed = actualSpeed;
        #endif
    }
}
