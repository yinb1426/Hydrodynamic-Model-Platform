using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterSurfaceLocalGenerator : MonoBehaviour
{
    public Material _Material;
    public UIDocument bottomBarDocument;

    // 底栏的文本
    private Label textStep;

    private bool isRunning = false;
    private bool isDrawing = false;

    private int savingStep = 0;
    private int endingStep = 0;
    private int drawingStep = 0;

    private Mesh mesh;

    // Mesh顶点属性数量
    private const int vertexAttributeCount = 2; // vertex, uv

    // Mesh属性
    private int vertexCount = 0;         // 顶点总数量
    private int triangleIndexCount = 0;  // 三角形总数量
    private int meshResolution = 0;      // Mesh总边数
    private int meshWidth = 0;           // Mesh总宽度

    private int curRunningStep = 0;
    private int curDrawingStep = 0;
    private int totalDrawingStep = 0;

    private HydroDynamicModel model = null;

    private List<float[]> waterHeightList = null;
    private List<Vector2[]> waterVelocityList = null;

    private Queue<Vector3[]> verticesQueue = null;
    private Queue<RenderTexture> waterVelocityQueue = null;
    private Queue<RenderTexture> waterHeightQueue = null;

    private Vector3[] verticesBefore = null, verticesAfter = null;
    private RenderTexture waterVelocityBefore = null, waterVelocityAfter = null;
    private RenderTexture waterHeightBefore = null, waterHeightAfter = null;

    void Start()
    {
        VisualElement bottomBarVE = bottomBarDocument.rootVisualElement;
        textStep = bottomBarVE.Q<Label>(UIConstants.STEP_LABEL_NAME);
    }

    // Update is called once per frame
    void Update()
    {
        if(isRunning)
        {
            if(curRunningStep == 0)
            {
                textStep.text = "Start Running!";
                GetComponent<MeshRenderer>().enabled = false;
            }
            if (curRunningStep % savingStep == 0)
            {
                textStep.text = "Current Running Step: " + curRunningStep.ToString();

                model.UpdateWaterMeshHeight();
                
                Vector3[] newVertices = model.GetWaterHeightVertices();
                float[] newWaterHeight = model.GetWaterHeight();
                Vector2[] newWaterVelocity = model.GetWaterVelocity();

                RenderTexture waterVelocityTexture = model.UpdateVelocityTexture();
                RenderTexture waterHeightTexture = model.UpdateWaterHeightTexture();

                waterHeightList.Add(newWaterHeight);
                waterVelocityList.Add(newWaterVelocity);

                verticesQueue.Enqueue(newVertices);
                waterVelocityQueue.Enqueue(waterVelocityTexture);
                waterHeightQueue.Enqueue(waterHeightTexture);

                if(curRunningStep == endingStep)
                {
                    isRunning = false;
                    isDrawing = true;
                }
            }

            model.RunningOneStep();
            curRunningStep++;
        }

        if(isDrawing)
        {
            if(curDrawingStep == 0)
            {
                textStep.text = "Start Drawing!";
                GetComponent<MeshRenderer>().enabled = true;
                verticesAfter = verticesQueue.Dequeue();
                waterVelocityAfter = waterVelocityQueue.Dequeue();
                waterHeightAfter = waterHeightQueue.Dequeue();
                totalDrawingStep = verticesQueue.Count * drawingStep;
            }
            textStep.text = "Current Drawing Step: " + curDrawingStep.ToString();
            if (curDrawingStep % drawingStep == 0 && curDrawingStep != totalDrawingStep)
            {
                verticesBefore = verticesAfter;
                waterVelocityBefore = waterVelocityAfter;
                waterHeightBefore = waterHeightAfter;
                verticesAfter = verticesQueue.Dequeue();
                waterVelocityAfter = waterVelocityQueue.Dequeue();
                waterHeightAfter = waterHeightQueue.Dequeue();
            }
            if (curDrawingStep < totalDrawingStep)
            {
                float lerpValue = curDrawingStep % drawingStep / (float)drawingStep;

                Vector3[] verticesMiddle = VertexDataCreator.GetLerpVertices(verticesBefore, verticesAfter, lerpValue);
                mesh.vertices = verticesMiddle;
                mesh.RecalculateNormals();

                _Material.SetTexture("_FlowMapBefore", waterVelocityBefore);
                _Material.SetTexture("_FlowMapAfter", waterVelocityAfter);
                _Material.SetTexture("_WaterHeightTextureBefore", waterHeightBefore);
                _Material.SetTexture("_WaterHeightTextureAfter", waterHeightAfter);
                _Material.SetFloat("_TimeStep", lerpValue);

                curDrawingStep++;
            }
        }
    }

    private void SetMeshParams(List<int> paramsList)
    {
        vertexCount = paramsList[0];
        triangleIndexCount = paramsList[1];
        meshResolution = paramsList[2];
        meshWidth = paramsList[3];
    }

    private void SetDrawingParams(List<int> drawingParamsList)
    {
        savingStep = drawingParamsList[0];
        endingStep = drawingParamsList[1];
        drawingStep = drawingParamsList[2];
    }

    public void StartGenerating(string modelName, List<float> paramsList, List<string> fileList, List<int> meshParamsList, List<int> drawingParamsList)
    {
        SetMeshParams(meshParamsList);
        SetDrawingParams(drawingParamsList);

        Type modelType = Type.GetType(modelName + "Model");
        if (modelType != null)
            model = (HydroDynamicModel)Activator.CreateInstance(modelType);
        else throw new Exception(modelName + "doesn't exist!");

        model.InitializeModel(paramsList, fileList);

        // 创建Mesh顶点数据
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1); // 创建一个网格体列表，该列表中只创建一个网格体
        Mesh.MeshData meshData = meshDataArray[0]; // 获取网格体列表中的网格体，因为只创建一个，所以取[0]

        // 创建顶点属性
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );

        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes); // 将顶点属性赋给meshData
        vertexAttributes.Dispose();

        NativeArray<Vector3> positions = meshData.GetVertexData<Vector3>();
        VertexDataCreator.CreateVertexData(ref positions, meshWidth, meshResolution);

        NativeArray<Vector2> texCoords = meshData.GetVertexData<Vector2>(1);
        VertexDataCreator.CreateTexCoordsData(ref texCoords, meshResolution);

        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt32);
        NativeArray<int> triangleIndices = meshData.GetIndexData<int>();
        VertexDataCreator.CreateTriangleIndices(ref triangleIndices, meshResolution);

        var bounds = new Bounds(new Vector3(meshWidth / 2f, 0.0f, meshWidth / 2f), new Vector3(meshWidth * 2f, meshWidth * 2f, meshWidth * 2f));

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        mesh = new Mesh
        {
            bounds = bounds,
            name = "Water Mesh"
        };
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.RecalculateNormals();

        waterHeightList = new();
        waterVelocityList = new();

        // 创建所需要的队列
        verticesQueue = new();
        waterVelocityQueue = new();
        waterHeightQueue = new();

        // 开始运行
        isRunning = true;
    }

    public List<float[]> GetWaterHeightList() { return waterHeightList; }
    public List<Vector2[]> GetWaterVelocityList() { return waterVelocityList; }
    public bool GetRunningStatus() { return isDrawing; }

    public void FinishRunning()
    {
        isRunning = false;
        isDrawing = false;

        savingStep = 0;
        endingStep = 0;
        drawingStep = 0;

        vertexCount = 0;
        triangleIndexCount = 0;
        meshResolution = 0;
        meshWidth = 0;
        curRunningStep = 0;
        curDrawingStep = 0;
        totalDrawingStep = 0;

        model = null;
        waterHeightList = null;
        waterVelocityList = null;
        verticesQueue = null;
        waterVelocityQueue = null;
        waterHeightQueue = null;

        verticesBefore = null;
        verticesAfter = null;
        waterVelocityBefore = null;
        waterVelocityAfter = null;
        waterHeightBefore = null;
        waterHeightAfter = null;

        DestroyMesh();
    }

    private void DestroyMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Destroy(meshFilter.mesh);
            meshFilter.mesh = null;
        }
        Destroy(mesh);
    }

}
