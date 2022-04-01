using System.Collections;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public static CloudController instance;

    [SerializeField] UIHandler uiHandler;
    [SerializeField] Transform cloudSpawner;
    [SerializeField] Transform renderedClouds;
    [SerializeField] Transform hiddenClouds;

    BoxCollider boxCollider;
    CapsuleCollider capsuleCollider;

    [HideInInspector]
    public bool cloudsEnabled = false;
    bool cloudsProcessing = false;
    bool cloudsDensityChangeInProgress = false;

    float actualTimeSinceSpawn = 0;
    int cloudDensity = 0;
    [HideInInspector]
    public float rainStrength = 0;
    int randomAdjustment = 0;

    const float CLOUD_MIN_HEIGHT = 35;
    const float CLOUD_HEIGHT_SPREAD = 20;
    int AMOUNT_OF_SPAWNS;
    const float TIME_UNTIL_NEXT_SPAWN = 26;
    float SPAWNER_OFFSET;

    #region DEBUG *********************************
    #if UNITY_EDITOR
        [Header("Debug Monitor")]
        [SerializeField] bool renderSpawners = false;
        bool renderSpawnersBuffer = false;
        [SerializeField] int RandomAdjustment = 0;
        [SerializeField] float TimeUntilNextSpawn = 0;
        [SerializeField] float ActualTimeSinceSpawn = 0;
    #endif
    #endregion

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Instance of CloudController already exists in scene!", this);
            this.enabled = false; //Disable instead of destroy so you can see it in inspector and stop whatever has 2 of them
            return;
        }
        instance = this;

        #if UNITY_EDITOR
            foreach (Transform spawner in cloudSpawner)
                spawner.GetComponent<MeshRenderer>().enabled = renderSpawners;
            renderSpawnersBuffer = renderSpawners;
        #endif
    }

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.center = new Vector3(boxCollider.center.x, CLOUD_MIN_HEIGHT + CLOUD_HEIGHT_SPREAD / 2, boxCollider.center.z);
        boxCollider.size = new Vector3(boxCollider.size.x, CLOUD_HEIGHT_SPREAD, boxCollider.size.z);

        capsuleCollider = GetComponent <CapsuleCollider>();
        capsuleCollider.center = new Vector3(capsuleCollider.center.x, CLOUD_MIN_HEIGHT + CLOUD_HEIGHT_SPREAD / 2, capsuleCollider.center.z);

        cloudDensity = (int)uiHandler.cloudDensitySlider.value;
        rainStrength = UIHandler.instance.rainStrengthSlider.value;

        /* initialize constants */
        AMOUNT_OF_SPAWNS = cloudSpawner.transform.childCount;
        SPAWNER_OFFSET = capsuleCollider.radius;
    }

    void Update()
    {
        rainStrength = UIHandler.instance.rainStrengthSlider.value;

        /* update the position of the cloud spawn points */
        UpdateCloudSpawnerTransform();

        /* prevent any spawning/enabling/disabling as long as initial generation of clouds or destroying of clouds take place */
        if (cloudsProcessing)
            return;

        /* update the amount of clouds being rendered if user changed density */
        if (!cloudsDensityChangeInProgress)
            StartCoroutine(UpdateCloudDensity());

        /* en-/disable clouds depending on user input */
        if (uiHandler.cloudActivationToggle.isOn && !cloudsEnabled)
        {
            StartCoroutine(EnableClouds());
            StartCoroutine(StartSpawningClouds());
            actualTimeSinceSpawn = 0;
        }
        else if (!uiHandler.cloudActivationToggle.isOn && cloudsEnabled)
        {
            StartCoroutine(DisableClouds());
        }

        /* clouds can be spawned if clouds are enabled in the first place and if no en-/disabling takes place */
        if (cloudsEnabled && !cloudsProcessing && !cloudsDensityChangeInProgress)
        {
            /* spawn clouds every X seconds */
            actualTimeSinceSpawn += Time.deltaTime;
            float wind_strength_influence = WindController.instance.actualWindStrength * 1.5f + 1;
            float cloud_density_influence = (cloudDensity / uiHandler.cloudDensitySlider.maxValue) * 0.5f + 1;

            #if UNITY_EDITOR
                ActualTimeSinceSpawn = actualTimeSinceSpawn;
                TimeUntilNextSpawn = TIME_UNTIL_NEXT_SPAWN / (wind_strength_influence * cloud_density_influence);
            #endif

            if (actualTimeSinceSpawn > TIME_UNTIL_NEXT_SPAWN / (wind_strength_influence * cloud_density_influence))
            {
                actualTimeSinceSpawn = 0;

                StartCoroutine(StartSpawningClouds());
            }
        }

        #if UNITY_EDITOR
            if(renderSpawnersBuffer != renderSpawners)
                foreach (Transform spawner in cloudSpawner)
                    spawner.GetComponent<MeshRenderer>().enabled = renderSpawners;
            renderSpawnersBuffer = renderSpawners;
        #endif
    }

    IEnumerator UpdateCloudDensity()
    {
        cloudsDensityChangeInProgress = true;

        int updated_cloud_density = (int)uiHandler.cloudDensitySlider.value;

        if (updated_cloud_density > cloudDensity)
        {
            for (int i = 0; i < hiddenClouds.childCount; i++)
            {
                float random = Random.Range((int)uiHandler.cloudDensitySlider.minValue, (int)uiHandler.cloudDensitySlider.maxValue + 1);
                if (updated_cloud_density >= random)
                {
                    try
                    {
                        ParticleSystem.EmissionModule emmission_module = renderedClouds.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystem>().emission;
                        if (Mathf.Approximately(rainStrength, 0))
                        {
                            emmission_module.enabled = false;
                        }
                        else
                        {
                            emmission_module.enabled = true;
                        }

                        hiddenClouds.GetChild(i).transform.GetComponent<MeshRenderer>().enabled = true;
                        hiddenClouds.GetChild(i).transform.parent = renderedClouds;
                    }
                    catch
                    {

                    }
                }

                yield return null;
            }
        }
        else if (updated_cloud_density < cloudDensity)
        {
            for (int i = 0; i < renderedClouds.childCount; i++)
            {
                float random = Random.Range((int)uiHandler.cloudDensitySlider.minValue, (int)uiHandler.cloudDensitySlider.maxValue + 1);
                if (updated_cloud_density < random)
                {
                    try
                    {
                        ParticleSystem.EmissionModule emmission_module = renderedClouds.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystem>().emission;
                        emmission_module.enabled = false;

                        renderedClouds.GetChild(i).transform.GetComponent<MeshRenderer>().enabled = false;
                        renderedClouds.GetChild(i).transform.parent = hiddenClouds;
                    }
                    catch
                    {

                    }
                }

                yield return null;
            }
        }

        cloudDensity = updated_cloud_density;

        cloudsDensityChangeInProgress = false;
    }

    void UpdateCloudSpawnerTransform()
    {
        cloudSpawner.position = CalculateSpawnerPositionForCapsuleCollider(-WindController.instance.actualWindDirection);
        Vector3 target_spawn_direction = capsuleCollider.center - cloudSpawner.transform.position;
        Vector3 new_direction = Vector3.RotateTowards(cloudSpawner.transform.forward, target_spawn_direction, 360 * Mathf.Deg2Rad, 0.0f);
        cloudSpawner.transform.rotation = Quaternion.LookRotation(new_direction);
    }

    Vector3 CalculateSpawnerPositionForCapsuleCollider(Vector3 wind_direction)
    {
        Vector2 point_1 = Vector2.zero;
        Vector2 point_2 = new Vector2(wind_direction.x, wind_direction.z);

        float radiant_angle = Mathf.Atan2(point_2.y - point_1.y, point_2.x - point_1.x);

        Vector3 spawner_position = capsuleCollider.center + SPAWNER_OFFSET * new Vector3(Mathf.Cos(radiant_angle), 0, Mathf.Sin(radiant_angle));

        return spawner_position;
    }

    Vector3 CalculateSpawnerPositionForBoxCollider(Vector3 wind_direction)
    {
        /* approximatly around the box of the collider depending on the wind direction */
        Vector2 spawner_position = new Vector2();
        if (wind_direction.x < wind_direction.z)
        {
            if (Mathf.Abs(wind_direction.x) < Mathf.Abs(wind_direction.z))
                spawner_position.Set(wind_direction.x * Mathf.Sqrt(2) * SPAWNER_OFFSET, SPAWNER_OFFSET);
            else
                spawner_position.Set(-SPAWNER_OFFSET, wind_direction.z * Mathf.Sqrt(2) * SPAWNER_OFFSET);
        }
        else
        {
            if (Mathf.Abs(wind_direction.x) > Mathf.Abs(wind_direction.z))
                spawner_position.Set(SPAWNER_OFFSET, wind_direction.z * Mathf.Sqrt(2) * SPAWNER_OFFSET);
            else
                spawner_position.Set(wind_direction.x * Mathf.Sqrt(2) * SPAWNER_OFFSET, -SPAWNER_OFFSET);
        }

        return new Vector3(spawner_position.x, 0, spawner_position.y) + capsuleCollider.center;
    }

    IEnumerator StartSpawningClouds()
    {
        int rendered_clouds = 0;

        foreach (Transform spawn_point in cloudSpawner.transform)
        {
            GameObject cloud = SpawnCloud(spawn_point.position);

            /* all clouds are hidden by default --> render a couple of those depending on the cloud density */
            int random = Random.Range((int)uiHandler.cloudDensitySlider.minValue, (int)uiHandler.cloudDensitySlider.maxValue + 1);
            /* cloudDensity       1 - 5 */
            /* random             1 - 5 */
            /* random_adjustment -1 - 1 */
            if (cloudDensity >= (random + randomAdjustment))
            {
                cloud.transform.GetComponent<MeshRenderer>().enabled = true;
                cloud.transform.parent = renderedClouds;
                rendered_clouds++;
            }

            yield return null;
        }

        /* adjust the next randomly spawned clouds --> in-/decreases the chance of a cloud being rendered the next time */
        if (rendered_clouds > cloudDensity && cloudDensity > 1)
            randomAdjustment = 1;
        else if (rendered_clouds < cloudDensity)
            randomAdjustment = -1;
        else
            randomAdjustment = 0;

        #if UNITY_EDITOR
            RandomAdjustment = randomAdjustment;
        #endif
    }

    GameObject SpawnCloud(Vector3 position)
    {
        float xCoord = (position.x + (float)Globals.instance.terrainSize * Time.time) / 1.01f;
        float cloud_type_noise = Mathf.PerlinNoise(xCoord, 0);

        GameObject cloud;

        if (cloud_type_noise < 0.34f)
            cloud = (GameObject)Instantiate(Resources.Load("Prefabs/Cloud_1"));
        else if (cloud_type_noise < 0.66f)
            cloud = (GameObject)Instantiate(Resources.Load("Prefabs/Cloud_2"));
        else
            cloud = (GameObject)Instantiate(Resources.Load("Prefabs/Cloud_3"));

        xCoord += Globals.instance.terrainSize;
        float height_noise = Mathf.PerlinNoise(xCoord, 0);

        xCoord += Globals.instance.terrainSize;
        float position_noise = Mathf.PerlinNoise(xCoord, 0);

        float x_offset = (position_noise - 0.5f) * 20f;
        float z_offset = (position_noise - 0.5f) * 20f;

        cloud.transform.parent = hiddenClouds;
        cloud.transform.position = new Vector3(position.x + x_offset, CLOUD_MIN_HEIGHT + height_noise * CLOUD_HEIGHT_SPREAD, position.z + z_offset);
        cloud.transform.rotation = Quaternion.Euler(0, cloud_type_noise * 1440, 0);
        cloud.transform.localScale *= 1 + (0.9f - height_noise)*3.5f;

        return cloud;
    }

    IEnumerator EnableClouds()
    {
        cloudsProcessing = true;

        for(int y = 0; y < AMOUNT_OF_SPAWNS; y++)
        {
            for (int x = 0; x < AMOUNT_OF_SPAWNS; x++)
            {
                Vector3 cloud_position = new Vector3(x * 14 - 10, CLOUD_MIN_HEIGHT, y * 14 - 10);

                float random = Random.Range((int)uiHandler.cloudDensitySlider.minValue, (int)uiHandler.cloudDensitySlider.maxValue + 1);

                /* only spawn clouds if they are inside the collider */
                if (Vector3.Distance(cloud_position, capsuleCollider.center) < capsuleCollider.radius)
                {
                    GameObject cloud = SpawnCloud(cloud_position);

                    if (cloudDensity >= random)
                    {
                        cloud.transform.parent = renderedClouds;
                        cloud.transform.GetComponent<MeshRenderer>().enabled = true;
                    }
                }

                yield return null;
            }
        }

        cloudsProcessing = false;
        cloudsEnabled = true;
    }

    IEnumerator DisableClouds()
    {
        cloudsProcessing = true;

        while(hiddenClouds.childCount != 0 || renderedClouds.childCount != 0)
        {
            foreach (Transform cloud in hiddenClouds)
            {
                Destroy(cloud.gameObject);

                //yield return new WaitForSeconds(.1f);
                yield return null;
            }

            foreach (Transform cloud in renderedClouds)
            {
                Destroy(cloud.gameObject);

                //yield return new WaitForSeconds(.1f);
                yield return null;
            }
        }

        cloudsProcessing = false;
        cloudsEnabled = false;
    }

    private void OnTriggerEnter(Collider cloud)
    {
        //cloud.gameObject.GetComponent<MeshRenderer>().enabled = true;
        //cloud.transform.parent = renderedClouds;
    }

    private void OnTriggerExit(Collider cloud)
    {
        Destroy(cloud.gameObject);
    }
}
