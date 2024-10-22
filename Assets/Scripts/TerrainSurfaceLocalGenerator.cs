using OSGeo.GDAL;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainSurfaceLocalGenerator : MonoBehaviour
{
    // ����Mesh��Ĭ��Ϊ������
    private Mesh mesh;

    // Mesh������������
    private readonly int vertexAttributeCount = 2; // vertex, uv
  
    // Mesh����
    private int vertexCount = 0;         // ����������
    private int triangleIndexCount = 0;  // ������������
    private int meshResolution = 0;      // Mesh�ܱ���
    private int meshWidth = 0;           // Mesh�ܿ��
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
        // ��ȡ�ļ�
        Dataset ds = Gdal.Open(fileName, Access.GA_ReadOnly);
        Band demBand = ds.GetRasterBand(1);

        // ��ȡ�������
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

        // �������θ߶����飬����դ�����ݵ����������
        float[] tmpTerrainHeight = new float[vertexCount];
        demBand.ReadRaster(0, 0, ds.RasterXSize, ds.RasterYSize, tmpTerrainHeight, ds.RasterXSize, ds.RasterYSize, 0, 0);
        terrainHeight = VertexDataCreator.TransformHeightMap(tmpTerrainHeight, meshResolution);

        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1); // ����һ���������б����б���ֻ����һ��������
        Mesh.MeshData meshData = meshDataArray[0]; // ��ȡ�������б��е������壬��Ϊֻ����һ��������ȡ[0]

        // ������������
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );

        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes); // ���������Ը���meshData
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
