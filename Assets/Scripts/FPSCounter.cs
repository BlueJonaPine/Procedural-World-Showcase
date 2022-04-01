using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    Text fpsText;

    KeyCode activationKey = KeyCode.F1;
    int frameCounter = 0;
    float fpsUpdateTime = 0;

    float fpsUpdateInterval = 0;
    const float FPS_REGULAR_UPDATE_INTERVAL = 0.35f;

    private void Start()
    {
        fpsText = GetComponent<Text>();
    }

    void Update()
    {
        /* Check if FPS Counter must be displayed or not */
        if (Input.GetKeyDown(activationKey))
        {
            fpsText.enabled = !fpsText.enabled;
            fpsText.text = "0 FPS";
            /* If the FPS Counter must be displayed then show the first value without mean value generation */
            fpsUpdateInterval = 0;
        }

        if (fpsText.enabled)
        {
            frameCounter++;
            fpsUpdateTime += Time.deltaTime;

            if (fpsUpdateTime >= fpsUpdateInterval)
            {
                fpsText.text = ((int)(frameCounter / fpsUpdateTime)).ToString() + " FPS";

                /* Reset the variables for the mean value of FPS */
                fpsUpdateInterval = FPS_REGULAR_UPDATE_INTERVAL;
                fpsUpdateTime = 0;
                frameCounter = 0;
            }
        }
    }
}
