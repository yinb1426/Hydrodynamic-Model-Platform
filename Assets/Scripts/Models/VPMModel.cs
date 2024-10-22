using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using UnityEngine;

public class VPMModel : HydroDynamicModel
{
    private ComputeShader computeShader;

    private int vertexCount = 0;         // 顶点总数量
    private int meshResolution = 0;      // Mesh总边数
    private int meshWidth = 0;           // Mesh总宽度

    private float deltaT = 0.0f;
    private float pipelineLength = 0.0f;
    private float gravity = 0.0f;
    private float Ke = 0.0f;

    private float[] terrainHeight;
    private float[] waterHeight;
    private float[] rainfallRate;
    private Vector4[] outputFlow;
    private Vector4[] newOutputFlow;
    private Vector2[] waterVelocity;

    private ComputeBuffer positionsBuffer;
    private ComputeBuffer waterHeightBuffer;
    private ComputeBuffer terrainHeightBuffer;
    private ComputeBuffer rainfallRateBuffer;
    private ComputeBuffer outputFlowBuffer;
    private ComputeBuffer newOutputFlowBuffer;
    private ComputeBuffer waterVelocityBuffer;

    private RenderTexture waterVelocityTexture;
    private RenderTexture waterHeightTexture;

    private int groupsX;
    private int groupsY;

    private int boundaryConditionKernelHandle;
    private int waterIncrementByRainfallKernelHandle;
    private int updateOutputFlowKernelHandle;
    private int updateNewOutputFlowKernelHandle;
    private int updateVelocityAndWaterHeightKernelHandle;
    //private int evaporationKernelHandle;
    private int updateWaterHeightWithTerrainKernelHandle;
    private int convertVelocityToTextureKernelHandle;
    private int convertHeightToTextureKernelHandle;

    readonly int
        waterHeightId = Shader.PropertyToID("waterHeight"),
        terrainHeightId = Shader.PropertyToID("terrainHeight"),
        rainfallRateId = Shader.PropertyToID("rainfallRate"),
        outputFlowId = Shader.PropertyToID("outputFlow"),
        newOutputFlowId = Shader.PropertyToID("newOutputFlow"),
        waterVelocityId = Shader.PropertyToID("waterVelocity"),
        positionsId = Shader.PropertyToID("positions"),
        waterVelocityTextureId = Shader.PropertyToID("waterVelocityTexture"),
        waterHeightTextureId = Shader.PropertyToID("waterHeightTexture"),
        sizeXId = Shader.PropertyToID("sizeX"),
        sizeYId = Shader.PropertyToID("sizeY"),
        deltaTId = Shader.PropertyToID("deltaT"),
        rateFactorId = Shader.PropertyToID("rateFactor"),
        gravityId = Shader.PropertyToID("gravity"),
        pipeLengthId = Shader.PropertyToID("pipeLength"),
        keId = Shader.PropertyToID("Ke");

    public override void InitializeModel(List<float> paramList, List<string> fileList)
    {
        // 初始化Compute Shader
        computeShader = Resources.Load<ComputeShader>("Models/ComputeShaders/VPMModel");

        // 常量赋值
        deltaT = paramList[0];
        pipelineLength = paramList[1];
        gravity = paramList[2];
        Ke = paramList[3];

        // 读取高度纹理
        Dataset ds = Gdal.Open(fileList[0], Access.GA_ReadOnly);
        Band demBand = ds.GetRasterBand(1);

        // 获取相关数据
        vertexCount = ds.RasterXSize * ds.RasterYSize;
        meshResolution = ds.RasterXSize - 1;

        double[] metaData = new double[6];
        ds.GetGeoTransform(metaData);
        meshWidth = meshResolution * (int)metaData[1];

        // 创建地形高度数组，并将栅格数据导入进数组中
        float[] tmpTerrainHeight = new float[vertexCount];
        demBand.ReadRaster(0, 0, ds.RasterXSize, ds.RasterYSize, tmpTerrainHeight, ds.RasterXSize, ds.RasterYSize, 0, 0);
        terrainHeight = VertexDataCreator.TransformHeightMap(tmpTerrainHeight, meshResolution);

        // 创建速度纹理
        waterVelocityTexture = new RenderTexture(meshResolution + 1, meshResolution + 1, 0, RenderTextureFormat.RGFloat);
        waterVelocityTexture.enableRandomWrite = true;

        // 创建深度纹理
        waterHeightTexture = new RenderTexture(meshResolution + 1, meshResolution + 1, 0, RenderTextureFormat.RFloat);
        waterHeightTexture.enableRandomWrite = true;

        // 加载计算着色器所需要的数组
        waterHeight = new float[vertexCount];
        Array.Fill(waterHeight, 0.0f);

        rainfallRate = new float[vertexCount];
        Array.Fill(rainfallRate, 0.2f);

        outputFlow = new Vector4[vertexCount];
        Array.Fill(outputFlow, Vector4.zero);

        newOutputFlow = new Vector4[vertexCount];
        Array.Fill(newOutputFlow, Vector4.zero);

        waterVelocity = new Vector2[vertexCount];
        Array.Fill(waterVelocity, Vector2.zero);

        groupsX = Mathf.CeilToInt((meshResolution + 1) / 32f);
        groupsY = Mathf.CeilToInt((meshResolution + 1) / 16f);

        // 初始化ComputeBuffer
        Vector3[] positionsArray = new Vector3[vertexCount];
        VertexDataCreator.CreateVertexData(ref positionsArray, meshWidth, meshResolution);
        positionsBuffer = new ComputeBuffer(vertexCount, 3 * sizeof(float));
        positionsBuffer.SetData(positionsArray);

        terrainHeightBuffer = new ComputeBuffer(vertexCount, sizeof(float));
        terrainHeightBuffer.SetData(terrainHeight);

        waterHeightBuffer = new ComputeBuffer(vertexCount, sizeof(float));
        waterHeightBuffer.SetData(waterHeight);

        rainfallRateBuffer = new ComputeBuffer(vertexCount, sizeof(float));
        rainfallRateBuffer.SetData(rainfallRate);

        outputFlowBuffer = new ComputeBuffer(vertexCount, 4 * sizeof(float));
        outputFlowBuffer.SetData(outputFlow);

        newOutputFlowBuffer = new ComputeBuffer(vertexCount, 4 * sizeof(float));
        newOutputFlowBuffer.SetData(newOutputFlow);

        waterVelocityBuffer = new ComputeBuffer(vertexCount, 2 * sizeof(float));
        waterVelocityBuffer.SetData(waterVelocity);

        // 设置常量
        computeShader.SetInt(sizeXId, meshResolution + 1);
        computeShader.SetInt(sizeYId, meshResolution + 1);
        computeShader.SetFloat(deltaTId, deltaT);
        computeShader.SetFloat(rateFactorId, 60.0f);
        computeShader.SetFloat(gravityId, gravity);
        computeShader.SetFloat(pipeLengthId, pipelineLength);
        computeShader.SetFloat(keId, Ke);

        boundaryConditionKernelHandle = computeShader.FindKernel("BoundaryCondition");
        computeShader.SetBuffer(boundaryConditionKernelHandle, waterHeightId, waterHeightBuffer);

        waterIncrementByRainfallKernelHandle = computeShader.FindKernel("WaterIncrementByRainfall");
        computeShader.SetBuffer(waterIncrementByRainfallKernelHandle, waterHeightId, waterHeightBuffer);
        computeShader.SetBuffer(waterIncrementByRainfallKernelHandle, rainfallRateId, rainfallRateBuffer);

        updateOutputFlowKernelHandle = computeShader.FindKernel("UpdateOutputFlow");
        computeShader.SetBuffer(updateOutputFlowKernelHandle, terrainHeightId, terrainHeightBuffer);
        computeShader.SetBuffer(updateOutputFlowKernelHandle, waterHeightId, waterHeightBuffer);
        computeShader.SetBuffer(updateOutputFlowKernelHandle, outputFlowId, outputFlowBuffer);
        computeShader.SetBuffer(updateOutputFlowKernelHandle, newOutputFlowId, newOutputFlowBuffer);

        updateNewOutputFlowKernelHandle = computeShader.FindKernel("UpdateNewOutputFlow");
        computeShader.SetBuffer(updateNewOutputFlowKernelHandle, outputFlowId, outputFlowBuffer);
        computeShader.SetBuffer(updateNewOutputFlowKernelHandle, newOutputFlowId, newOutputFlowBuffer);

        updateVelocityAndWaterHeightKernelHandle = computeShader.FindKernel("UpdateVelocityAndWaterHeight");
        computeShader.SetBuffer(updateVelocityAndWaterHeightKernelHandle, waterHeightId, waterHeightBuffer);
        computeShader.SetBuffer(updateVelocityAndWaterHeightKernelHandle, outputFlowId, outputFlowBuffer);
        computeShader.SetBuffer(updateVelocityAndWaterHeightKernelHandle, waterVelocityId, waterVelocityBuffer);

        //evaporationKernelHandle = computeShader.FindKernel("Evaporation");
        //computeShader.SetBuffer(evaporationKernelHandle, waterHeightId, waterHeightBuffer);

        updateWaterHeightWithTerrainKernelHandle = computeShader.FindKernel("UpdateWaterHeightWithTerrain");
        computeShader.SetBuffer(updateWaterHeightWithTerrainKernelHandle, waterHeightId, waterHeightBuffer);
        computeShader.SetBuffer(updateWaterHeightWithTerrainKernelHandle, terrainHeightId, terrainHeightBuffer);
        computeShader.SetBuffer(updateWaterHeightWithTerrainKernelHandle, positionsId, positionsBuffer);

        convertVelocityToTextureKernelHandle = computeShader.FindKernel("ConvertVelocityToTexture");
        computeShader.SetTexture(convertVelocityToTextureKernelHandle, waterVelocityTextureId, waterVelocityTexture);
        computeShader.SetBuffer(convertVelocityToTextureKernelHandle, waterVelocityId, waterVelocityBuffer);

        convertHeightToTextureKernelHandle = computeShader.FindKernel("ConvertHeightToTexture");
        computeShader.SetTexture(convertHeightToTextureKernelHandle, waterHeightTextureId, waterHeightTexture);
        computeShader.SetBuffer(convertHeightToTextureKernelHandle, waterHeightId, waterHeightBuffer);
    }

    public override void RunningOneStep()
    {
        computeShader.Dispatch(boundaryConditionKernelHandle, groupsX, groupsY, 1);
        computeShader.Dispatch(waterIncrementByRainfallKernelHandle, groupsX, groupsY, 1);
        computeShader.Dispatch(updateOutputFlowKernelHandle, groupsX, groupsY, 1);
        computeShader.Dispatch(updateNewOutputFlowKernelHandle, groupsX, groupsY, 1);
        computeShader.Dispatch(updateVelocityAndWaterHeightKernelHandle, groupsX, groupsY, 1);
    }

    public override void UpdateWaterMeshHeight()
    {
        computeShader.Dispatch(updateWaterHeightWithTerrainKernelHandle, groupsX, groupsY, 1);
    }

    public override RenderTexture UpdateVelocityTexture()
    {
        computeShader.Dispatch(convertVelocityToTextureKernelHandle, groupsX, groupsY, 1);
        RenderTexture newTexture = new(meshResolution + 1, meshResolution + 1, 0, RenderTextureFormat.RGFloat)
        {
            enableRandomWrite = true
        };
        Graphics.Blit(waterVelocityTexture, newTexture);
        return newTexture;
    }

    public override RenderTexture UpdateWaterHeightTexture()
    {
        computeShader.Dispatch(convertHeightToTextureKernelHandle, groupsX, groupsY, 1);
        RenderTexture newTexture = new(meshResolution + 1, meshResolution + 1, 0, RenderTextureFormat.RFloat)
        {
            enableRandomWrite = true
        };
        Graphics.Blit(waterHeightTexture, newTexture);

        return newTexture;
    }

    public override Vector3[] GetWaterHeightVertices()
    {
        Vector3[] newVertices = new Vector3[vertexCount];
        positionsBuffer.GetData(newVertices);
        return newVertices;
    }

    public override float[] GetWaterHeight()
    {
        float[] newWaterHeight = new float[vertexCount];
        waterHeightBuffer.GetData(newWaterHeight);
        return newWaterHeight;
    }

    public override Vector2[] GetWaterVelocity()
    {
        Vector2[] newWaterVelocity = new Vector2[vertexCount];
        waterVelocityBuffer.GetData(newWaterVelocity);
        return newWaterVelocity;
    }
}
