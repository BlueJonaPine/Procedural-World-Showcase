using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public static UIHandler instance;

    [HideInInspector]
    public Vector2Int coordinates = Vector2Int.zero;

    [Header("UI Inputs and Widgets")]
    public Transform terrainGenerationPanel = null;
    public InputField xCoordinateInput = null;
    public InputField yCoordinateInput = null;
    public InputField seedInput = null;
    public Toggle animationToggle = null;
    public Button terrainGenerationPanelButton = null;
    public Transform windControllerPanel = null;
    public Slider windDirectionSlider = null;
    public Slider windStrengthSlider = null;
    public Button windControllerPanelButton = null;
    public Slider dayTimeSlider = null;
    public Button graphicsQualityButton = null;
    public Toggle cloudActivationToggle = null;
    public Slider cloudDensitySlider = null;
    public Slider rainStrengthSlider = null;
    public GameObject loadingBar = null;
    public Text loadingPercentText = null;
    public Transform cameraControllerPanel = null;
    public Transform optionsPanel = null;
    public Button settingsButton = null;
    public Button creditsButton = null;
    public Button qualityButton = null;
    public Button exitButton = null;
    public Transform creditsPanel = null;

    [Header("Light Source")]
    public GameObject sunGameobject = null;
    public ReflectionProbe reflectionProbe = null;

    [Header("Render Settings")]
    public RenderPipelineAsset highSettings = null;
    public RenderPipelineAsset lowSettings = null;

    private int seed = 0;

    private MouseCaster mouseCaster = new MouseCaster();

    private bool terrainGenerationPanelIsMaximized = true;
    private bool terrainGenerationPanelIsTransiting = false;
    private bool windControllerPanelIsMaximized = true;
    private bool windControllerPanelIsTransiting = false;
    private bool graphicsQualityIsHigh = true;
    private bool seedChanged = false;
    private bool coordinatesChanged = false;

    /* Constants ******************************************************************** */
    /* Borders for coordinate input */
    private int MINIMUM_COORDINATE = 0;
    private int MAXIMUM_COORDINATE = 65535;
    /* General panel constants */
    private int PANEL_WIDTH_SHOWN;
    private int PANEL_WIDTH_HIDDEN;
    private float PANEL_TRANSITION_SPEED = 4f;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Instance of UIHandler already exists in scene!", this);
            this.enabled = false; //Disable instead of destroy so you can see it in inspector and stop whatever has 2 of them
            return;
        }
        instance = this;

        /* Initialize constants */
        RectTransform rect = terrainGenerationPanel.GetComponent<RectTransform>();
        PANEL_WIDTH_SHOWN = Mathf.RoundToInt(rect.anchoredPosition.x);
        PANEL_WIDTH_HIDDEN = PANEL_WIDTH_SHOWN + Mathf.RoundToInt(Mathf.Abs(rect.rect.width) - (terrainGenerationPanelButton.GetComponent<RectTransform>().anchoredPosition.x + Mathf.Abs(terrainGenerationPanelButton.GetComponent<RectTransform>().rect.width)));

        /* Initialize content of UI widgets */
        xCoordinateInput.text = coordinates.x.ToString();
        yCoordinateInput.text = coordinates.y.ToString();
        seedInput.text = EncryptSeed().ToString();
    }

    void Update()
    {
        /* Move Panels in and out */
        UpdatePanelTransitions();

        if (loadingBar.activeSelf)
        {
            loadingPercentText.text = ((int)TerrainCreator.instance.terrainGenerationCompletion).ToString() + "%";
        }

        if (creditsPanel.gameObject.activeSelf && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            mouseCaster.CastFromMousePosition(new int[1] { 0 }, false, 100);
            RectTransform rect_transform = creditsButton.transform.GetComponent<RectTransform>();
            if (mouseCaster.clickPointV2.x < rect_transform.rect.x || mouseCaster.clickPointV2.x > rect_transform.rect.x + rect_transform.rect.width ||
                mouseCaster.clickPointV2.x < rect_transform.rect.y || mouseCaster.clickPointV2.x > rect_transform.rect.y + rect_transform.rect.height)
            {
                OnCreditsShowHide();
            }
        }
    }

    private void UpdatePanelTransitions()
    {
        if (terrainGenerationPanelIsTransiting)
        {
            RectTransform rect = terrainGenerationPanel.GetComponent<RectTransform>();

            if (terrainGenerationPanelIsMaximized)
                rect.anchoredPosition = new Vector3(Mathf.Lerp(rect.anchoredPosition.x, PANEL_WIDTH_SHOWN, PANEL_TRANSITION_SPEED * Time.deltaTime), rect.anchoredPosition.y, 0);
            else
                rect.anchoredPosition = new Vector3(Mathf.Lerp(rect.anchoredPosition.x, PANEL_WIDTH_HIDDEN, PANEL_TRANSITION_SPEED * Time.deltaTime), rect.anchoredPosition.y, 0);

            if (rect.anchoredPosition.x >= PANEL_WIDTH_HIDDEN - 1 && !terrainGenerationPanelIsMaximized)
            {
                rect.anchoredPosition = new Vector3(PANEL_WIDTH_HIDDEN, rect.anchoredPosition.y, 0);
                terrainGenerationPanelIsTransiting = false;
            }
            else if (rect.anchoredPosition.x <= PANEL_WIDTH_SHOWN + 1 && terrainGenerationPanelIsMaximized)
            {
                rect.anchoredPosition = new Vector3(PANEL_WIDTH_SHOWN, rect.anchoredPosition.y, 0);
                terrainGenerationPanelIsTransiting = false;
            }
        }

        if (windControllerPanelIsTransiting)
        {
            RectTransform rect = windControllerPanel.GetComponent<RectTransform>();

            if (windControllerPanelIsMaximized)
                rect.anchoredPosition = new Vector3(Mathf.Lerp(rect.anchoredPosition.x, PANEL_WIDTH_SHOWN, PANEL_TRANSITION_SPEED * Time.deltaTime), rect.anchoredPosition.y, 0);
            else
                rect.anchoredPosition = new Vector3(Mathf.Lerp(rect.anchoredPosition.x, PANEL_WIDTH_HIDDEN, PANEL_TRANSITION_SPEED * Time.deltaTime), rect.anchoredPosition.y, 0);

            if (rect.anchoredPosition.x >= PANEL_WIDTH_HIDDEN - 1 && !windControllerPanelIsMaximized)
            {
                rect.anchoredPosition = new Vector3(PANEL_WIDTH_HIDDEN, rect.anchoredPosition.y, 0);
                windControllerPanelIsTransiting = false;
            }
            else if (rect.anchoredPosition.x <= PANEL_WIDTH_SHOWN + 1 && windControllerPanelIsMaximized)
            {
                rect.anchoredPosition = new Vector3(PANEL_WIDTH_SHOWN, rect.anchoredPosition.y, 0);
                windControllerPanelIsTransiting = false;
            }
        }
    }

    private string EncryptSeed()
    {
        int seed = coordinates.x;
        seed += coordinates.y << 16;

        byte digit_count = 0;
        int combined_seed = seed;
        while (combined_seed / 10 != 0)
        {
            combined_seed /= 10;
            digit_count++;
        }

        string result = "0x" + seed.ToString("X" + digit_count.ToString());

        return result;
    }

    private void DecryptSeed()
    {
        int x = seed & MAXIMUM_COORDINATE;
        int y = seed >> 16;

        coordinates.Set(x, y);

        xCoordinateInput.text = x.ToString();
        yCoordinateInput.text = y.ToString();
    }

    public void OnInputCoordinatesChange()
    {
        try
        {
            int x = Int32.Parse(xCoordinateInput.text);
            if (x > MAXIMUM_COORDINATE)
                x = MAXIMUM_COORDINATE;
            else if (x < MINIMUM_COORDINATE)
                x = MINIMUM_COORDINATE;
            xCoordinateInput.text = x.ToString();

            int y = Int32.Parse(yCoordinateInput.text);
            if (y > MAXIMUM_COORDINATE)
                y = MAXIMUM_COORDINATE;
            else if (y < MINIMUM_COORDINATE)
                y = MINIMUM_COORDINATE;
            yCoordinateInput.text = y.ToString();

            coordinates.Set(x, y);

            if(!seedChanged)
            {
                coordinatesChanged = true;

                seedInput.text = EncryptSeed().ToString();

                coordinatesChanged = false;
            }
        }
        catch
        {

        }
    }

    public void OnInputSeedChange()
    {
        if (coordinatesChanged || seedChanged)
            return;

        try
        {
            string result = seedInput.text;
            if (result.Length == 0)
                result = "0";
            if (result[0] != '0' || result[1] != 'x')
            {
                if (result.Length >= 2 && !char.IsDigit(result[1]))
                    return;
            }

            bool hex = false;
            if (result.Length >= 2 && result[0] == '0' && result[1] == 'x')
            {
                result = result.Split(new string[] { "0x" }, StringSplitOptions.None)[1];
                hex = true;
            }

            string _result = "";
            for (int i = 0; i < result.Length; i++)
            {
                int val = (int)char.GetNumericValue(result[i]);
                if ((val < 48 && val > 57) &&
                    (val < 65 && val > 70) &&
                    (val < 97 && val > 102))
                    return;

                if (i < 8)
                    _result += result[i];
            }

            seedChanged = true;

            if(hex)
                seedInput.text = "0x" + _result;
            else
                seedInput.text = _result;

            //seed = Int32.Parse(seedInput.text);
            if(result != "")
            {
                seed = Convert.ToInt32(_result, 16);
                
            }
            else
            {
                seed = 0;
            }

            DecryptSeed();

            seedChanged = false;
        }
        catch
        {

        }
    }

    public void TerrainGenerationPanelShowHide()
    {
        Text button_text = terrainGenerationPanelButton.transform.GetChild(0).GetComponent<Text>();

        terrainGenerationPanelIsTransiting = true;

        if (terrainGenerationPanelIsMaximized)
        {
            terrainGenerationPanelIsMaximized = false;
            button_text.text = "<";
        }
        else
        {
            terrainGenerationPanelIsMaximized = true;
            button_text.text = ">";
        }
    }

    public void windControllerPanelShowHide()
    {
        Text button_text = windControllerPanelButton.transform.GetChild(0).GetComponent<Text>();

        windControllerPanelIsTransiting = true;

        if (windControllerPanelIsMaximized)
        {
            windControllerPanelIsMaximized = false;
            button_text.text = "<";
        }
        else
        {
            windControllerPanelIsMaximized = true;
            button_text.text = ">";
        }
    }

    public void OnOptionsPanelShowHide()
    {
        if (creditsButton.gameObject.activeSelf)
        { 
            creditsButton.gameObject.SetActive(false);
            qualityButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
        }
        else
        {
            if (!TerrainCreator.instance.isTerrainGenerationOngoing)
                creditsButton.gameObject.SetActive(true);
                qualityButton.gameObject.SetActive(true);
                exitButton.gameObject.SetActive(true);
        }
    }

    public void OnExitApplication()
    {
        Application.Quit();
    }

    public void OnCreditsShowHide()
    {
        if (creditsPanel.gameObject.activeSelf)
            creditsButton.GetComponent<Image>().color = exitButton.GetComponent<Image>().color;
        else
            creditsButton.GetComponent<Image>().color = settingsButton.GetComponent<Image>().color;

        creditsPanel.gameObject.SetActive(!creditsPanel.gameObject.activeSelf);
    }

    public void OnGraphicsQualityChange()
    {
        if (graphicsQualityIsHigh)
        {
            graphicsQualityIsHigh = false;

            qualityButton.transform.GetChild(0).GetComponent<Text>().text = "High Qlty";

            //QualitySettings.SetQualityLevel(0);
            QualitySettings.renderPipeline = lowSettings;
            GraphicsSettings.renderPipelineAsset = lowSettings;
        }
        else
        {
            graphicsQualityIsHigh = true;

            qualityButton.transform.GetChild(0).GetComponent<Text>().text = "Low Qlty";

            //QualitySettings.SetQualityLevel(5);
            QualitySettings.renderPipeline = highSettings;
            GraphicsSettings.renderPipelineAsset = highSettings;
        }
    }

    public void OnCloudEnable()
    {
        cloudDensitySlider.interactable = cloudActivationToggle.isOn;
        rainStrengthSlider.interactable = cloudActivationToggle.isOn;
    }

    public void EnableLoadingScreen(bool is_active)
    {
        loadingBar.SetActive(is_active);
    }

    public void EnableUI(bool is_active)
    {
        if(!is_active)
            TerrainGenerationPanelShowHide();

        windControllerPanel.gameObject.SetActive(is_active);
        cameraControllerPanel.gameObject.SetActive(is_active);


        terrainGenerationPanelButton.interactable = is_active;
        //settingsButton.interactable = is_active;
    }
}
