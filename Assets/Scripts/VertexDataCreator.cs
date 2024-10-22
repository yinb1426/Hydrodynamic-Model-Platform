using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class VertexDataCreator
{
    public static float[] TransformHeightMap(float[] heightMap, int resolution)
    {
        float[] newHeightMap = new float[heightMap.Length];
        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                newHeightMap[z * (resolution + 1) + x] = heightMap[(resolution - z) * (resolution + 1) + x];
            }
        return newHeightMap;
    }
    public static void CreateVertexData(ref NativeArray<Vector3> positions, int meshWidth, int resolution)
    {
        float gridSize = meshWidth / (float)resolution;
        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                positions[z * (resolution + 1) + x] = new Vector3(gridSize * x, 0f, gridSize * z);
            }
    }
    public static void CreateVertexData(ref Vector3[] positions, int meshWidth, int resolution)
    {
        float gridSize = meshWidth / (float)resolution;
        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                positions[z * (resolution + 1) + x] = new Vector3(gridSize * x, 0f, gridSize * z);
            }
    }
    public static void CreateVertexData(ref NativeArray<Vector3> positions, float[] height, int meshWidth, int resolution)
    {
        float gridSize = meshWidth / (float)resolution;
        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                positions[z * (resolution + 1) + x] = new Vector3(gridSize * x, height[z * (resolution + 1) + x], gridSize * z);
            }
    }
    public static void CreateTexCoordsData(ref NativeArray<Vector2> texCoords, int resolution)
    {
        for (int y = 0; y <= resolution; ++y)
            for (int x = 0; x <= resolution; ++x)
            {
                texCoords[y * (resolution + 1) + x] = new Vector2(1f / resolution * x, 1f / resolution * y);
            }
    }
    public static void CreateTriangleIndices(ref NativeArray<int> triangleIndices, int resolution)
    {
        for (int y = 0; y < resolution; ++y)
            for (int x = 0; x < resolution; ++x)
            {
                triangleIndices[(y * resolution + x) * 6] = y * (resolution + 1) + x;
                triangleIndices[(y * resolution + x) * 6 + 1] = (y + 1) * (resolution + 1) + x;
                triangleIndices[(y * resolution + x) * 6 + 2] = y * (resolution + 1) + x + 1;
                triangleIndices[(y * resolution + x) * 6 + 3] = y * (resolution + 1) + x + 1;
                triangleIndices[(y * resolution + x) * 6 + 4] = (y + 1) * (resolution + 1) + x;
                triangleIndices[(y * resolution + x) * 6 + 5] = (y + 1) * (resolution + 1) + x + 1;
            }
    }
    public static Vector3[] GetLerpVertices(Vector3[] positionBefore, Vector3[] positionAfter, float lerpValue)
    {
        Vector3[] positionMiddle = new Vector3[positionAfter.Length];
        for(int i = 0; i < positionAfter.Length; i++)
        {
            positionMiddle[i] = positionBefore[i] * (1.0f - lerpValue) + positionAfter[i] * lerpValue;
        }
        return positionMiddle;
    }

    public static Vector3[] GetRasterWaterHeight(float[] positions, List<double> metaDataList, int resolution)
    {
        Vector3[] rasterVertices = new Vector3[positions.Length];
        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                rasterVertices[z * (resolution + 1) + x] = new Vector3((float)(metaDataList[0] + metaDataList[2] * x), (float)(metaDataList[1] + metaDataList[3] * z), positions[z * (resolution + 1) + x]);
            }
        return rasterVertices;
    }

    public static Vector4[] GetRasterWaterVelocity(Vector2[] velocity, List<double> metaDataList, int resolution)
    {
        Vector4[] rasterVertices = new Vector4[velocity.Length];
        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                rasterVertices[z * (resolution + 1) + x] = new Vector4((float)(metaDataList[0] + metaDataList[2] * x), (float)(metaDataList[1] + metaDataList[3] * z), velocity[z * (resolution + 1) + x].x, velocity[z * (resolution + 1) + x].y);
            }
        return rasterVertices;
    }

    public static List<float>[] GetRasterWaterHeightAndVelocity(float[] positions, Vector2[] velocity, List<double> metaDataList, int resolution)
    {
        List<float>[] rasterVertices = new List<float>[velocity.Length];
        float startX = (float)metaDataList[0], deltaX = (float)metaDataList[2];
        float startY = (float)(metaDataList[1] + resolution * metaDataList[3]), deltaY = Mathf.Abs((float)metaDataList[3]);

        for (int z = 0; z <= resolution; ++z)
            for (int x = 0; x <= resolution; ++x)
            {
                rasterVertices[z * (resolution + 1) + x] = new List<float>()
                {
                    (float)(startX + deltaX * x), (float)(startY + deltaY * z),
                    positions[z * (resolution + 1) + x], velocity[z * (resolution + 1) + x].x, velocity[z * (resolution + 1) + x].y
                };
            }
        return rasterVertices;
    }
}
