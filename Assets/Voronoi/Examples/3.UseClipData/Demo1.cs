using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Demo1 : MonoBehaviour
{
    public Material foodMat;
    public TextAsset meshData;
    public bool isHide;
    private Material mat;
    private bool canMove = false;

    private List<Demo1Clip> foodClips = new();
    void Start()
    {
        if (meshData != null)
        {
            var data = JsonUtility.FromJson(meshData.text, typeof(MeshGroupData)) as MeshGroupData;
            CreateMeshes(data);
        }
    }
    public void SetMove(bool move)
    {
        canMove = move;
    }

    private void Update()
    {
        if (!canMove) return;
        foreach (var clip in foodClips)
        {
            clip.UpdatePos(Time.deltaTime);
        }
    }

    public void SetMat(Texture text)
    {
        mat = new Material(foodMat);
        mat.SetTexture("_BaseMap", text);
        mat.SetTexture("_EmissionMap", text);
    }

    public void CreateMeshes(MeshGroupData data)
    {
        for (int i = 0; i < data.ChipDatas.Count; i++)
        {
            var chipData = data.ChipDatas[i];
            var mesh = new Mesh();

            mesh.SetVertices(chipData.Vertices);
            mesh.SetUVs(0, chipData.Uvs);
            mesh.SetTriangles(chipData.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //
            var go = new GameObject($"chip_" + i);
            go.transform.parent = transform;
            var clip = go.AddComponent<Demo1Clip>();
            var rd = go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();
            rd.sharedMaterial = mat;
            mf.mesh = mesh;

            foodClips.Add(clip);
        }
    }

}
