using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraRigController : MonoBehaviour
{
    public static CameraRigController instance;

    [SerializeField] Transform mainCameraTransform;
    [SerializeField] Transform orthographicTarget;
    [SerializeField] Transform switchCameraButton;
    [SerializeField] Transform isoCameraButton;
    [SerializeField] Transform freeLookCameraButton;
    [SerializeField] Transform showroomCameraButton;
    [SerializeField] Text cameraMovementText;

    Vector3[] ORTHOGRAPHIC_POSITIONS = { new Vector3(-42, 87, -42), new Vector3(-42, 87, 92), new Vector3(92, 87, 92), new Vector3(92, 87, -42) };
    const float ORTHOGRAPHIC_SIZE = 32;
    Vector3 PERSPECTIVE_POSITION = new Vector3(-100, 180, -100);
    const float PERSPECTIVE_FOV = 15;
    float DISTANCE_CAMERA_TO_TERRAIN;
    Vector3 CAMERA_ROTATION = new Vector3(40, 45, 0);
    bool cameraMovementUpdateOngoing = false;

    Camera cam;
    IsoCameraController isoCameraController;
    FreeLookCameraController freeLookCameraController = new FreeLookCameraController();
    ShowroomCameraController showroomCameraController;

    bool forced360CameraEnabled = false;

    #region DEBUG *********************************
    #if UNITY_EDITOR
        [Header("Debug Monitor")]
        [SerializeField] Vector3 CameraRotation;
    #endif
    #endregion

    enum CameraMode
    {
        iso = 0,
        showroom = 1,
        free_look = 2
    };
    CameraMode cameraMode = CameraMode.showroom;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Instance of CameraRigController already exists in scene!", this);
            this.enabled = false; //Disable instead of destroy so you can see it in inspector and stop whatever has 2 of them
            return;
        }
        instance = this;

        cam = mainCameraTransform.GetComponent<Camera>();

        OnShowroomCameraEnable();

        isoCameraController = new IsoCameraController(mainCameraTransform, ORTHOGRAPHIC_POSITIONS);

        DISTANCE_CAMERA_TO_TERRAIN = Vector3.Distance(mainCameraTransform.localPosition, orthographicTarget.localPosition);

        showroomCameraController = new ShowroomCameraController(orthographicTarget.position, DISTANCE_CAMERA_TO_TERRAIN);
    }

    private void Update()
    {
        #region DEBUG *********************************
        #if UNITY_EDITOR
                CameraRotation = mainCameraTransform.eulerAngles;
        #endif
        #endregion

        if (forced360CameraEnabled)
            return;

        if (cameraMode == CameraMode.iso)
        {
            isoCameraController.UpdateCameraMovement(cam, orthographicTarget, Time.deltaTime, Time.time);
        }
        else if (cameraMode == CameraMode.free_look)
        {
            freeLookCameraController.UpdateCameraMovement(mainCameraTransform, cam, Time.deltaTime, DISTANCE_CAMERA_TO_TERRAIN, PERSPECTIVE_FOV);
        }
        else
        {
            showroomCameraController.UpdateCameraMovement(cam, Time.deltaTime);
        }
    }

    public void SwitchCameraMode()
    {
        if (isoCameraButton.gameObject.activeSelf)
        {
            isoCameraButton.gameObject.SetActive(false);
            freeLookCameraButton.gameObject.SetActive(false);
            showroomCameraButton.gameObject.SetActive(false);
        }
        else
        {
            isoCameraButton.gameObject.SetActive(true);
            freeLookCameraButton.gameObject.SetActive(true);
            showroomCameraButton.gameObject.SetActive(true);
        }
    }

    Coroutine startedCoroutine = null;

    public void OnIsoCameraEnable()
    {
        cam.orthographic = true;
        cam.orthographicSize = ORTHOGRAPHIC_SIZE;
        mainCameraTransform.localPosition = ORTHOGRAPHIC_POSITIONS[0];
        mainCameraTransform.localRotation = Quaternion.Euler(CAMERA_ROTATION);

        if(cameraMode != CameraMode.iso)
        {
            cameraMode = CameraMode.iso;

            freeLookCameraButton.GetComponent<Image>().color = isoCameraButton.GetComponent<Image>().color;
            showroomCameraButton.GetComponent<Image>().color = isoCameraButton.GetComponent<Image>().color;
            isoCameraButton.GetComponent<Image>().color = switchCameraButton.GetComponent<Image>().color;

            if (cameraMovementUpdateOngoing == true)
                StopCoroutine(startedCoroutine);
            startedCoroutine = StartCoroutine(UpdateCameraMovementText("A+D = Camera Movement\nMouse Wheel = Zoom"));
        }

        isoCameraButton.gameObject.SetActive(false);
        freeLookCameraButton.gameObject.SetActive(false);
        showroomCameraButton.gameObject.SetActive(false);
    }

    public void OnFreeLookCameraEnable()
    {
        cam.orthographic = false;
        cam.fieldOfView = PERSPECTIVE_FOV;
        mainCameraTransform.localPosition = PERSPECTIVE_POSITION;
        mainCameraTransform.localRotation = Quaternion.Euler(CAMERA_ROTATION + new Vector3(4,0,0));

        if (cameraMode != CameraMode.free_look)
        {
            cameraMode = CameraMode.free_look;

            isoCameraButton.GetComponent<Image>().color = freeLookCameraButton.GetComponent<Image>().color;
            showroomCameraButton.GetComponent<Image>().color = freeLookCameraButton.GetComponent<Image>().color;
            freeLookCameraButton.GetComponent<Image>().color = switchCameraButton.GetComponent<Image>().color;

            if (cameraMovementUpdateOngoing == true)
                StopCoroutine(startedCoroutine);
            startedCoroutine = StartCoroutine(UpdateCameraMovementText("Right Mouse Click + Mouse Movement = Camera Look\nWASD = Camera Movement\nShift = Fast Speed"));
        }

        isoCameraButton.gameObject.SetActive(false);
        freeLookCameraButton.gameObject.SetActive(false);
        showroomCameraButton.gameObject.SetActive(false);
    }

    public void OnShowroomCameraEnable()
    {
        cam.orthographic = true;
        cam.orthographicSize = ORTHOGRAPHIC_SIZE;
        mainCameraTransform.localPosition = ORTHOGRAPHIC_POSITIONS[0];
        mainCameraTransform.localRotation = Quaternion.Euler(CAMERA_ROTATION);

        if (cameraMode != CameraMode.showroom)
        {
            cameraMode = CameraMode.showroom;

            isoCameraButton.GetComponent<Image>().color = showroomCameraButton.GetComponent<Image>().color;
            freeLookCameraButton.GetComponent<Image>().color = showroomCameraButton.GetComponent<Image>().color;
            showroomCameraButton.GetComponent<Image>().color = switchCameraButton.GetComponent<Image>().color;

            if (cameraMovementUpdateOngoing == true)
                StopCoroutine(startedCoroutine);
            startedCoroutine = StartCoroutine(UpdateCameraMovementText("Left Mouse Click onto Terrain + Mouse Movement = Camera Movement\nMouse Wheel = Zoom"));
        }

        isoCameraButton.gameObject.SetActive(false);
        freeLookCameraButton.gameObject.SetActive(false);
        showroomCameraButton.gameObject.SetActive(false);
    }

    private IEnumerator UpdateCameraMovementText(string text)
    {
        yield return null;

        cameraMovementUpdateOngoing = true;
        float alpha = 1f;

        cameraMovementText.text = text;
        cameraMovementText.color = new Color(cameraMovementText.color.r, cameraMovementText.color.g, cameraMovementText.color.b, alpha);

        cameraMovementText.transform.gameObject.SetActive(true);

        yield return new WaitForSeconds(5.5f);

        while (alpha > 0.005f)
        {
            alpha -= 0.005f;
            cameraMovementText.color = new Color(cameraMovementText.color.r, cameraMovementText.color.g, cameraMovementText.color.b, alpha);
            yield return null;
        }

        cameraMovementText.transform.gameObject.SetActive(false);

        cameraMovementUpdateOngoing = false;
    }

    public void Enable360Movement(bool is_active)
    {
        forced360CameraEnabled = is_active;

        if (is_active)
            OnShowroomCameraEnable();
    }

    public void Force360Movement(float angle)
    {
        showroomCameraController.MoveCameraHorizontally(cam.transform, angle, Quaternion.Euler(CAMERA_ROTATION).eulerAngles.y);
    }
}
