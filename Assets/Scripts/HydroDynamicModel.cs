using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydroDynamicModel
{

    public virtual void InitializeModel(List<float> paramList, List<string> fileList)
    {
        Debug.Log("Initialize Model");
    }

    public virtual void RunningOneStep()
    {
        Debug.Log("Model Running One-Step");
    }

    public virtual void UpdateWaterMeshHeight() 
    {
        Debug.Log("Update Water Mesh Height");
    }

    public virtual RenderTexture UpdateVelocityTexture()
    {
        Debug.Log("Update Velocity Texture");
        return null;
    }

    public virtual RenderTexture UpdateWaterHeightTexture()
    {
        Debug.Log("Update Water Height Texture");
        return null;
    }

    public virtual Vector3[] GetWaterHeightVertices()
    {
        Debug.Log("Get Water Height Vertices");
        return null;
    }

    public virtual float[] GetWaterHeight() 
    {
        Debug.Log("Get Water Height");
        return null;
    }

    public virtual Vector2[] GetWaterVelocity()
    {
        Debug.Log("Get Water Velocity");
        return null;
    }
}
