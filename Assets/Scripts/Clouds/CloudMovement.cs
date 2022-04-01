using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    Vector3 windDirectionBuffer = Vector3.zero;
    
    const float MOVEMENT_SPEED = 0.5f;
    float MOVEMENT_SPEED_DEVIATION;
    
    private void Start()
    {
        MOVEMENT_SPEED_DEVIATION = 0.4f / transform.localScale.y;

        windDirectionBuffer = WindController.instance.actualWindDirection;
    }

    void Update()
    {
        transform.Translate(WindController.instance.actualWindDirection * (WindController.instance.actualWindStrength * 2f + 1f) * (MOVEMENT_SPEED + MOVEMENT_SPEED_DEVIATION) * Time.deltaTime, Space.World);

        /* destroy the cloud if it was hidden and the direction changed */
        if (windDirectionBuffer != WindController.instance.actualWindDirection)
        {
            windDirectionBuffer = WindController.instance.actualWindDirection;

            if(!transform.GetComponent<MeshRenderer>().enabled)
                Destroy(transform.gameObject);
        }

        /* destroy the cloud if it is too far away from the terrain */
        if (Vector3.Distance(transform.position, Globals.instance.terrainCenter) > Globals.instance.terrainSize * 2)
            Destroy(transform.gameObject);
    }
}
