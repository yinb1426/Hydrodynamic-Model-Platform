using NativeFileBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ModelController : MonoBehaviour
{

    private bool isRunning = false;
    private TerrainSurfaceLocalGenerator terrainGenerator;
    private WaterSurfaceLocalGenerator waterGenerator;

    void Start()
    {
        terrainGenerator = GameObject.Find("Terrain").GetComponent<TerrainSurfaceLocalGenerator>();
        waterGenerator = GameObject.Find("Water").GetComponent<WaterSurfaceLocalGenerator>();
    }

    public void StartGenerating(string modelName, List<float> paramsList, List<string> fileList, List<int> drawingParamsList)
    {
        isRunning = true;
        terrainGenerator.StartGenerating(fileList[0]);
        List<int> meshParamsList = terrainGenerator.GetMeshParams();
        waterGenerator.StartGenerating(modelName, paramsList, fileList, meshParamsList, drawingParamsList);
    }

    public void SetTerrainRenderingParams(string type, string filePath)
    {
        terrainGenerator.SetRenderingMaterial(type, filePath);
    }

    public void FinishRunning()
    {
        if(isRunning)
        {
            isRunning = false;
            terrainGenerator.FinishRunning();
            waterGenerator.FinishRunning();
        }
    }

    public void ExportFiles()
    {
        if (waterGenerator.GetRunningStatus())
        {
            var title = "Open File";
            var path = StandaloneFileBrowser.OpenFolderPanel(title);
            string outFilePath = path[0] + "\\";

            List<float[]> waterHeightList = waterGenerator.GetWaterHeightList();
            List<Vector2[]> waterVelocityList = waterGenerator.GetWaterVelocityList();
            List<double> metaDataList = terrainGenerator.GetMetaDataList();
            int meshResolution = terrainGenerator.GetMeshResolution();

            for (int i = 0; i < waterHeightList.Count; i++)
            {
                List<float>[] raster = VertexDataCreator.GetRasterWaterHeightAndVelocity(waterHeightList[i], waterVelocityList[i], metaDataList, meshResolution);
                WriteRasterFile(outFilePath + i.ToString() + ".txt", raster);
            }
        }
        else
            Debug.Log("Can't Export Files!");
    }

    private void WriteRasterFile(string filePath, List<float>[] raster)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("X\tY\tDepth\tVx\tVy\t");
                
                foreach(List<float> line in raster)
                {
                    string content = string.Empty;
                    foreach(float value in line)
                    {
                        content += value.ToString() + "\t";
                    }
                    writer.WriteLine(content);
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Write Raster File Error: " + e.Message);
        }
    }
}
