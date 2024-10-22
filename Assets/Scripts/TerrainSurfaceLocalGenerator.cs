using OSGeo.GDAL;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainSurfaceLocalGenerator : MonoBehaviour
{
    // 地形Mesh，默认为正方形
    private Mesh mesh;

    // Mesh顶点属性数量
    private readonly int vertexAttributeCount = 2; // vertex, uv
  
    // Mesh属性
    private int vertexCount = 0;         // 顶点总数量
    private int triangleIndexCount = 0;  // 三角形总数量
    private int meshResolution = 0;      // Mesh总边数
    private int meshWidth = 0;           // Mesh总宽度
    private List<double> metaDataList = null;

    private float[] terrainHeight = null;

    // Start is called before the first frame update
    void Start()
    {
        // Register GDAL
        Gdal.AllRegister();
    }

    public void StartGenerating(string fileName)
    {
        // 读取文件
        Dataset ds = Gdal.Open(fileName, Access.GA_ReadOnly);
        Band demBand = ds.GetRasterBand(1);

        // 获取相关数据
        vertexCount = ds.RasterXSize * ds.RasterYSize;
        meshResolution = ds.RasterXSize - 1;
        triangleIndexCount = meshResolution * meshResolution * 2 * 3;

        double[] metaData = new double[6];
        ds.GetGeoTransform(metaData);
        meshWidth = meshResolution * (int)metaData[1];

        metaDataList = new()
        {
            metaData[0],
            metaData[3],
            metaData[1],
            metaData[5]
        };

        // 创建地形高度数组，并将栅格数据导入进数组中
        float[] tmpTerrainHeight = new float[vertexCount];
        demBand.ReadRaster(0, 0, ds.RasterXSize, ds.RasterYSize, tmpTerrainHeight, ds.RasterXSize, ds.RasterYSize, 0, 0);
        terrainHeight = VertexDataCreator.TransformHeightMap(tmpTerrainHeight, meshResolution);

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
        VertexDataCreator.CreateVertexData(ref positions, terrainHeight, meshWidth, meshResolution);

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
            name = "Terrain Mesh"
        };
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.RecalculateNormals();
    }

    public void FinishRunning()
    {
        vertexCount = 0;
        triangleIndexCount = 0;
        meshResolution = 0;
        meshWidth = 0;
        DestroyMesh();
    }

    public List<int> GetMeshParams()
    {
        List<int> paramsList = new()
        {
            vertexCount,
            triangleIndexCount,
            meshResolution,
            meshWidth
        };
        return paramsList;
    }

    public List<double> GetMetaDataList() { return metaDataList; }

    public int GetMeshResolution() { return meshResolution; }

    private void DestroyMesh()
    {
        metaDataList = null;
        terrainHeight = null;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Destroy(meshFilter.mesh);
            meshFilter.mesh = null;
        }
        Destroy(mesh);
    }
}
