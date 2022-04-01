using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoCameraController
{
    public float movementSpeed = 0.1f;
    public float fastSpeedMultiplier = 2.0f;
    public float movementTime = 0.15f;
    public byte rotationAmount = 90;
    public Vector3 zoomAmount = new Vector3(0, -5, 5);

    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 newZoom;

    private float nextLeftRotationTime, nextRightRotationTime = 0.0f;
    private const float nextRotationRate = 0.3f;

    private Vector3[] targetPositions;
    private int targetTransformIndex = 0;

    const float ZOOM_SPEED = 10;

    public IsoCameraController(Transform camera_transform, Vector3[] target_positions)
    {
        targetPositions = target_positions;
        newPosition = target_positions[0];

        newZoom = camera_transform.localPosition;
    }

    public void UpdateCameraMovement(Camera camera, Transform target_transform, float delta_time, float time)
    {
        float lerp_time = delta_time / movementTime;

        /* Determine which direction to rotate towards */
        Vector3 targetDirection = target_transform.position - camera.transform.position;
        /* Rotate the forward vector towards the target direction by one step */
        Vector3 newDirection = Vector3.RotateTowards(camera.transform.forward, targetDirection, 1, 0);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, newPosition, 0.25f * lerp_time);
        camera.transform.localRotation = Quaternion.Lerp(camera.transform.localRotation, Quaternion.LookRotation(newDirection), 2.25f * lerp_time);
        //camera_transform.localPosition = Vector3.Lerp(camera_transform.localPosition, newZoom, lerp_time);

        HandleMouseInput(camera);

        if (Input.anyKey && Vector3.Distance(camera.transform.localPosition, newPosition) < 15)
        {
            HandleKeyboardInput(time);
        }
    }

    private void HandleMouseInput(Camera camera)
    {
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

    private void HandleKeyboardInput(float time)
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            SetNextTargetTransformIndex(false);
            newPosition = targetPositions[targetTransformIndex];
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            SetNextTargetTransformIndex(true);
            newPosition = targetPositions[targetTransformIndex];
        }

        if (Input.GetKey(KeyCode.Q) && time > nextLeftRotationTime)
        {
            nextLeftRotationTime = time + nextRotationRate;
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        else if (Input.GetKey(KeyCode.E) && time > nextRightRotationTime)
        {
            nextRightRotationTime = time + nextRotationRate;
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
    }

    private void SetNextTargetTransformIndex(bool clockwise)
    {
        if (clockwise)
        {
            targetTransformIndex++;
            if (targetTransformIndex >= targetPositions.Length)
                targetTransformIndex = 0;
        }
        else
        {
            targetTransformIndex--;
            if (targetTransformIndex < 0)
                targetTransformIndex = targetPositions.Length - 1;
        }
    }
}
