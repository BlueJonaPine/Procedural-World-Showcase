using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A simple free camera to be added to a Unity game object.
/// 
/// Keys:
///	wasd / arrows	- movement
///	q/e 			- up/down (local space)
///	r/f 			- up/down (world space)
///	pageup/pagedown	- up/down (world space)
///	hold shift		- enable fast movement mode
///	right mouse  	- enable free look
///	mouse			- free look / rotation
///     
/// </summary>
public class FreeLookCameraController
{
    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    private float movementSpeed = 25f;

    /// <summary>
    /// Speed of camera movement when shift is held down,
    /// </summary>
    private float fastMovementSpeed = 80f;

    /// <summary>
    /// Sensitivity for free look.
    /// </summary>
    private float freeLookSensitivity = 2.5f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel.
    /// </summary>
    private float zoomSensitivity = 10f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel (fast mode).
    /// </summary>
    private float fastZoomSensitivity = 50f;

    /// <summary>
    /// Set to true when free looking (on right mouse button).
    /// </summary>
    private bool looking = false;

    public void UpdateCameraMovement(Transform cam_transform, Camera cam, float delta_time, float distance_to_terrain, float perspective_fov)
    {
        /* FOV settings **************************************************************************************/
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

        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

        movementSpeed *= Mathf.Clamp(distance / 50f, 0.5f, 1);

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

        if (Input.GetKey(KeyCode.Q))
        {
            cam_transform.position = cam_transform.position + (cam_transform.up * movementSpeed * delta_time);
        }

        if (Input.GetKey(KeyCode.E))
        {
            cam_transform.position = cam_transform.position + (-cam_transform.up * movementSpeed * delta_time);
        }

        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
        {
            cam_transform.position = cam_transform.position + (Vector3.up * movementSpeed * delta_time);
        }

        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
        {
            cam_transform.position = cam_transform.position + (-Vector3.up * movementSpeed * delta_time);
        }

        if (looking)
        {
            float newRotationX = cam_transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            float newRotationY = cam_transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            cam_transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
            cam_transform.position = cam_transform.position + cam_transform.forward * axis * zoomSensitivity;
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

    /// <summary>
    /// Enable free looking.
    /// </summary>
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Disable free looking.
    /// </summary>
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
