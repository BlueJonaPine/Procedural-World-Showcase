using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudColor : MonoBehaviour
{
    Material cloudMaterial = null;

    float COLOR_ADAPTION_SPEED = 0.1f;

    void Start()
    {
        cloudMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        cloudMaterial.SetFloat(Shader.PropertyToID("_ColorStrength"), CloudController.instance.rainStrength);
    }

    void Update()
    {
        float color_strength = Mathf.Lerp(cloudMaterial.GetFloat(Shader.PropertyToID("_ColorStrength")), CloudController.instance.rainStrength, Time.deltaTime * COLOR_ADAPTION_SPEED);
        cloudMaterial.SetFloat(Shader.PropertyToID("_ColorStrength"), color_strength);
    }
}
