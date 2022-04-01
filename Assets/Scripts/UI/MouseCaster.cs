using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCaster
{
    public Vector2 clickPointV2 = Vector2.zero;
    public Vector3 clickPointV3 = Vector3.zero;
    public bool layerIsHit = false;

    public void CastFromMousePosition(int[] layers, bool layer_exception, float distance_to_detect)
    {
        int layerMask = 0;

        // Bit shift the index of the layer to get a bit mask
        for (int i = 0; i < layers.Length; i++)
            layerMask += 1 << layers[i];

        // This would cast rays only against colliders specified in given layer

        // collide against everything except given layer 8.
        if (layer_exception)
            layerMask = ~layerMask;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance_to_detect, layerMask))
        {
            clickPointV3.Set(hit.point.x, hit.point.y, hit.point.z);
            clickPointV2.Set(hit.point.x, hit.point.z);
            layerIsHit = true;
        }
        else
        {
            layerIsHit = false;
        }
    }
}
