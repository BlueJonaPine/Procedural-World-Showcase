using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedMesh
{
    public Tile[,] tiles = null;
    private List<Tile> activeMeshTiles = new List<Tile>();
    public Vector2Int highestTile = new Vector2Int(0, 0);
    private float highestNoise;
    private Vector2Int noiseOffset;

    public GameObject gameObject = null;

    private string combinedMeshName;
    private Vector3Int combinedMeshOrigin;
    private Vector3[] combinedMeshVertices;
    private int[] combinedMeshTriangles;
    public Vector2[] combinedMeshUVs;

    public CombinedMesh(Vector3Int mesh_center)
    {
        combinedMeshName = "Combined_Mesh_" + mesh_center.x.ToString() + "_" + mesh_center.y.ToString() + "_" + mesh_center.z.ToString();
        int offset = Globals.instance.terrainSize / 2;
        combinedMeshOrigin = new Vector3Int(mesh_center.x - offset, mesh_center.y, mesh_center.z - offset);
    }

    public void PrepareMeshGeneration(Vector2Int offset)
    {
        tiles = new Tile[Globals.instance.terrainSize, Globals.instance.terrainSize];
        activeMeshTiles.Clear();

        noiseOffset = offset;

        highestNoise = -1;

        combinedMeshVertices = null;
        combinedMeshTriangles = null;
        combinedMeshUVs = null;
    }

    public void AddTileToMesh(int x, int y, float frequency, bool generate_noise, float non_generated_noise)
    {
        float noise;

        if (generate_noise)
        {
            float xCoord = (float)(noiseOffset.x * Globals.instance.terrainSize + x) / ((float)Globals.instance.terrainSize * frequency);
            float yCoord = (float)(noiseOffset.y * Globals.instance.terrainSize + y) / ((float)Globals.instance.terrainSize * frequency);

            noise = Mathf.PerlinNoise(xCoord, yCoord);
            noise = (float)Math.Round((decimal)noise, 4, MidpointRounding.ToEven);
            if (noise < 0)
                noise = 0;
            else if (noise > 1)
                noise = 1;
            tiles[x, y] = new Tile(noise);
        }
        else
            noise = non_generated_noise;

        float cube_y_size = CalculateCubeHeight(noise);
        if (noise > highestNoise)
        {
            highestNoise = noise;
            highestTile.x = x;
            highestTile.y = y;
        }

        tiles[x, y].GenerateProceduralCube(new Vector3Int(x, combinedMeshOrigin.y, y), new Vector3(1, cube_y_size, 1), Globals.instance.NoiseToUV(noise, false));

        activeMeshTiles.Add(tiles[x, y]);

        if (combinedMeshVertices == null)
        {
            combinedMeshVertices = tiles[x, y].vertices;
            combinedMeshTriangles = tiles[x, y].triangles;
            combinedMeshUVs = tiles[x, y].uvs;
        }
        else
        {
            int vertices_length_buffer = combinedMeshVertices.Length;

            Vector3[] vertices = new Vector3[tiles[x, y].vertices.Length + combinedMeshVertices.Length];
            combinedMeshVertices.CopyTo(vertices, 0);
            tiles[x, y].vertices.CopyTo(vertices, combinedMeshVertices.Length);
            combinedMeshVertices = vertices;

            int[] triangles = new int[tiles[x, y].triangles.Length + combinedMeshTriangles.Length];
            combinedMeshTriangles.CopyTo(triangles, 0);
            for (int i = 0; i < tiles[x, y].triangles.Length; i++)
            {
                triangles[i + combinedMeshTriangles.Length] = tiles[x, y].triangles[i] + vertices_length_buffer;
            }
            combinedMeshTriangles = triangles;

            Vector2[] uvs = new Vector2[tiles[x, y].uvs.Length + combinedMeshUVs.Length];
            combinedMeshUVs.CopyTo(uvs, 0);
            tiles[x, y].uvs.CopyTo(uvs, combinedMeshUVs.Length);
            combinedMeshUVs = uvs;
        }
    }

    public void VisualizeMesh(Transform parent_transform)
    {
        if (parent_transform != null && tiles != null)
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(combinedMeshName);
                gameObject.isStatic = true;
                gameObject.transform.parent = parent_transform;
                gameObject.transform.position = combinedMeshOrigin;
                gameObject.AddComponent<MeshFilter>();
                gameObject.AddComponent<MeshRenderer>();
                gameObject.AddComponent<MeshCollider>();
                gameObject.layer = 6;
            }

            if (activeMeshTiles.Count != 0)
            {
                Mesh mesh = new Mesh();
                mesh.name = "Mesh of " + combinedMeshName;
                mesh.Clear();
                mesh.vertices = combinedMeshVertices;
                mesh.triangles = combinedMeshTriangles;
                mesh.uv = combinedMeshUVs;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                //mesh.Optimize();
                gameObject.GetComponent<MeshFilter>().mesh = mesh;
                gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            }

            gameObject.GetComponent<MeshRenderer>().sharedMaterial = Globals.instance.combinedMeshColorMaterial;
        }
    }

    public void UpdateMeshUVs()
    {
        gameObject.GetComponent<MeshFilter>().mesh.uv = combinedMeshUVs;
    }

    private float CalculateCubeHeight(float noise)
    {
        float height_scale = Globals.instance.heightScale;

        float cube_height = 1;
        float sea_level = Globals.instance.GetSeaLevel();
        float mountain_level = Globals.instance.GetMountainlevel();

        if (noise <= sea_level)
        {
            float linearization = noise / sea_level;
            cube_height += Globals.instance.waterLevelTransition.Evaluate(linearization) * height_scale * sea_level;
        }
        else if (noise <= mountain_level)
        {
            float linearization = (noise - sea_level) / (mountain_level - sea_level);
            cube_height += height_scale * sea_level + Globals.instance.grassLevelTransition.Evaluate(linearization) * height_scale * mountain_level;
        }
        else
        {
            float linearization = (noise - mountain_level) / (1 - mountain_level);
            cube_height += height_scale * sea_level + height_scale * mountain_level + Globals.instance.mountainLevelTransition.Evaluate(linearization) * height_scale;
        }

        return cube_height;
    }

    public void ClearMesh()
    {
        gameObject.GetComponent<MeshCollider>().sharedMesh.Clear();
        gameObject.GetComponent<MeshFilter>().mesh.Clear();
    }
}
