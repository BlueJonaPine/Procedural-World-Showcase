using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public float noiseValue = -1f;
    public float heightWorldCoordinates = -1f;
    public Vector3 topFaceMiddlePosition = Vector3.zero;
    public bool isOccupied = false;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    public Tile(float noise_value)
    {
        noiseValue = noise_value;
    }

    public void GenerateProceduralCube(Vector3 origin, Vector3 size, float uv_x)
    {
        heightWorldCoordinates = size.y;
        topFaceMiddlePosition = new Vector3(origin.x + size.x / 2, heightWorldCoordinates, origin.z + size.z / 2);

        List<Vector3> _vertices_temporary = new List<Vector3>();
        int face_count = 0;

        //float origin_offset = size / 2.0f;
        //origin = new Vector3(origin.x - origin_offset, origin.y - origin_offset, origin.z - origin_offset);

        _vertices_temporary.Add(new Vector3(0 * size.x, 0 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 0 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 1 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 1 * size.y, 1 * size.z) + origin);
        face_count++;
        _vertices_temporary.Add(new Vector3(1 * size.x, 0 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 0 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 1 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 1 * size.y, 0 * size.z) + origin);
        face_count++;
        _vertices_temporary.Add(new Vector3(0 * size.x, 0 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 0 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 0 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 0 * size.y, 0 * size.z) + origin);
        face_count++;
        _vertices_temporary.Add(new Vector3(0 * size.x, 1 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 1 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 1 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 1 * size.y, 1 * size.z) + origin);
        face_count++;
        _vertices_temporary.Add(new Vector3(0 * size.x, 0 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 0 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 1 * size.y, 0 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 1 * size.y, 0 * size.z) + origin);
        face_count++;
        _vertices_temporary.Add(new Vector3(1 * size.x, 0 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 0 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(0 * size.x, 1 * size.y, 1 * size.z) + origin);
        _vertices_temporary.Add(new Vector3(1 * size.x, 1 * size.y, 1 * size.z) + origin);
        face_count++;

        vertices = _vertices_temporary.ToArray();

        triangles = new int[face_count * 6];
        uvs = new Vector2[face_count * 4];
        int triangles_counter = 0;
        int uv_counter = 0;
        for (int i = 0; i < face_count; i++)
        {
            int triangleOffset = i * 4;
            triangles[triangles_counter++] = 0 + triangleOffset;
            triangles[triangles_counter++] = 2 + triangleOffset;
            triangles[triangles_counter++] = 1 + triangleOffset;

            triangles[triangles_counter++] = 0 + triangleOffset;
            triangles[triangles_counter++] = 3 + triangleOffset;
            triangles[triangles_counter++] = 2 + triangleOffset;

            // same uvs for all faces
            uvs[uv_counter++] = new Vector2(uv_x, 0);
            uvs[uv_counter++] = new Vector2(uv_x, 0);
            uvs[uv_counter++] = new Vector2(uv_x, 1);
            uvs[uv_counter++] = new Vector2(uv_x, 1);
        }
    }

    public Mesh GetOptimizedProceduralCubeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

    public void ModifyUVs(float uv_x)
    {
        int uv_counter = 0;
        while(uv_counter < uvs.Length)
        {
            uvs[uv_counter++].x = uv_x;
            uvs[uv_counter++].x = uv_x;
            uvs[uv_counter++].x = uv_x;
            uvs[uv_counter++].x = uv_x;
        }
    }
}
