using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals instance;

    /* Global variables **************************************************************************************************** */

    /* Terrain generation */
    [Header("Terrain dimensions")]
    [Tooltip("This value multiplied by the noise (0.0-1.0) and the Animation Curves results in the tile height.")]
    [Range(1,100)]
    public int heightScale = 10;
    [Tooltip("This value represents the amount of rows (x) and columns (y) of tiles.")]
    [Range(50,50)]
    public int terrainSize = 50;
    public AnimationCurve waterLevelTransition;
    public AnimationCurve grassLevelTransition;
    public AnimationCurve mountainLevelTransition;
    public float[] terrainLevels;
    public Vector3 terrainCenter = new Vector3(25, 0, 25);

    public enum TerrainLevels
    {
        water1 = 0,
        water2 = 1,
        water3 = 2,
        grass1 = 3,
        grass2 = 4,
        grass3 = 5,
        rock1 = 6,
        rock2 = 7,
        rock3 = 8
    };

    /* Terrain generation */
    [Header("Terrain colors")]
    public Color waterColorDeep = new Color(0, 0, 0);
    public Color waterColor = new Color(0, 0, 0);
    public Color waterColorShallow = new Color(0, 0, 0);
    public Color sandColor = new Color(0, 0, 0);
    public Color grassColor = new Color(0, 0, 0);
    public Color grassHighlandColor = new Color(0, 0, 0);
    public Color dirtColor = new Color(0, 0, 0);
    public Color rockColor = new Color(0, 0, 0);
    public Color rock2Color = new Color(0, 0, 0);
    public Color snowColor = new Color(0, 0, 0);

    private Texture2D terrainColor;

    /* Terrain materials */
    [Header("Terrain materials")]
    public Material combinedMeshColorMaterial = null;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Instance of Globals already exists in scene!", this);
            this.enabled = false; //Disable instead of destroy so you can see it in inspector and stop whatever has 2 of them
            return;
        }
        instance = this;

        CreateTerrainColorTexture();
    }

    public float GetSeaLevel()
    {
        return terrainLevels[(int)TerrainLevels.water3];
    }

    public float GetMountainlevel()
    {
        return terrainLevels[(int)TerrainLevels.grass3];
    }

    private void CreateTerrainColorTexture()
    {
        terrainLevels = new float[] { 0.09f, 0.15f, 0.22f, 0.37f, 0.54f, 0.66f, 0.7f, 0.89f };

        terrainColor = new Texture2D(terrainLevels.Length + 1 + 100, 1);
        terrainColor.filterMode = FilterMode.Point;
        terrainColor.SetPixel(0, 0, waterColorDeep);
        terrainColor.SetPixel(1, 0, waterColor);
        terrainColor.SetPixel(2, 0, waterColorShallow);
        terrainColor.SetPixel(3, 0, grassColor);
        terrainColor.SetPixel(4, 0, grassHighlandColor);
        terrainColor.SetPixel(5, 0, dirtColor);
        terrainColor.SetPixel(6, 0, rockColor);
        terrainColor.SetPixel(7, 0, rock2Color);
        terrainColor.SetPixel(8, 0, snowColor);
        for(int i = 9; i < 103; i++)
            terrainColor.SetPixel(i, 0, Color.magenta);
        terrainColor.SetPixel(103, 0, sandColor);
        for (int i = 104; i < terrainColor.width; i++)
            terrainColor.SetPixel(i, 0, Color.magenta);

        terrainColor.Apply();

        combinedMeshColorMaterial.mainTexture = terrainColor;
    }

    public float NoiseToUV(float noise, bool is_sand)
    {
        const float uv_offset = 0.001f;

        int i = 0;
        for (; i < terrainLevels.Length; i++)
        {
            if(noise < terrainLevels[i])
            {
                break;
            }
        }

        if (is_sand)
            return ((float)(i + 100) / terrainColor.width) + uv_offset;
        else
            return ((float)i / terrainColor.width) + uv_offset;
    }

    public TerrainLevels NoiseToTerrainLevel(float noise)
    { 
        int i = 0;
        for (; i < terrainLevels.Length; i++)
        {
            if (noise < terrainLevels[i])
            {
                return (TerrainLevels)i;
            }
        }

        return (TerrainLevels)i;
    }
}
