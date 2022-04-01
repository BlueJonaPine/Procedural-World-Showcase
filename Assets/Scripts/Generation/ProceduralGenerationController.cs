using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerationController : MonoBehaviour
{
    enum States
    {
        generating,
        idling
    }
    States state = States.idling;
    bool startGeneration = false;
    float animationCameraAngle;

    const float ANIMATION_CAMERA_SPEED = 0.45f;

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case States.idling:
                if (startGeneration)
                    SwitchToGenerating();
                break;

            case States.generating:
                if (UIHandler.instance.animationToggle.isOn)
                {
                    float speed_adaption;
                    if (TerrainCreator.instance.isTerrainCreated)
                        speed_adaption = Mathf.Clamp(22f / (360 - animationCameraAngle), 1, 8);
                    else
                        speed_adaption = Mathf.Clamp((360 - animationCameraAngle) / 72f, 1, 6);
                    animationCameraAngle = Mathf.Lerp(animationCameraAngle, TerrainCreator.instance.terrainGenerationCompletion * 3.6f, speed_adaption * ANIMATION_CAMERA_SPEED * Time.deltaTime);
                    CameraRigController.instance.Force360Movement(animationCameraAngle);
                }
                if (TerrainCreator.instance.isTerrainCreated && animationCameraAngle >= 359.5f)
                    SwitchToIdling();
                break;
        }
    }

    private void SwitchToGenerating()
    {
        state = States.generating;

        /* disable UI and enable loading screen *******************************************************************/
        UIHandler.instance.EnableUI(false);
        if (!UIHandler.instance.animationToggle.isOn)
        {
            UIHandler.instance.EnableLoadingScreen(true);
            animationCameraAngle = 360;
        }
        else /* enable 360° paning camera movement *********************************************************************/
        {
            CameraRigController.instance.Enable360Movement(true);
            animationCameraAngle = 0;
        }

        /* First clear everything *********************************************************************************/
        TerrainCreator.instance.DestroyGeneratedWorld();
        /* Then generate and visualize the new terrain ************************************************************/
        TerrainCreator.instance.GenerateWorld(UIHandler.instance.animationToggle.isOn);
    }

    private void SwitchToIdling()
    {
        state = States.idling;

        startGeneration = false;

        UIHandler.instance.EnableLoadingScreen(false);
        UIHandler.instance.EnableUI(true);

        CameraRigController.instance.Enable360Movement(false);
    }

    public void OnTerrainGeneration()
    {
        startGeneration = true;
    }
}
