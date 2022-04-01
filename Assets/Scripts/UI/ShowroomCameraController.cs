using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowroomCameraController
{
    Vector3 panAroundPosition;

    float mouseButtonDownTime = 0;
    Vector2 previousMousePosition = Vector2.zero;

    bool mouseButtonIsHeldDown = false;
    bool mouseButtonIsAtTarget = false;

    float distanceToTarget;

    MouseCaster mouseCaster = new MouseCaster();

    const float MOUSE_BUTTON_DOWN_TRIGGER_TIME = 0.08f;
    const float PAN_SPEED = 0.3f;
    const float CAMERA_MIN_X_ANGLE = -20f;
    const float ZOOM_SPEED = 12;

    public ShowroomCameraController(Vector3 center_position, float distance_to_target)
    {
        panAroundPosition = center_position;
        distanceToTarget = distance_to_target;
    }

    public void UpdateCameraMovement(Camera camera, float delta_time)
    {
        HandleMouseInput(delta_time);

        if (mouseButtonIsHeldDown)
        {
            Cursor.visible = false;

            MoveCamera(camera.transform);
        }
        else
        {
            Cursor.visible = true;

            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (Input.GetAxis("Mouse ScrollWheel") > 0 && !isOverUI)
            {
                camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * ZOOM_SPEED;
                if (camera.orthographicSize < 15)
                    camera.orthographicSize = 15;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && !isOverUI)
            {
                camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * ZOOM_SPEED;
                if (camera.orthographicSize > 40)
                    camera.orthographicSize = 40;
            }
        }

        /* always update the previous mouse position --> prevents "jumping" camera */
        previousMousePosition.Set(Input.mousePosition.x, Input.mousePosition.y);
    }

    private void HandleMouseInput(float delta_time)
    {
        if (Input.GetMouseButton(0))
        {
            /* only perform this once when mouse button is held down initially */
            if (!mouseButtonIsAtTarget)
            {
                mouseCaster.CastFromMousePosition(new int[1] { 6 }, false, 800);
                bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

                /* check if mouse is over terrain */
                if (mouseCaster.layerIsHit && !isOverUI)
                    mouseButtonIsAtTarget = true;
                else
                    mouseButtonIsAtTarget = false;
            }
            else
            {
                /* mouse button is held down after defined duration */
                mouseButtonDownTime += delta_time;
                if (mouseButtonDownTime >= MOUSE_BUTTON_DOWN_TRIGGER_TIME)
                    mouseButtonIsHeldDown = true;
                else
                    mouseButtonIsHeldDown = false;
            }
        }
        else
        {
            mouseButtonDownTime = 0;
            mouseButtonIsHeldDown = false;
            mouseButtonIsAtTarget = false;
            mouseCaster.layerIsHit = false;
        }
    }


    private void MoveCamera(Transform camera)
    {
        Vector2 mouse_difference = new Vector2(Input.mousePosition.x - previousMousePosition.x, Input.mousePosition.y - previousMousePosition.y);

        float rotationAroundYAxis = mouse_difference.x * PAN_SPEED;
        float rotationAroundXAxis = -mouse_difference.y * PAN_SPEED;

        camera.position = panAroundPosition;

        camera.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
        camera.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

        float x_rotation = camera.rotation.eulerAngles.x;
        if (x_rotation >= 180)
            x_rotation = -1 * (360 - camera.rotation.eulerAngles.x);

        if (x_rotation < CAMERA_MIN_X_ANGLE)
            camera.rotation = Quaternion.Euler(CAMERA_MIN_X_ANGLE, camera.rotation.eulerAngles.y, camera.rotation.eulerAngles.z);

        camera.Translate(new Vector3(0, 0, -distanceToTarget));
    }

    public void MoveCameraHorizontally(Transform camera, float y_value, float offset)
    {
        float rotationAroundYAxis = y_value;

        camera.position = panAroundPosition;

        //camera.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);
        camera.rotation = Quaternion.Euler(camera.rotation.eulerAngles.x, offset + y_value, camera.rotation.eulerAngles.z);

        camera.Translate(new Vector3(0, 0, -distanceToTarget));
    }
}
