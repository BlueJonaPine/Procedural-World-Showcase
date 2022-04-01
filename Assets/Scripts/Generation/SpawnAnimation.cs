using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAnimation : MonoBehaviour
{
    Vector2Int coordinates;
    [SerializeField] bool isTerrainTile = false;

    float Y_ENDPOSITION;
    [SerializeField] float ANIMATION_SPEED = 8f;
    const float Y_OFFSET = 0.1f;
    const float TILE_ANIMATION_SPEED = 7f;

    // Start is called before the first frame update
    void Start()
    {
        if (TerrainCreator.instance.terrainGenerationIsAnimated)
        {
            string[] name = gameObject.name.Split('_');
            int x = Int32.Parse(name[name.Length - 2]);
            int y = Int32.Parse(name[name.Length - 1]);
            coordinates = new Vector2Int(x, y);

            if (isTerrainTile)
            {
                Y_ENDPOSITION = 0;
                StartCoroutine(AnimateTileGeneration());
            }
            else
            {
                Y_ENDPOSITION = TerrainCreator.instance.combinedMesh.tiles[coordinates.x, coordinates.y].heightWorldCoordinates;
                StartCoroutine(AnimateGeneration());
            }
        }
    }

    IEnumerator AnimateGeneration()
    {
        while (transform.position.y > Y_ENDPOSITION + Y_OFFSET)
        {
            float distance = transform.position.y - Y_ENDPOSITION;
            float speed_adaption = Mathf.Clamp(5f / distance, 1, 3);

            float y_pos = Mathf.Lerp(transform.position.y, Y_ENDPOSITION, speed_adaption * ANIMATION_SPEED * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, y_pos, transform.position.z);

            yield return null;
        }

        transform.position = new Vector3(transform.position.x, Y_ENDPOSITION, transform.position.z);

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    IEnumerator AnimateTileGeneration()
    {
        while (transform.position.y < Y_ENDPOSITION - Y_OFFSET)
        {
            float distance = transform.position.y - Y_ENDPOSITION;
            float speed_adaption = Mathf.Clamp(2f / distance, 1, 4);

            float y_pos = Mathf.Lerp(transform.position.y, Y_ENDPOSITION, speed_adaption * TILE_ANIMATION_SPEED * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, y_pos, transform.position.z);

            yield return null;
        }

        transform.position = new Vector3(transform.position.x, Y_ENDPOSITION, transform.position.z);

        yield return new WaitForSeconds(0.5f);

        Destroy(transform.gameObject);
    }
}
