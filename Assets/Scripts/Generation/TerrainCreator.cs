using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCreator : MonoBehaviour
{
    public static TerrainCreator instance;

    public GameObject canvasUI = null;
    public GameObject terrainHeightmapPlane = null;
    public GameObject terrainHeightmapTrees = null;
    public GameObject smallNoiseHeightmapPlane = null;
    public GameObject mediumNoiseHeightmapPlane = null;
    public GameObject largeNoiseHeightmapPlane = null;
    public GameObject eagleNoiseHeightmapPlane = null;
    public GameObject raisedHideNoiseHeightmapPlane = null;
    public GameObject lighthouseNoiseHeightmapPlane = null;
    public GameObject lighthouseMaskHeightmapPlane = null;
    public GameObject environmentParent = null;
    public GameObject housesAndVehiclesParent = null;
    public GameObject wildlifeParent = null;
    public Transform terrainCenter = null;
    public GameObject waterCube = null;
    public GameObject tilePrefab = null;
    public Transform tileGenerationAnimationParent = null;
    public Transform tileGenerationFinishedParent = null;

    /* The origin of the sampled area in the plane. */
    private int xOrg;
    private int yOrg;

    /* The number of cycles of the basic noise pattern that are repeated over the width and height of the texture. */
    private const float SCALE = 0.5f;

    private Texture2D terrainNoiseTexture;
    private Color32[] terrainNoiseTexturePixels;
    private Texture2D treeNoiseTexture;
    private Color32[] treeNoiseTexturePixels;

    private Texture2D smallNoiseTexture;
    private Color32[] smallNoiseTexturePixels;
    private Texture2D mediumNoiseTexture;
    private Color32[] mediumNoiseTexturePixels;
    private Texture2D largeNoiseTexture;
    private Color32[] largeNoiseTexturePixels;

    private Texture2D eagleNoiseTexture;
    private Color32[] eagleNoiseTexturePixels;
    private Texture2D raisedHideNoiseTexture;
    private Color32[] raisedHideNoiseTexturePixels;
    private Texture2D lighthouseNoiseTexture;
    private Color32[] lighthouseNoiseTexturePixels;

    private float[,] terrainPerlinNoise;
    private float waterCubeScaleY;

    private int boatHousesGenerated;
    private bool lightHouseGenerated;
    private bool senderStationGenerated;
    bool[,] isBeach = new bool[50, 50];

    public CombinedMesh combinedMesh = null;
    public float terrainGenerationCompletion = 0;

    public bool isTerrainCreated = false;
    public bool isTerrainGenerationOngoing = false;
    public bool terrainGenerationIsAnimated = false;

    /* Width and height of the texture in pixels. */
    int PIXEL_WIDTH;
    int PIXEL_HEIGHT;

    const int MAX_SEAGULLS_PER_TILE = 5;
    const int MIN_SEAGULLS_PER_TILE = 1;
    const float SEAGULL_SPAWN_HEIGHT = 1f;
    const int BOAT_SPAWN_CHANCE_PERCENTAGE_PER_TILE = 3;

    /* Constant values that are related to the noise value (0-1) and influence the generation. */
    const float MAX_SEAGULL_SPAWN_WATER_DEPTH = 0.0045f;
    const float MAX_BEACH_HEIGHT = 0.024f;
    const float MIN_BEACH_HEIGHT = 0.012f;
    const float TREE_MAX_NOISE_LEVEL_PER_TILE = 0.555f;
    const float TREE_MIN_NOISE_LEVEL_PER_TILE = 0.25f;

    #region DEBUG *************************************************
    #if UNITY_EDITOR
        public float debugNoise1;
        public float debugNoise2;
        public Vector2Int debugMouseClickPoint = new Vector2Int();
        private MouseCaster debugMouseCaster = new MouseCaster();
    #endif
    #endregion

    public void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Instance of TerrainCreator already exists in scene!", this);
            this.enabled = false; //Disable instead of destroy so you can see it in inspector and stop whatever has 2 of them
            return;
        }
        instance = this;

        if (waterCube == null || canvasUI == null || terrainHeightmapPlane == null || terrainHeightmapTrees == null || terrainCenter == null || environmentParent == null || housesAndVehiclesParent == null || wildlifeParent == null)
        {
            Debug.LogError("References in the TerrainCreator script missing.");
            return;
        }
    }

    void Start()
    {
        xOrg = UIHandler.instance.coordinates.x;
        yOrg = UIHandler.instance.coordinates.y;

        PIXEL_WIDTH = Globals.instance.terrainSize;
        PIXEL_HEIGHT = Globals.instance.terrainSize;

        terrainPerlinNoise = new float[PIXEL_WIDTH, PIXEL_HEIGHT];

        /* Set up the texture and a Color array to hold pixels during processing. */

        terrainNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        terrainNoiseTexture.filterMode = FilterMode.Point;
        terrainNoiseTexturePixels = new Color32[terrainNoiseTexture.width * terrainNoiseTexture.height];
        terrainHeightmapPlane.GetComponent<Renderer>().material.mainTexture = terrainNoiseTexture;

        treeNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        treeNoiseTexturePixels = new Color32[treeNoiseTexture.width * treeNoiseTexture.height];
        terrainHeightmapTrees.GetComponent<Renderer>().material.mainTexture = treeNoiseTexture;

        smallNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        smallNoiseTexturePixels = new Color32[smallNoiseTexture.width * smallNoiseTexture.height];
        smallNoiseHeightmapPlane.GetComponent<Renderer>().material.mainTexture = smallNoiseTexture;

        mediumNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        mediumNoiseTexturePixels = new Color32[mediumNoiseTexture.width * mediumNoiseTexture.height];
        mediumNoiseHeightmapPlane.GetComponent<Renderer>().material.mainTexture = mediumNoiseTexture;

        largeNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        largeNoiseTexturePixels = new Color32[largeNoiseTexture.width * largeNoiseTexture.height];
        largeNoiseHeightmapPlane.GetComponent<Renderer>().material.mainTexture = largeNoiseTexture;

        raisedHideNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        raisedHideNoiseTexturePixels = new Color32[raisedHideNoiseTexture.width * raisedHideNoiseTexture.height];
        raisedHideNoiseHeightmapPlane.GetComponent<Renderer>().material.mainTexture = raisedHideNoiseTexture;

        eagleNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        eagleNoiseTexturePixels = new Color32[eagleNoiseTexture.width * eagleNoiseTexture.height];
        eagleNoiseHeightmapPlane.GetComponent<Renderer>().material.mainTexture = eagleNoiseTexture;

        lighthouseNoiseTexture = new Texture2D(PIXEL_WIDTH, PIXEL_HEIGHT);
        lighthouseNoiseTexturePixels = new Color32[lighthouseNoiseTexture.width * lighthouseNoiseTexture.height];
        lighthouseNoiseHeightmapPlane.GetComponent<Renderer>().material.mainTexture = lighthouseNoiseTexture;

        waterCubeScaleY = (Globals.instance.GetSeaLevel() * Globals.instance.heightScale) + 0.99f;
        waterCube.transform.localScale = new Vector3(waterCube.transform.localScale.x, waterCubeScaleY, waterCube.transform.localScale.z);
    }

    void Update()
    {
        xOrg = UIHandler.instance.coordinates.x;
        yOrg = UIHandler.instance.coordinates.y;

        #region DEBUG *************************************************
        #if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                debugMouseCaster.CastFromMousePosition(new int[1] { 6 }, false, 500);
                debugMouseClickPoint.x = Mathf.Clamp((int)debugMouseCaster.clickPointV2.x, 0, 49);
                debugMouseClickPoint.y = Mathf.Clamp((int)debugMouseCaster.clickPointV2.y, 0, 49);
                debugNoise1 = terrainPerlinNoise[debugMouseClickPoint.x, debugMouseClickPoint.y];
                if (isTerrainCreated)
                    debugNoise2 = combinedMesh.tiles[debugMouseClickPoint.x, debugMouseClickPoint.y].noiseValue;
            }
        #endif
        #endregion
    }

    float UpdateNoiseMasksForGeneration(int x, int y)
    {
        float x_val = xOrg * Globals.instance.terrainSize + x + 100;
        float y_val = yOrg * Globals.instance.terrainSize + y + 100;
        float xCoord = (float)x_val / ((float)Globals.instance.terrainSize * 0.5f);
        float yCoord = (float)y_val / ((float)Globals.instance.terrainSize * 0.5f);
        float noise1 = Mathf.PerlinNoise(xCoord, yCoord);
        xCoord = (float)x_val / ((float)Globals.instance.terrainSize * 0.05f);
        yCoord = (float)y_val / ((float)Globals.instance.terrainSize * 0.05f);
        float noise2 = Mathf.PerlinNoise(xCoord, yCoord);
        float tree_noise_level = (noise1 + noise2) / 2;// noise1 * noise2;
        Color color = new Color(tree_noise_level, tree_noise_level, tree_noise_level, 1);
        treeNoiseTexturePixels[y * treeNoiseTexture.width + x] = color;

        xCoord = (float)x / ((float)Globals.instance.terrainSize * 0.8f);
        yCoord = (float)y / ((float)Globals.instance.terrainSize * 0.8f);
        float noise = Mathf.PerlinNoise(xCoord, yCoord);
        color = new Color(noise, noise, noise, 1);
        smallNoiseTexturePixels[y * smallNoiseTexture.width + x] = color;

        xCoord = (float)x / ((float)Globals.instance.terrainSize * 0.205f);
        yCoord = (float)y / ((float)Globals.instance.terrainSize * 0.205f);
        noise = Mathf.PerlinNoise(xCoord, yCoord);
        color = new Color(noise, noise, noise, 1);
        mediumNoiseTexturePixels[y * mediumNoiseTexture.width + x] = color;

        xCoord = (float)x / ((float)Globals.instance.terrainSize * 0.05f);
        yCoord = (float)y / ((float)Globals.instance.terrainSize * 0.05f);
        noise = Mathf.PerlinNoise(xCoord, yCoord);
        color = new Color(noise, noise, noise, 1);
        largeNoiseTexturePixels[y * largeNoiseTexture.width + x] = color;

        x_val = xOrg * Globals.instance.terrainSize + x + 1000;
        y_val = yOrg * Globals.instance.terrainSize + y + 1000;
        xCoord = (float)x_val / ((float)Globals.instance.terrainSize * 0.22f);
        yCoord = (float)y_val / ((float)Globals.instance.terrainSize * 0.22f);
        noise = Mathf.PerlinNoise(xCoord, yCoord);
        color = new Color(noise, noise, noise, 1);
        raisedHideNoiseTexturePixels[y * raisedHideNoiseTexture.width + x] = color;

        x_val = xOrg * Globals.instance.terrainSize + x + 500;
        y_val = yOrg * Globals.instance.terrainSize + y + 500;
        xCoord = (float)x_val / ((float)Globals.instance.terrainSize * 0.5f);
        yCoord = (float)y_val / ((float)Globals.instance.terrainSize * 0.5f);
        noise = Mathf.PerlinNoise(xCoord, yCoord);
        color = new Color(noise, noise, noise, 1);
        eagleNoiseTexturePixels[y * eagleNoiseTexture.width + x] = color;

        x_val = xOrg * Globals.instance.terrainSize + x + 50;
        y_val = yOrg * Globals.instance.terrainSize + y + 50;
        xCoord = (float)x_val / ((float)Globals.instance.terrainSize * 0.15f);
        yCoord = (float)y_val / ((float)Globals.instance.terrainSize * 0.15f);
        noise = Mathf.PerlinNoise(xCoord, yCoord);
        color = new Color(noise, noise, noise, 1);
        lighthouseNoiseTexturePixels[y * lighthouseNoiseTexture.width + x] = color;

        return tree_noise_level;
    }

    IEnumerator CreateTerrainMesh()
    {
        isTerrainCreated = false;
        isTerrainGenerationOngoing = true;

        terrainGenerationCompletion = 0;
        float max_tiles_for_generation = (Globals.instance.terrainSize * Globals.instance.terrainSize) * 0.01f;

        if (combinedMesh == null)
        {
            combinedMesh = new CombinedMesh(new Vector3Int((int)terrainCenter.position.x, (int)terrainCenter.position.y, (int)terrainCenter.position.z));
        }
        combinedMesh.PrepareMeshGeneration(new Vector2Int(xOrg, yOrg));

        for (int y = 0; y < Globals.instance.terrainSize; y++)
        {
            for (int x = 0; x < Globals.instance.terrainSize; x++)
            {
                combinedMesh.AddTileToMesh(x, y, SCALE, true, 0);

                terrainGenerationCompletion += 1 / max_tiles_for_generation;

                float tree_noise = UpdateNoiseMasksForGeneration(x, y);

                if (terrainGenerationIsAnimated)
                {
                    GameObject go = Instantiate(tilePrefab);
                    go.transform.parent = tileGenerationAnimationParent;
                    go.transform.position += new Vector3(0, -3.01f - combinedMesh.tiles[x, y].topFaceMiddlePosition.y, 0);
                    go.GetComponent<MeshFilter>().mesh = combinedMesh.tiles[x, y].GetOptimizedProceduralCubeMesh();
                    go.name += "_" + x.ToString() + "_" + y.ToString();

                    GenerateTrees(x, y, tree_noise);
                    GenerateRocks(x, y, 1 - tree_noise);

                    yield return null;
                }
            }

            if (!terrainGenerationIsAnimated)
            {
                yield return null;
            }
            else
            {
                combinedMesh.VisualizeMesh(transform);
            }
        }

        if (terrainGenerationIsAnimated)
        {
            StartCoroutine(AnimateWaterGeneration());
        }
        waterCube.SetActive(true);

        isTerrainCreated = true;
        isTerrainGenerationOngoing = false;

        if (!terrainGenerationIsAnimated)
            combinedMesh.VisualizeMesh(transform);

        UpdateNoiseMaskTextures();
        GenerateEnvironment();
    }

    IEnumerator AnimateWaterGeneration()
    {
        waterCube.transform.localScale = new Vector3(waterCube.transform.localScale.x, waterCubeScaleY / 100f, waterCube.transform.localScale.z);

        while (waterCube.transform.localScale.y < waterCubeScaleY - 0.01f)
        {
            float y_scale = Mathf.Lerp(waterCube.transform.localScale.y, waterCubeScaleY, 2f * Time.deltaTime);
            waterCube.transform.localScale = new Vector3(waterCube.transform.localScale.x, y_scale, waterCube.transform.localScale.z);

            yield return null;
        }

        waterCube.transform.localScale = new Vector3(waterCube.transform.localScale.x, waterCubeScaleY, waterCube.transform.localScale.z);
    }

    private void UpdateNoiseMaskTextures()
    {
        treeNoiseTexture.SetPixels32(treeNoiseTexturePixels);
        treeNoiseTexture.Apply();

        smallNoiseTexture.SetPixels32(smallNoiseTexturePixels);
        smallNoiseTexture.Apply();

        mediumNoiseTexture.SetPixels32(mediumNoiseTexturePixels);
        mediumNoiseTexture.Apply();

        largeNoiseTexture.SetPixels32(largeNoiseTexturePixels);
        largeNoiseTexture.Apply();

        eagleNoiseTexture.SetPixels32(eagleNoiseTexturePixels);
        eagleNoiseTexture.Apply();

        raisedHideNoiseTexture.SetPixels32(raisedHideNoiseTexturePixels);
        raisedHideNoiseTexture.Apply();

        lighthouseNoiseTexture.SetPixels32(lighthouseNoiseTexturePixels);
        lighthouseNoiseTexture.Apply();
    }

    private void ClearEnvironment()
    {
        foreach (Transform child in environmentParent.transform)
            Destroy(child.gameObject);
        foreach (Transform child in wildlifeParent.transform)
            Destroy(child.gameObject);
        foreach (Transform child in housesAndVehiclesParent.transform)
            Destroy(child.gameObject);
    }

    private void GenerateEnvironment()
    {
        Vector2Int eagle_position = new Vector2Int(-1, -1);

        bool is_beach = false;
        isBeach = new bool[50, 50];
        float maximum_mountain_eagle_noise = 0;
        boatHousesGenerated = 0;
        lightHouseGenerated = false;
        senderStationGenerated = false;

        GenerateSummitCross();
        
        for (int z = 0; z < Globals.instance.terrainSize; z++)
        {
            for (int x = 0; x < Globals.instance.terrainSize; x++)
            {
                if (!terrainGenerationIsAnimated)
                {
                    GenerateTrees(x, z, treeNoiseTexture.GetPixel(x, z).r);
                    GenerateRocks(x, z, 1 - treeNoiseTexture.GetPixel(x, z).r);
                }

                GenerateRaisedHide(x, z);
                GenerateWindmills(x, z);
                GenerateBoats(x, z);
                GenerateBeaches(x, z, ref is_beach);
                GenerateBoatHouse(x, z, is_beach);
                GenerateSeagulls(x, z, is_beach);
                GenerateEagle(x, z, ref eagle_position, ref maximum_mountain_eagle_noise);
                GenerateLighthouse(x, z, is_beach);
                GenerateSenderStation(x, z);
            }
        }

        /* This update will draw beaches on tiles that were determined in GenerateEnvironment() */
        combinedMesh.UpdateMeshUVs();
    }

    private void GenerateLighthouse(int x, int y, bool is_beach)
    {
        if (combinedMesh.tiles[x, y].isOccupied || !is_beach || lightHouseGenerated)
        {
            return;
        }

        float noise = lighthouseNoiseTexture.GetPixel(x, y).r;

        if (noise > 0.75f)
        {
            int large_sea = 4;

            int min_x = x - 11;
            if (min_x < 0)
                min_x = 0;
            if (combinedMesh.tiles[min_x, y].noiseValue > Globals.instance.GetSeaLevel())
                large_sea--;

            int max_x = x + 11;
            if (max_x >= Globals.instance.terrainSize)
                max_x = Globals.instance.terrainSize - 1;
            if (combinedMesh.tiles[max_x, y].noiseValue > Globals.instance.GetSeaLevel())
                large_sea--;

            int min_y = y - 11;
            if (min_y < 0)
                min_y = 0;
            if (combinedMesh.tiles[x, min_y].noiseValue > Globals.instance.GetSeaLevel())
                large_sea--;

            int max_y = y + 11;
            if (max_y >= Globals.instance.terrainSize)
                max_y = Globals.instance.terrainSize - 1;
            if (combinedMesh.tiles[x, max_y].noiseValue > Globals.instance.GetSeaLevel())
                large_sea--;

            if (large_sea <= 0)
                return;
 
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/Lighthouse"), housesAndVehiclesParent.transform);
            float rotation = ((int)(Unity.Mathematics.math.remap(0, noise, 0, 4, (float)x % noise)) % 4) * 90;
            go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, rotation, go.transform.rotation.eulerAngles.z);

            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z);
            else
                go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition;
            go.name += "_" + x.ToString() + "_" + y.ToString();

            lightHouseGenerated = true;

            combinedMesh.tiles[x, y].isOccupied = true;
        }
    }

    private void GenerateBoatHouse(int x, int y, bool is_beach)
    {
        if (combinedMesh.tiles[x, y].isOccupied || !is_beach || boatHousesGenerated >= 2)
        {
            return;
        }

        float noise = eagleNoiseTexture.GetPixel(x, y).r;

        if (((noise > 0.5f) && (boatHousesGenerated == 0)) || ((noise < (0.15f + (float)boatHousesGenerated / 10f)) && (boatHousesGenerated > 0)))
        {
            GameObject go;

            if (x < 1 || x >= Globals.instance.terrainSize - 1 || y < 1 || y >= Globals.instance.terrainSize - 1)
                return;
            if ((combinedMesh.tiles[x - 1, y].noiseValue < combinedMesh.tiles[x, y].noiseValue) && (combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel()))
            {
                go = (GameObject)Instantiate(Resources.Load("Prefabs/BoatHouse"), housesAndVehiclesParent.transform);
                go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, -90, go.transform.rotation.eulerAngles.z);
                
            }
            else if((combinedMesh.tiles[x, y - 1].noiseValue < combinedMesh.tiles[x, y].noiseValue) && (combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel()))
            {
                go = (GameObject)Instantiate(Resources.Load("Prefabs/BoatHouse"), housesAndVehiclesParent.transform);
                go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, 180, go.transform.rotation.eulerAngles.z);
            }
            else if ((combinedMesh.tiles[x + 1, y].noiseValue < combinedMesh.tiles[x, y].noiseValue) && (combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel()))
            {
                go = (GameObject)Instantiate(Resources.Load("Prefabs/BoatHouse"), housesAndVehiclesParent.transform);
                go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, 90, go.transform.rotation.eulerAngles.z);
            }
            else if ((combinedMesh.tiles[x, y + 1].noiseValue < combinedMesh.tiles[x, y].noiseValue) && (combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel()))
            {
                go = (GameObject)Instantiate(Resources.Load("Prefabs/BoatHouse"), housesAndVehiclesParent.transform);
                go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, 0, go.transform.rotation.eulerAngles.z);
            }
            else
                return;

            boatHousesGenerated++;

            go.name += "_" + x.ToString() + "_" + y.ToString();
            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z);
            else
                go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition;
            combinedMesh.tiles[x, y].isOccupied = true;
        }
    }

    private void GenerateWindmills(int x, int y)
    {
        if (combinedMesh.tiles[x, y].isOccupied)
        {
            return;
        }

        if (y == 0 || x == 0 || y == Globals.instance.terrainSize - 1 || x == Globals.instance.terrainSize - 1 ||
            combinedMesh.tiles[x, y].noiseValue < Globals.instance.terrainLevels[(int)Globals.TerrainLevels.grass1] ||
            combinedMesh.tiles[x, y].noiseValue > Globals.instance.terrainLevels[(int)Globals.TerrainLevels.rock1])
        {
            return;
        }

        float noise = mediumNoiseTexture.GetPixel(x, y).r;

        if (noise >= mediumNoiseTexture.GetPixel(x - 1, y).r ||
            noise >= mediumNoiseTexture.GetPixel(x + 1, y).r ||
            noise >= mediumNoiseTexture.GetPixel(x, y - 1).r ||
            noise >= mediumNoiseTexture.GetPixel(x, y + 1).r ||
            noise >= mediumNoiseTexture.GetPixel(x + 1, y + 1).r ||
            noise >= mediumNoiseTexture.GetPixel(x + 1, y - 1).r ||
            noise >= mediumNoiseTexture.GetPixel(x - 1, y - 1).r ||
            noise >= mediumNoiseTexture.GetPixel(x - 1, y + 1).r)
        {
            return;
        }

        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/Windmill"), housesAndVehiclesParent.transform);

        if (terrainGenerationIsAnimated)
            go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z);
        else
            go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition;

        /* this will face the windmill away from the highest point on the terrain */
        Vector2 destination_vector = combinedMesh.highestTile - new Vector2(x, y);
        float windmill_angle = Vector2.Angle(Vector2.up, destination_vector);
        if (destination_vector.x < 0)
            windmill_angle *= -1;
        windmill_angle += 180;

        Vector3 windmill_rotation = new Vector3(go.transform.rotation.eulerAngles.x, windmill_angle, go.transform.rotation.eulerAngles.z);

        go.transform.rotation = Quaternion.Euler(windmill_rotation.x, windmill_rotation.y, windmill_rotation.z);
        go.name += "_" + x.ToString() + "_" + y.ToString();

        combinedMesh.tiles[x, y].isOccupied = true;
    }

    private void GenerateBoats(int x, int y)
    {
        if (combinedMesh.tiles[x, y].isOccupied)
        {
            return;
        }

        if ((combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel() - 0.01f) &&
            (combinedMesh.tiles[x, y].noiseValue > Globals.instance.terrainLevels[(int)Globals.TerrainLevels.water2]))
        {
            int random = UnityEngine.Random.Range(0, 100);

            GameObject go = null;

            if (random < BOAT_SPAWN_CHANCE_PERCENTAGE_PER_TILE)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Boat_Small"), housesAndVehiclesParent.transform);
            else
                return;

            float boat_angle = UnityEngine.Random.Range(0, 360);

            Vector3 boat_position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, (Globals.instance.GetSeaLevel() * Globals.instance.heightScale) + 1, combinedMesh.tiles[x, y].topFaceMiddlePosition.z);
            Vector3 boat_rotation = new Vector3(go.transform.rotation.eulerAngles.x, boat_angle, go.transform.rotation.eulerAngles.z);

            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(boat_position.x, 80, boat_position.z);
            else
                go.transform.position = boat_position;

            go.transform.rotation = Quaternion.Euler(boat_rotation.x, boat_rotation.y, boat_rotation.z);
            go.name += "_" + x.ToString() + "_" + y.ToString();

            combinedMesh.tiles[x, y].isOccupied = true;
        }
    }

    private void GenerateBeaches(int x, int y, ref bool is_beach)
    {
        bool is_tile_beach = false;
        if ((combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel() + MAX_BEACH_HEIGHT) &&
            (combinedMesh.tiles[x, y].noiseValue > Globals.instance.GetSeaLevel() - MIN_BEACH_HEIGHT))
        {
            if (x > 0 &&
                x < (combinedMesh.tiles.GetLength(1) - 1) &&
                y > 0 &&
                y < (combinedMesh.tiles.GetLength(0) - 1))
            {
                if (combinedMesh.tiles[x - 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x - 1, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x + 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                    combinedMesh.tiles[x + 1, y + 1].noiseValue < Globals.instance.GetSeaLevel())
                    is_tile_beach = true;
            }
            else
            {
                if(x == 0)
                {
                    if (y == 0)
                    {
                        if (combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y + 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                    else if (y >= combinedMesh.tiles.GetLength(0) - 1)
                    {
                        if (combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                    else
                    {
                        if (combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y + 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                }
                else if(x >= combinedMesh.tiles.GetLength(1) - 1)
                {
                    if (y == 0)
                    {
                        if (combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                    else if (y >= combinedMesh.tiles.GetLength(0) - 1)
                    {
                        if (combinedMesh.tiles[x - 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                    else
                    {
                        if (combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                }
                else
                {
                    if (y == 0)
                    {
                        if (combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y + 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                    else if (y >= combinedMesh.tiles.GetLength(0) - 1)
                    {
                        if (combinedMesh.tiles[x - 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                    else
                    {
                        if (combinedMesh.tiles[x - 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x - 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y + 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y - 1].noiseValue < Globals.instance.GetSeaLevel() ||
                            combinedMesh.tiles[x + 1, y + 1].noiseValue < Globals.instance.GetSeaLevel())
                            is_tile_beach = true;
                    }
                }
            }
        }

        is_beach = is_tile_beach;

        if (is_tile_beach)
        {
            isBeach[x, y] = true;
            combinedMesh.tiles[x, y].ModifyUVs(Globals.instance.NoiseToUV(Globals.instance.GetSeaLevel(), true));

            int uv_index_start = y * Globals.instance.terrainSize * combinedMesh.tiles[x, y].uvs.Length + x * combinedMesh.tiles[x, y].uvs.Length;
            if (uv_index_start < 0)
                uv_index_start = 0;

            int uv_index_end = uv_index_start + combinedMesh.tiles[x, y].uvs.Length;

            int j = 0;
            for (int i = uv_index_start; i < uv_index_end; i++)
            {
                combinedMesh.combinedMeshUVs[i] = combinedMesh.tiles[x, y].uvs[j++];
            }
        }
    }

    private void GenerateSeagulls(int x, int y, bool is_beach)
    {
        if (!is_beach)
            return;

        if ((combinedMesh.tiles[x, y].noiseValue < Globals.instance.GetSeaLevel()) &&
            (combinedMesh.tiles[x, y].noiseValue > Globals.instance.GetSeaLevel() - MAX_SEAGULL_SPAWN_WATER_DEPTH))
        {
            int random = UnityEngine.Random.Range(MIN_SEAGULLS_PER_TILE, MAX_SEAGULLS_PER_TILE + 1);
            for (int i = 0; i < random; i++)
            {
                GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/Seagull"), wildlifeParent.transform);
                go.name += "_" + x.ToString() + "_" + y.ToString();
                float rndm = UnityEngine.Random.Range(0, 360);
                float randomized_pos = rndm / 1000f;
                go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition + new Vector3(randomized_pos, SEAGULL_SPAWN_HEIGHT + (randomized_pos - 0.18f), randomized_pos);
                go.transform.rotation = Quaternion.Euler(0, rndm, 0);
            }
        }
    }

    private void GenerateTrees(int x, int y, float tree_noise_level)
    {
        if (combinedMesh.tiles[x, y].isOccupied)
        {
            return;
        }

        GameObject go = null;

        if ((Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.grass1) &&
            (combinedMesh.tiles[x, y].noiseValue > (Globals.instance.GetSeaLevel() + Globals.instance.terrainLevels[(int)Globals.TerrainLevels.grass1]) / 2) &&
            (tree_noise_level > TREE_MIN_NOISE_LEVEL_PER_TILE) && (tree_noise_level < TREE_MAX_NOISE_LEVEL_PER_TILE))
        {
            float tree_chance = Mathf.InverseLerp(TREE_MIN_NOISE_LEVEL_PER_TILE, TREE_MAX_NOISE_LEVEL_PER_TILE, tree_noise_level) * 100;

            if (tree_chance <= 20)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple6"), environmentParent.transform);
            else if (tree_chance <= 40)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple7"), environmentParent.transform);
            else if (tree_chance <= 50)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple11"), environmentParent.transform);
            else if (tree_chance <= 55)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump1"), environmentParent.transform);
            else if (tree_chance <= 60)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump2"), environmentParent.transform);
            else
                return;
        }
        else if ((Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.grass2) &&
            (tree_noise_level > TREE_MIN_NOISE_LEVEL_PER_TILE) && (tree_noise_level < TREE_MAX_NOISE_LEVEL_PER_TILE))
        {
            float tree_chance = Mathf.InverseLerp(TREE_MIN_NOISE_LEVEL_PER_TILE, TREE_MAX_NOISE_LEVEL_PER_TILE, tree_noise_level) * 100;

            if (tree_chance <= 10)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple6"), environmentParent.transform);
            else if (tree_chance <= 75)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple11"), environmentParent.transform);
            else if (tree_chance <= 90)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple12"), environmentParent.transform);
            else if (tree_chance <= 93)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Dead1"), environmentParent.transform);
            else if (tree_chance <= 96)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump1"), environmentParent.transform);
            else
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump2"), environmentParent.transform);
        }
        else if ((Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.grass3) &&
            (tree_noise_level > TREE_MIN_NOISE_LEVEL_PER_TILE) && (tree_noise_level < TREE_MAX_NOISE_LEVEL_PER_TILE))
        {
            float tree_chance = Mathf.InverseLerp(TREE_MIN_NOISE_LEVEL_PER_TILE, TREE_MAX_NOISE_LEVEL_PER_TILE, tree_noise_level) * 100;

            if (tree_chance <= 20)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple11"), environmentParent.transform);
            else if (tree_chance <= 60)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple12"), environmentParent.transform);
            else if (tree_chance <= 80)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_simple13"), environmentParent.transform);
            else if (tree_chance <= 90)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Dead1"), environmentParent.transform);
            else if (tree_chance <= 95)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump1"), environmentParent.transform);
            else
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump2"), environmentParent.transform);
        }
        else if ((Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.rock1) &&
            (tree_noise_level > TREE_MIN_NOISE_LEVEL_PER_TILE) && (tree_noise_level < TREE_MAX_NOISE_LEVEL_PER_TILE))
        {
            float tree_chance = Mathf.InverseLerp(TREE_MIN_NOISE_LEVEL_PER_TILE, TREE_MAX_NOISE_LEVEL_PER_TILE, tree_noise_level) * 100;

            if (tree_chance <= 80)
                return;
            else if (tree_chance <= 87)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Dead1"), environmentParent.transform);
            else if (tree_chance <= 94)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump1"), environmentParent.transform);
            else
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Tree_Stump2"), environmentParent.transform);
        }

        if (go != null)
        {
            go.name += "_" + x.ToString() + "_" + y.ToString();

            int random = UnityEngine.Random.Range(0, 100);

            Vector3 randomized_position = new Vector3(random / 500f, 0, random / 500f);

            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z) + randomized_position;
            else
                go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition + randomized_position;

            Vector3 euler_angles = go.transform.rotation.eulerAngles;
            float variation = ((float)random - 49.5f) / 10f;
            go.transform.rotation = Quaternion.Euler(euler_angles.x + variation, euler_angles.y + ((float)random - 49.5f) * 3.6f, euler_angles.z + variation);

            Vector3 local_scale = go.transform.localScale;
            float scale_variation = ((float)random - 49.5f) / 500;
            go.transform.localScale = new Vector3(local_scale.x + scale_variation, local_scale.y + scale_variation * 2, local_scale.z + scale_variation);

            if (!terrainGenerationIsAnimated)
                go.transform.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            combinedMesh.tiles[x, y].isOccupied = true;
        }
    }

    private void GenerateEagle(int x, int y, ref Vector2Int eagle_position, ref float maximum_mountain_eagle_noise)
    {
        float noise = eagleNoiseTexture.GetPixel(x, y).r;

        if (Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.rock2 &&
            noise > maximum_mountain_eagle_noise)
        {
            maximum_mountain_eagle_noise = noise;
            eagle_position.Set(x, y);
        }

        if (x < Globals.instance.terrainSize - 1 || y < Globals.instance.terrainSize - 1)
            return;

        if (eagle_position.x >= 0 && eagle_position.y >= 0)
        {
            // Bit shift the index of the layer (6) to get a bit mask
            int layerMask = 1 << 6;

            GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/Nest"), wildlifeParent.transform);
            go.name += "_" + x.ToString() + "_" + y.ToString();
            go.transform.position = combinedMesh.tiles[eagle_position.x, eagle_position.y].topFaceMiddlePosition;

            RaycastHit hit;
            Vector3 origin = go.transform.position + new Vector3(0, 0.01f, 0);
            Vector3 mountain_eagle_offset_position = Vector3.up;
            if (Physics.Raycast(origin, Vector3.forward, out hit, 2, layerMask))
                mountain_eagle_offset_position += Vector3.back * 2;
            else if (Physics.Raycast(origin, Vector3.back, out hit, 2, layerMask))
                mountain_eagle_offset_position += Vector3.forward * 2;
            else if (Physics.Raycast(origin, Vector3.left, out hit, 2, layerMask))
                mountain_eagle_offset_position += Vector3.right * 2;
            else if (Physics.Raycast(origin, Vector3.right, out hit, 2, layerMask))
                mountain_eagle_offset_position += Vector3.left * 2;

            go = (GameObject)Instantiate(Resources.Load("Prefabs/Eagle"), wildlifeParent.transform);
            go.transform.position = combinedMesh.tiles[eagle_position.x, eagle_position.y].topFaceMiddlePosition + mountain_eagle_offset_position;
        }
    }

    private void GenerateSummitCross()
    {
        int x = combinedMesh.highestTile.x;
        int y = combinedMesh.highestTile.y;

        if (combinedMesh.tiles[x, y].noiseValue >= 0.8f)
        { 
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/SummitCross"), environmentParent.transform);
            go.name += "_" + x.ToString() + "_" + y.ToString();
            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z);
            else
                go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition;

            int random = UnityEngine.Random.Range(0, 10);
            Vector3 euler_angles = go.transform.rotation.eulerAngles;
            float variation = ((float)random - 4.95f) / 2;
            go.transform.rotation = Quaternion.Euler(euler_angles.x + variation, euler_angles.y + ((float)random - 4.95f) * 36f, euler_angles.z + variation);
        }
    }

    private void GenerateRaisedHide(int x, int y)
    {
        if (combinedMesh.tiles[x, y].isOccupied)
        {
            return;
        }

        if (y == 0 || x == 0 || y == Globals.instance.terrainSize - 1 || x == Globals.instance.terrainSize - 1 ||
            combinedMesh.tiles[x, y].noiseValue < Globals.instance.terrainLevels[(int)Globals.TerrainLevels.grass1] ||
            combinedMesh.tiles[x, y].noiseValue > Globals.instance.terrainLevels[(int)Globals.TerrainLevels.rock1])
        { 
            return;
        }

        float noise = raisedHideNoiseTexture.GetPixel(x, y).r;

        if (noise >= raisedHideNoiseTexture.GetPixel(x - 1, y).r ||
            noise >= raisedHideNoiseTexture.GetPixel(x + 1, y).r ||
            noise >= raisedHideNoiseTexture.GetPixel(x, y - 1).r ||
            noise >= raisedHideNoiseTexture.GetPixel(x, y + 1).r)
        {
            return;
        }

        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/RaisedHide"), housesAndVehiclesParent.transform);
        go.name += "_" + x.ToString() + "_" + y.ToString();
        Vector3 temp_position = combinedMesh.tiles[x, y].topFaceMiddlePosition;
        if (terrainGenerationIsAnimated)
            go.transform.position = new Vector3(temp_position.x, 80, temp_position.z);
        else
            go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition;
        int rotation = ((int)(combinedMesh.tiles[x, y].noiseValue * 1000) % 4) * 90;
        go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, rotation, go.transform.rotation.eulerAngles.z);
    }

    private void GenerateRocks(int x, int y, float noise)
    {
        if (combinedMesh.tiles[x, y].isOccupied)
        {
            return;
        }

        if (((Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.rock1) && (noise > 0.45f) && (noise < 0.55f)) ||
            ((Globals.instance.NoiseToTerrainLevel(combinedMesh.tiles[x, y].noiseValue) == Globals.TerrainLevels.rock2) && (noise > 0.48f) && (noise < 0.52f)))
        {
            GameObject go;

            float rock_chance = Unity.Mathematics.math.remap(0, noise, 0, 1, (xOrg + x + y + yOrg) % noise);

            if (rock_chance <= 0.33f)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Rock_1"), environmentParent.transform);
            else if (rock_chance <= 0.66f)
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Rock_2"), environmentParent.transform);
            else
                go = (GameObject)Instantiate(Resources.Load("Prefabs/Rock_3"), environmentParent.transform);

            float deviation = Unity.Mathematics.math.remap(0, 1 - noise, -0.2f, 0.2f, (xOrg + x + y + yOrg) % (1 - noise));
            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x + deviation, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z + deviation);
            else
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x + deviation, combinedMesh.tiles[x, y].topFaceMiddlePosition.y, combinedMesh.tiles[x, y].topFaceMiddlePosition.z + deviation);
            go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, deviation * 900, go.transform.rotation.eulerAngles.z);
            go.transform.localScale = go.transform.localScale + go.transform.localScale * deviation;
            go.name += "_" + x.ToString() + "_" + y.ToString();

            if (!terrainGenerationIsAnimated)
                go.transform.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            combinedMesh.tiles[x, y].isOccupied = true;
        }
    }

    private void GenerateSenderStation(int x, int y)
    {
        if (combinedMesh.tiles[x, y].isOccupied || senderStationGenerated)
        {
            return;
        }

        float noise = 1 - eagleNoiseTexture.GetPixel(x, y).r;

        if ((noise > 0.8f) &&
            (combinedMesh.tiles[x, y].noiseValue > Globals.instance.terrainLevels[(int)Globals.TerrainLevels.grass1]) &&
            (combinedMesh.tiles[x, y].noiseValue < Globals.instance.terrainLevels[(int)Globals.TerrainLevels.rock1]))
        {
            GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/SenderStation"), housesAndVehiclesParent.transform);
            go.name += "_" + x.ToString() + "_" + y.ToString();

            if (terrainGenerationIsAnimated)
                go.transform.position = new Vector3(combinedMesh.tiles[x, y].topFaceMiddlePosition.x, 80, combinedMesh.tiles[x, y].topFaceMiddlePosition.z);
            else
                go.transform.position = combinedMesh.tiles[x, y].topFaceMiddlePosition;

            int rotation = ((int)(combinedMesh.tiles[x, y].noiseValue * 1000) % 4) * 90;
            go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, rotation, go.transform.rotation.eulerAngles.z);

            senderStationGenerated = true;

            combinedMesh.tiles[x, y].isOccupied = true;
        }
    }

    public void GenerateWorld(bool animate_generation)
    {
        terrainGenerationIsAnimated = animate_generation;
        StartCoroutine(CreateTerrainMesh());
    }

    public void DestroyGeneratedWorld()
    {
        if (combinedMesh != null)
        {
            waterCube.SetActive(false);

            combinedMesh.ClearMesh();
            Destroy(combinedMesh.gameObject);
            ClearEnvironment();
        }
    }
}
