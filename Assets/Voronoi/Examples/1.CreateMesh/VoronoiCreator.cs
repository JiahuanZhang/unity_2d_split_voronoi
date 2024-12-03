using System.Collections.Generic;
using UnityEngine;

public class VoronoiCreator : MonoBehaviour
{
    public GameObject textPref;
    public Material material;

    [Tooltip("only for test generate logic")]
    public bool testLogic;

    [Header("Alpha Test Texture")]
    [Tooltip("if enable alpha test,transparent clip will be removed.")]
    public bool isAlphaTestEnable;
    [Tooltip("texture sampling for alpha test.You Should Enable 'Read/Write' and 'Alpha is transparency'!")]
    public Texture2D texture;

    [Header("Uv Data")]
    [Tooltip("mesh uv scale factor")]
    public Vector2 Uv = new Vector2(1, 1);

    [Header("Voronio Freature Points")]
    [Tooltip("will generate a new seed when seed <= 0")]
    public int Seed;
    [Tooltip("mesh size in position")]
    public Vector2 MeshSize = new Vector2(10, 10);
    [Tooltip("freature point count in x")]
    public int PointCountX = 10;
    [Tooltip("freature point count in y")]
    public int PointCountY = 10;
    [Tooltip("freature point offset factor in x")]
    [Range(0f, 1f)] public float MaxOffsetX = 0.5f;
    [Tooltip("freature point offset factor in y")]
    [Range(0f, 1f)] public float MaxOffsetY = 0.5f;

    [Header("View Options")]
    public bool drawVertex;
    public bool drawPoint;
    public bool drawPolygon;
    public bool drawCell;
    public bool drawText;

    private List<Vector2> featurePoints = new List<Vector2>();

    [HideInInspector] public List<Edge> trianglesEdgeList;
    [HideInInspector] public List<EdgeV> voronoiEdges;
    [HideInInspector] public List<Triangle> allTriangles;
    [HideInInspector] public Dictionary<long, CellVertex> vertexDic;
    [HideInInspector] public Dictionary<long, Cell> cellDic;
    [HideInInspector] public Dictionary<long, Cell> errorcellDic;

    [HideInInspector] public List<Mesh> meshList;
    [HideInInspector] public MeshGroupData meshGroupData;

    private bool isFirst = true;
    void Awake()
    {
        if (testLogic)
        {
            for (int i = 0; i < 10; i++)
            {
                StartDraw();
            }
        }
        else
        {
            isFirst = true;
            StartDraw();
        }
    }

    private void OnValidate()
    {
        //if (!Application.isPlaying) return;
        //StartDraw();
    }
    public void StartDraw()
    {
        var points = GetFreaturePoints();
        var v = new Voronoi();
        v.StartGenerate(points);

        featurePoints = points;
        trianglesEdgeList = v.trianglesEdgeList;
        voronoiEdges = v.voronoiEdges;
        allTriangles = v.allTriangles;
        vertexDic = v.vertexDic;
        cellDic = v.cellDic;
        errorcellDic = v.errorcellDic;

        CreateMeshes();
    }


    private List<Vector2> GetFreaturePoints()
    {
        var points = new List<Vector2>();
        Vector2 point;
        if (Seed <= 0)
        {
            System.Random seeder = new System.Random();
            Seed = seeder.Next();
        }
        Debug.Log($"Seed: " + Seed);
        System.Random rand = new System.Random(Seed);
        float spacingX = Uv.x / (PointCountX - 1), spacingY = Uv.y / (PointCountY - 1);
        float offsetX = Uv.x / 2, offsetY = Uv.y / 2;
        for (int i = 0; i < PointCountX; i++)
        {
            for (int j = 0; j < PointCountY; j++)
            {
                point.x = i * spacingX - offsetX + (float)rand.NextDouble() * spacingX * MaxOffsetX + 0.5f;
                point.y = j * spacingY - offsetY + (float)rand.NextDouble() * spacingY * MaxOffsetY + 0.5f;
                points.Add(point);
            }
        }
        points.Sort(new SiteSorterXY());
        return points;
    }

    private void LateUpdate()
    {
        if (testLogic) return;
        
        if (drawVertex)
        {
            for (int i = 0; i < trianglesEdgeList.Count; i++)
            {
                var start = new Vector3(trianglesEdgeList[i]._a.x * MeshSize.x, trianglesEdgeList[i]._a.y * MeshSize.y, 0);
                var end = new Vector3(trianglesEdgeList[i]._b.x * MeshSize.x, trianglesEdgeList[i]._b.y * MeshSize.y, 0);
                Debug.DrawLine(start, end, Color.red);
            }
        }
        
        if (drawPoint)
        {
            for (int i = 0; i < featurePoints.Count; i++)
            {
                var start = new Vector3(featurePoints[i].x * MeshSize.x, featurePoints[i].y * MeshSize.y, 0);
                var end = new Vector3((featurePoints[i].x + 3) * MeshSize.x, (featurePoints[i].y + 3) * MeshSize.y, 0);
                Debug.DrawLine(start, end, Color.white);
            }
        }
        
        if (drawPolygon)
        {
            for (int i = 0; i < voronoiEdges.Count; i++)
            {
                var start = new Vector3(voronoiEdges[i]._a.x * MeshSize.x, voronoiEdges[i]._a.y * MeshSize.y, 0);
                var end = new Vector3(voronoiEdges[i]._b.x * MeshSize.x, voronoiEdges[i]._b.y * MeshSize.y, 0);
                Debug.DrawLine(start, end, Color.blue);
            }
        }
        
        if (drawCell)
        {
            bool isError = errorcellDic.Count > 0;
            var dic = !isError ? cellDic : errorcellDic;
            foreach (var cell in dic.Values)
            {
                for (int i = 0; i < cell.edges.Count; i++)
                {
                    var edge = cell.edges[i];
                    var start = new Vector3(edge._a.x * MeshSize.x, edge._a.y * MeshSize.y, 1);
                    var end = new Vector3(edge._b.x * MeshSize.x, edge._b.y * MeshSize.y, 1);
                    Debug.DrawLine(start, end, Color.green);
                    if (isError && isFirst)
                    {
                        var go = GameObject.Instantiate(textPref, this.transform);
                        go.transform.position = (start + end) / 2;
                        go.GetComponent<TextMesh>().text = edge.InstanceID.ToString();
                        go.name = edge.InstanceID.ToString();
                    }
                }
            }

        }
        if (drawText && isFirst)
        {
            foreach (var v in vertexDic.Values)
            {
                var go = GameObject.Instantiate(textPref, this.transform);
                go.transform.position = new Vector3(v.Pos.x * MeshSize.x, v.Pos.y * MeshSize.y);
                go.GetComponent<TextMesh>().text = v.instanceID.ToString();
                go.name = "v" + v.instanceID.ToString();
            }
            isFirst = false;
        }
    }


    void CreateMeshes()
    {
        if (testLogic) return;
        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_EmissionMap", texture);

        bool isError = errorcellDic.Count > 0;
        var dic = !isError ? cellDic : errorcellDic;
        meshGroupData = VoronoiMeshHelper.CreateMeshesWithTexture(dic, vertexDic, new Vector2(MeshSize.x, MeshSize.y), Seed, texture, true, Uv, MeshSize, PointCountX, PointCountY);

        foreach (var chipData in meshGroupData.ChipDatas)
        {

            var mesh = new Mesh();
            mesh.SetVertices(chipData.Vertices);
            mesh.SetUVs(0, chipData.Uvs);
            mesh.SetTriangles(chipData.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            //
            var go = new GameObject("m" + chipData.InstanceID);
            var rd = go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();
            rd.sharedMaterial = material;
            mf.mesh = mesh;
        }
    }
 
}
