using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VoronoiModifier : MonoBehaviour
{
    public static VoronoiModifier Instance;

    public Transform meshRoot, vertexRoot;
    public GameObject goPref;
    public Material material;
    public TextAsset textAsset;
    public bool ModifyMesh, ModifyVertex;
    public long drawClipID;

    [HideInInspector] public MeshGroupData meshGroupData;

    private Dictionary<long, MeshClip> meshDic = new();
    private Dictionary<Vector3, VertexModifier> vertexModifiers = new();
    void Awake()
    {
        Instance = this;

#if UNITY_EDITOR
        CreateSome();
#endif
    }

    private void LateUpdate()
    {
        foreach (MeshChipData cell in meshGroupData.ChipDatas)
        {
            if (drawClipID > 0 && drawClipID != cell.InstanceID) continue;
            var edges = cell.GetOutLineEdge();
            for (int i = 0; i < edges.Count; i++)
            {
                Debug.DrawLine(cell.Vertices[edges[i].X], cell.Vertices[edges[i].Y], Color.green);
            }
        }
    }

#if UNITY_EDITOR
    private void CreateSome()
    {
        // load mesh data 
        var data = JsonUtility.FromJson(textAsset.text, typeof(MeshGroupData)) as MeshGroupData;
        meshGroupData = data;

        // modify mat 
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(data.Texture);
        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_EmissionMap", texture);

        //
        InitGameject();
        InitOptPattenView();
    }
#endif
    public void ChangeOptPatten()
    {
        ModifyMesh = !ModifyMesh;
        ModifyVertex = !ModifyMesh;
        InitGameject();
        InitOptPattenView();
    }

    void InitGameject()
    {
        if (ModifyMesh)
        {
            meshDic.Clear();
            for (int i = meshRoot.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(meshRoot.transform.GetChild(i).gameObject);
            }
            CreateMeshes(meshGroupData);
        }
        else
        {
            vertexModifiers.Clear();
            for (int i = vertexRoot.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(vertexRoot.transform.GetChild(i).gameObject);
            }
            CreateVertexies(meshGroupData);
            CreateBG(meshGroupData);
        }
    }
    void InitOptPattenView()
    {
        if (ModifyMesh)
        {
            meshRoot.gameObject.SetActive(true);
            vertexRoot.gameObject.SetActive(false);
        }
        else
        {
            meshRoot.gameObject.SetActive(false);
            vertexRoot.gameObject.SetActive(true);
        }
    }

    private void CreateVertexies(MeshGroupData data)
    {
        for (int i = 0; i < data.ChipDatas.Count; i++)
        {
            var chipData = data.ChipDatas[i];
            for (int j = 0; j < chipData.Vertices.Length; j++)
            {
                var vertexPos = chipData.Vertices[j];
                var vm = CreateVertex(vertexPos);
                vm.CellVertexInfos.Add((chipData.InstanceID, j));
            }
        }
        VertexModifier CreateVertex(Vector3 v)
        {
            if (!vertexModifiers.TryGetValue(v, out var vm))
            {
                var go = GameObject.Instantiate(goPref, vertexRoot);
                int index = vertexModifiers.Count;
                go.name = index.ToString();
                vm = go.AddComponent<VertexModifier>();
                vm.Init(v, index);
                vertexModifiers.Add(v, vm);
            }
            return vm;
        }
    }

    public void MoveVertex(List<(long, int)> verteices, Vector3 newPos)
    {
        foreach (var (chipId, index) in verteices)
        {
            var chip = meshGroupData.ChipDatas.Find(e => e.InstanceID == chipId);
            chip.Vertices[index] = newPos;
            var newUv = Vector2.zero;
            newUv.x = newPos.x / meshGroupData.MeshSize.x;
            newUv.y = newPos.y / meshGroupData.MeshSize.y;
            chip.Uvs[index] = newUv;
        }
    }

    public void DelteClip(long instanceID)
    {
        var index = meshGroupData.ChipDatas.FindIndex(e => e.InstanceID == instanceID);
        meshGroupData.ChipDatas.RemoveAt(index);
    }

    public void CombinMeshes(long[] ids)
    {
        var mc = meshGroupData.CombineMeshes(ids);

        for (var i = 1; i < ids.Length; i++)
        {
            GameObject.Destroy(meshDic[ids[i]].gameObject);
            meshDic.Remove(ids[i]);
        }
        GameObject.Destroy(meshDic[mc.InstanceID].gameObject);
        meshDic.Remove(mc.InstanceID);
        CreateMeshClip(mc, true);
        Debug.Log("Combine mesh success!!");
    }

    void CreateMeshes(MeshGroupData data)
    {
        for (int i = 0; i < data.ChipDatas.Count; i++)
        {
            var chipData = data.ChipDatas[i];
            CreateMeshClip(chipData, true);
        }
    }
    private void CreateMeshClip(MeshChipData chipData, bool addComponent)
    {
        //
        var go = new GameObject();
        go.transform.parent = addComponent ? meshRoot : vertexRoot;
        var mc = go.AddComponent<MeshClip>();
        mc.Init(chipData, material);
        if (addComponent)
        {
            meshDic.Add(chipData.InstanceID, mc);
        }
        else
        {
            GameObject.Destroy(mc);
        }
    }

    private void CreateBG(MeshGroupData data)
    {
        for (int i = 0; i < data.ChipDatas.Count; i++)
        {
            CreateMeshClip(data.ChipDatas[i], false);
        }
    }


#if UNITY_EDITOR
    public void SaveMeshInEditor()
    {

        if (Application.isPlaying)
        {
            var path = AssetDatabase.GetAssetPath(this.textAsset);
            using (var sw = System.IO.File.CreateText(path))
            {
                var data = this.meshGroupData;
                var json = JsonUtility.ToJson(data);
                sw.Write(json);
                sw.Flush();
            }
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Can Only Save In Playing！！");
        }
    }
#endif
}