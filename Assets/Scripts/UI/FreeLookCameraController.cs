using UnityEngine;

public class FreeLookCameraController
{
    private bool looking = false;

    private float MOVEMENT_SPEED = 25f;
    private float FAST_MOVEMENT_SPEED = 80f;
    private float FREE_LOOK_SENSITIVITY = 2.5f;

    public void UpdateCameraMovement(Transform cam_transform, Camera cam, float delta_time, float distance_to_terrain, float perspective_fov)
    {
        /* FOV setting **************************************************************************************/
        int layerMask = 1 << 6;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        float distance = distance_to_terrain - 0.1f;
        if (Physics.Raycast(cam_transform.position, ray.direction, out hit, distance_to_terrain, layerMask))
        {
            distance = Mathf.Clamp(Vector3.Distance(cam_transform.localPosition, hit.point), 20, distance_to_terrain);
        }
        float fov_increase = distance_to_terrain / distance;
        if (fov_increase < 1)
            fov_increase = 0;
        else
            fov_increase *= 3f;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, perspective_fov + fov_increase, 1.2f * Time.deltaTime);

        /* Camera speed setting **************************************************************************************/
        bool fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float movementSpeed;
        if (fastMode)
            movementSpeed = FAST_MOVEMENT_SPEED;
        else
            movementSpeed = MOVEMENT_SPEED;
        movementSpeed *= Mathf.Clamp(distance / 50f, 0.5f, 1);

        /* Camera position setting **************************************************************************************/
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            cam_transform.position = cam_transform.position + (-cam_transform.right * movementSpeed * delta_time);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            cam_transform.position = cam_transform.position + (cam_transform.right * movementSpeed * delta_time);
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            cam_transform.position = cam_transform.position + (cam_transform.forward * movementSpeed * delta_time);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            cam_transform.position = cam_transform.position + (-cam_transform.forward * movementSpeed * delta_time);
        }

        /* Camera orientation setting **************************************************************************************/
        if (looking)
        {
            float newRotationX = cam_transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FREE_LOOK_SENSITIVITY;
            float newRotationY = cam_transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FREE_LOOK_SENSITIVITY;
            cam_transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartLooking();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopLooking();
        }
    }

    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
