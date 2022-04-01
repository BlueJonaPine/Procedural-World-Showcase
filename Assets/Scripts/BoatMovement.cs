using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    [SerializeField] float INTENSITY_ADJUSTMENT_ROTATION = 0.6f;

    Vector3 DEFAULT_ROTATION;
    float MAX_ROTATION_ANGLE;

    // Start is called before the first frame update
    void Start()
    {
        DEFAULT_ROTATION = transform.rotation.eulerAngles;
        MAX_ROTATION_ANGLE = Random.Range(4, 8);
    }

    // Update is called once per frame
    void Update()
    {
        float x_rotation = Mathf.Sin(INTENSITY_ADJUSTMENT_ROTATION * Time.time) * Mathf.Sin(INTENSITY_ADJUSTMENT_ROTATION * INTENSITY_ADJUSTMENT_ROTATION * Time.time) * MAX_ROTATION_ANGLE;
        float z_rotation = Mathf.Cos(INTENSITY_ADJUSTMENT_ROTATION * Time.time) * Mathf.Cos(INTENSITY_ADJUSTMENT_ROTATION * INTENSITY_ADJUSTMENT_ROTATION * Time.time) * MAX_ROTATION_ANGLE;

        transform.rotation = Quaternion.Euler(DEFAULT_ROTATION.x + x_rotation, DEFAULT_ROTATION.y, DEFAULT_ROTATION.z + z_rotation);
    }
}
